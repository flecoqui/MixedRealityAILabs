
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


public class Translator : MonoBehaviour {

    public static Translator instance;
    string translationTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
    string translationTextEndpoint = "https://api.microsofttranslator.com/v2/http.svc/Translate?";
    private const string ocpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key" ;
    // Sobstitute the value of authorizationKey with your own Key 
    private const string authorizationKey = "6ad0f6d409a6406bb7176552fcc54666";
    private string authorizationToken;

    private enum Languages
    {
        en = 0, fr, it, ja, ko
    };
    private Languages from = Languages.en;
    private Languages to = Languages.it;

    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start () {
        StartCoroutine("GetTokenCoroutine", authorizationKey);
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    /// Request a Token from Azure Translation Ser•v'ice by providing the access key. 
    /// Debugging result is delivered to the Results class. 
    private IEnumerator GetTokenCoroutine(string key) {
        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException("Authorization key not set.");
        WWWForm form = new WWWForm();
        Debug.Log("Url: " + translationTokenEndpoint + " key: " + key);
        using (UnityWebRequest www = UnityWebRequest.Post(translationTokenEndpoint, form))
        {



            www.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
            // The download handler is responsible for bringing back the token after the request 
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            authorizationToken = www.downloadHandler.text;
            if (www.isNetworkError || www.isHttpError)
            {
                Results.instance.azureResponseText.text = www.error;
            }
            long responseCode = www.responseCode;

            Results.instance.SetAzureResponse(responseCode.ToString());
        }
        // After receiving the token, begin capturing Audio with the Class 
        MicrophoneManager.instance.StartCapturingAudio();
        StopCoroutine("GetTokenCoroutine");
        yield return null;
    }

    public IEnumerator TranslateWithUnityNetworking(string text)
    {
        WWWForm form = new WWWForm();
        string result;
        string queryString;
        Debug.Log("Translation From: " + from.ToString() + " value: " + ((int)from).ToString());
        Debug.Log("To: " + to.ToString() + " value: " + ((int)to).ToString());
        queryString = string.Concat("text=",Uri.EscapeDataString(text),"&from=",from.ToString(), "&to=",to.ToString());
        using (UnityWebRequest www = UnityWebRequest.Get(translationTextEndpoint + queryString))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + authorizationToken);
            www.SetRequestHeader("Accept","application/xml");

            yield return www.SendWebRequest();
            string s = www.downloadHandler.text;

            DataContractSerializer serializer = new DataContractSerializer(typeof(string));

            using (Stream stream = GenerateStreamFromString(s))
            {
                Results.instance.SetTranslatedResult((string)serializer.ReadObject(stream));

            }
            if ( www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            StopCoroutine("TranslateWithUnityNetworking");
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
}
