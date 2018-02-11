
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;


public class LuisManager : MonoBehaviour
{

    [System.Serializable]
    public class AnalysedQuery
    {
        public TopScoringIntentData topScoringIntent;
        public EntityData[] entities;
        public string query;
    }

    [System.Serializable]
    public class TopScoringIntentData
    {
        public string intent;
        public float score;

    }
    [System.Serializable]
    public class EntityData
    {
        public string entity;
        public string type;
        public int startIndex;
        public int endIndex;
        public float score;

    }


    public static LuisManager instance;

    // fbe92a643352443886070d47cd81cfb0
    // Europe
    string luisEndpoint = "https://northeurope.api.cognitive.microsoft.com/luis/v2.0/apps/03316aef-f078-4361-89f9-1fbae3791356?subscription-key=fbe92a643352443886070d47cd81cfb0&verbose=true&timezoneOffset=60&q=";
    //string luisEndpoint = "https://northeurope.api.cognitive.microsoft.com/luis/v2.0/apps/03316aef-f078-4361-89f9-1fbae3791356?subscription-key=fbe92a643352443886070d47cd81cfb0&verbose=true&timezoneOffset=60";
   // string luisEndpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/8241229d-92cf-4eda-ba7d-44b77f18f444?subscription-key=42b9e51aff974485a8cdf01929c52e9a&verbose=true&timezoneOffset=60&q=";


    private void Awake()
    {
        instance = this;
    }






    public IEnumerator SubmitRequestToLuis(string text)
    {
        WWWForm form = new WWWForm();

        string queryString;
        queryString = string.Concat(Uri.EscapeDataString(text));

        Debug.Log("LUIS analysing string: " + text);

        using (UnityWebRequest www = UnityWebRequest.Get(luisEndpoint + queryString))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();
            long responseCode = www.responseCode;
            string s = www.downloadHandler.text;

            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(string));

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    using (Stream stream = GenerateStreamFromString(www.downloadHandler.text))
                    {
                        StreamReader reader = new StreamReader(stream);
                        AnalysedQuery analysedQuery = new AnalysedQuery();
                        analysedQuery = JsonUtility.FromJson<AnalysedQuery>(www.downloadHandler.text);

                        AnalyseResponseElements(analysedQuery);

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Luis Exception" + ex.Message);
            }
            yield return null;
        }

    }
    public static Stream GenerateStreamFromString(string s)
    {
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
    private void AnalyseResponseElements(AnalysedQuery aQuery)
    {
        string topIntent = aQuery.topScoringIntent.intent;

        Dictionary<string, string> entityDic = new Dictionary<string, string>();

        foreach (EntityData ed in aQuery.entities)
        {
            entityDic.Add(ed.type, ed.entity);
        }
        switch (aQuery.topScoringIntent.intent)
        {
            case "ChangeObjectColor":
                string targetColor = null;
                string color = null;
                Debug.Log("LUIS Intent ChangeObjectColor");
                foreach (var pair in entityDic)
                {
                    if(pair.Key == "target")
                    {
                        targetColor = pair.Value;
                    }
                    else if (pair.Key == "color")
                    {
                        color = pair.Value;
                    }
                }
                Behaviours.instance.ChangeTargetColor(targetColor,color);
                break;
            case "ChangeObjectSize":
                string targetForSize = null;
                Debug.Log("LUIS Intent ChangeObjectSize");
                foreach (var pair in entityDic)
                {
                    if (pair.Key == "target")
                    {
                        targetForSize = pair.Value;
                    }
                }
                if (entityDic.ContainsKey("upsize") == true)
                {
                    Behaviours.instance.UpSizeTarget(targetForSize);
                }
                if (entityDic.ContainsKey("downsize") == true)
                {
                    Behaviours.instance.DownSizeTarget(targetForSize);
                }
                
                break;

            default:
                Debug.Log("LUIS Intent unknown");
                break;
        }
    }
}
