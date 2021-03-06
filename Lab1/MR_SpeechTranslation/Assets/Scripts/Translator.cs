﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;


public class Translator : MonoBehaviour {
    public static Translator instance;
    string translationTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
    string translationTextEndpoint = "https://api.microsofttranslator.com/v2/http.svc/Translate?";
    private const string ocpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key" ;
    // Sobstitute the value of authorizationKey with your own Key 
    private const string authorizationKey = "f945042c15474984bcb00bc976cb5f29";
    private string authorizationToken;



    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start () {
        StartCoroutine("GetTokenCoroutine", authorizationKey);
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
                ParametersAndResults.instance.azureResponseText.text = www.error;
            }
            long responseCode = www.responseCode;
        }
        // After receiving the token, begin capturing Audio with the Class 
        StopCoroutine("GetTokenCoroutine");
        yield return null;
    }

    public IEnumerator TranslateWithUnityNetworking(string text, string from, string to)
    {


        string queryString;
        Debug.Log("Translation From: " + from.ToString() + " to: " + to );
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
                string translatedText = (string)serializer.ReadObject(stream);
                ParametersAndResults.instance.SetOuputString(translatedText);

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
