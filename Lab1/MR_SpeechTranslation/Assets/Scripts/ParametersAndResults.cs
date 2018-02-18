using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParametersAndResults : MonoBehaviour
{
    public static ParametersAndResults instance;
    [HideInInspector] public string azureResponsecode;
    [HideInInspector] public string outputString;
    [HideInInspector] public string inputString;
    [HideInInspector] public string micStatus;
    private string[] Languages = { "ar-eg", "de-de", "en-us", "es-es", "fr-fr", "it-it", "ja-jp", "pt-br", "ru-ru", "zh-cn" }; 

    public enum Language { ar, de, en, es, fr, it, ja, pt, ru, zh };
    public Language inputLanguage;
    public Language outputLanguage;
    public Text inputLanguageText;
    public Text outputLanguageText;
    public Text microphoneStatusText;
    public Text azureResponseText;
    public Text outputStringText;
    public Text inputStringText;


    private void Awake()
    {
        instance = this;
        inputLanguageText.text = inputLanguage.ToString();
        outputLanguageText.text = outputLanguage.ToString();
    }

    public void SetAzureResponse(string Result)
    {
        azureResponsecode = Result;
        azureResponseText.text = azureResponsecode;
    }
    public void SetInputString(string Result)
    {
        inputString = Result;
        inputStringText.text = Result;
        if (!string.IsNullOrEmpty(Result) && (Result != "??"))
        {
            StartCoroutine(Translator.instance.TranslateWithUnityNetworking(Result, ParametersAndResults.instance.inputLanguage.ToString(), ParametersAndResults.instance.outputLanguage.ToString()));
        }

    }
    public void SetOuputString(string Result)
    {
        outputString = Result;
        outputStringText.text = Result;
        if (!string.IsNullOrEmpty(Result) && (Result != "??"))
        {
            // launch SpeechToText
            StartCoroutine(TextToSpeech.instance.TextToSpeechWithUnityNetworking(Result, ParametersAndResults.instance.GetLanguageString(ParametersAndResults.instance.outputLanguage)));
        }
    }
    public void SetMicrophoneStatus(string Result)
    {
        micStatus = Result;
        microphoneStatusText.text = micStatus;
    }
    public string GetLanguageString(Language lang)
    {
        string result = "en-us";
        string inputLanguage = lang.ToString();
        foreach (string l in Languages)
        {
            if(l.StartsWith(inputLanguage))
            {
                result = l;
                break;
            }
        }
        return result;

    }
}
