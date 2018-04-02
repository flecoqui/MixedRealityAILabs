using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Runtime.Serialization;
using System.Collections;


public class ProductPrediction : MonoBehaviour {
    /// <summary> 
    /// This object represents the Prediction request 
    /// It host the day of the year and hour of the day 
    /// The product must be left blank when serialising 
    /// </summary> 
    public class RootObject
    {
        public Inputs Inputs { get; set; }
    }
    public class Inputs
    {
        public Input1 input1 { get; set; }
    }
    public class Input1
    {
        public List<string> ColumnNames { get; set; }
        public List<List<string>> Values { get; set; }
    }


    /// <summary> 
    /// This object containing the deserialised Prediction result 
    /// It host the list of the products 
    /// and the likelihood of them being sold at current date and time 
    /// </summary> 
    public class Prediction
    {
        public Results Results { get; set; }
    }
    public class Results
    {
        public Output1 output1;
    }
    public class Output1
    {
        public string type;
        public Value value;
    }
    public class Value
    {
        public List<string> ColumnNames { get; set; }
        public List<List<string>> Values { get; set; }
    }

    /// <summary> 
    /// The 'Primary Key' from your Machine Learning Portal 
    /// </summary> 
    private string authKey = "-insert here your service auth Key-";          
    /// <summary> 
    /// The 'Request-Response' Service Endpoint from your Machine Learning Portal 
    /// </summary> 
    private string serviceEndpoint = "-insert here your service endpoint Key-";
    /// <summary> 
    /// The Hour as set in Windows 
    /// </summary> 
    private string thisHour;
    /// <summary> 
    /// The Day, as set in Windows 
    /// </summary> 
    private string thisDay;
    /// <summary> 
    /// The Month, as set in Windows 
    /// </summary> 
    private string thisMonth;
    /// <summary> 
    /// The Numeric Day from current Date Conversion 
    /// </summary> 
    private string dayOfTheYear;
    /// <summary> 
    /// Dictionary for holding the first (or default) provided prediction  
    /// from the Machine Learning Experiment 
    /// </summary>     
    private Dictionary<string, string> predictionDictionary;
    /// <summary> 
    /// List for holding product prediction with name and scores 
    /// </summary> 
    private List<KeyValuePair<string, double>> keyValueList;

    // Use this for initialization
    void Start()
    {
        // Call to get the current date and time as set in Windows 
        GetTodayDateAndTime();
        // Call to set the HOUR in the UI 
        ShelfKeeper.instance.SetTime(thisHour);
        // Call to set the DATE in the UI 
        ShelfKeeper.instance.SetDate(thisDay, thisMonth);
        // Run the method to Get Predication from Azure Machine Learning 
        StartCoroutine(GetPrediction(thisHour, dayOfTheYear));
    }
    /// <summary> 
    /// Get current date and hour 
    /// </summary> 
    private void GetTodayDateAndTime()
    {
        // Get today date and time 
        DateTime todayDate = DateTime.Now;
        // Extrapolate the HOUR 
        thisHour = todayDate.Hour.ToString();
        // Extrapolate the DATE 
        thisDay = todayDate.Day.ToString();
        thisMonth = todayDate.ToString("MMM");
        // Extrapolate the day of the year 
        dayOfTheYear = todayDate.DayOfYear.ToString();
    }
    private IEnumerator GetPrediction(string timeOfDay, string dayOfYear)
    {
        WWWForm form = new WWWForm();
        // Populate the request object  
        // Using current day of the year and hour of the day 
        RootObject ro = new RootObject();
        ro.Inputs = new Inputs();
        ro.Inputs.input1 = new Input1();
        ro.Inputs.input1.ColumnNames = new List<string>();
        ro.Inputs.input1.ColumnNames.Add("day");
        ro.Inputs.input1.ColumnNames.Add("hour");
        ro.Inputs.input1.ColumnNames.Add("product");
        ro.Inputs.input1.Values = new List<List<string>>();
        List<string> l = new List<string>();
        l.Add(dayOfYear);
        l.Add(timeOfDay);
        l.Add("");

        ro.Inputs.input1.Values.Add(l);
        Debug.LogFormat("Score request built");
        // Serialise the request 
        string json = JsonConvert.SerializeObject(ro); using (UnityWebRequest www = UnityWebRequest.Post(serviceEndpoint, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + authKey);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
            yield return www.SendWebRequest();
            string response = www.downloadHandler.text;
            // Deserialise the response 
            DataContractSerializer serializer;
            serializer = new DataContractSerializer(typeof(string));
            DeserialiseJsonResponse(response);
        }
    }


    /// Deserialise the response received from the Machine Learning portal 
    /// </summary> 
    public void DeserialiseJsonResponse(string jsonResponse)
    {
        // Deserialize JSON 
        Prediction prediction = JsonConvert.DeserializeObject<Prediction>(jsonResponse);
        predictionDictionary = new Dictionary<string, string>();
        for (int i = 0; i < prediction.Results.output1.value.ColumnNames.Count; i++)
        {
            if (prediction.Results.output1.value.Values[0][i] != null)
            {
                predictionDictionary.Add(prediction.Results.output1.value.ColumnNames[i],
                     prediction.Results.output1.value.Values[0][i]);
            }
        }
        keyValueList = new List<KeyValuePair<string, double>>();
        // Strip all non-results, by adding only items of interest to the scoreList 
        for (int i = 0; i < predictionDictionary.Count; i++)
        {
            KeyValuePair<string, string> pair = predictionDictionary.ElementAt(i);
            if (pair.Key.StartsWith("Scored Probabilities"))
            {
                // Parse string as double then simplify the string key so to only have the item name 
                double scorefloat = 0f;
                double.TryParse(pair.Value, out scorefloat);
                string simplifiedName =
                    pair.Key.Replace("\"", "").Replace("Scored Probabilities for Class", "").Trim();
                keyValueList.Add(new KeyValuePair<string, double>(simplifiedName, scorefloat));
            }
        }
        // Sort Predictions (results will be lowest to highest) 
        keyValueList.Sort((x, y) => y.Value.CompareTo(x.Value));
        // Spawn the top three items, from the keyValueList, which we have sorted 
        for (int i = 0; i < 3; i++)
        {
            ShelfKeeper.instance.SpawnProduct(keyValueList[i].Key, i);
        }
        // Clear lists in case of reuse 
        keyValueList.Clear();

        predictionDictionary.Clear();
    }


    // Update is called once per frame
    void Update () {
		
	}
}
