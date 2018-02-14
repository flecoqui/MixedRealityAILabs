using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Results : MonoBehaviour {
    public static Results instance;
    [HideInInspector] public string azureResponsecode;
    [HideInInspector] public string translationResult;
    [HideInInspector] public string translationLanguageResult;
    [HideInInspector] public string dictationResult;
    [HideInInspector] public string dictationLanguageResult;
    [HideInInspector] public string micStatus;

    public Text azureResponseText;
    public Text translationResultText;
    public Text translationLanguageResultText;
    public Text dictationText;
    public Text dictationLanguageText;
    public Text microphoneStatusText;

    private void Awake()
    {
        instance = this;
    }

    public void SetAzureResponse(string Result)
    {
        azureResponsecode = Result;
        azureResponseText.text = azureResponsecode;
    }
    public void SetDictationResult(string Result)
    {
        dictationResult = Result;
        dictationText.text = dictationResult;
    }
    public void SetTranslatedResult(string Result)
    {
        translationResult = Result;
        translationResultText.text = translationResult;
        if (!string.IsNullOrEmpty(Result) && (Result != "??"))
        {
            // launch the Text To Speech
            StartCoroutine(TextToSpeech.instance.TextToSpeechWithUnityNetworking(Result, translationLanguageResult));
        }
    }
    public void SetMicrophoneStatus(string Result)
    {
        micStatus = Result;
        microphoneStatusText.text = micStatus;
    }
    public void SetDictationLanguageResult(string Result)
    {
        dictationLanguageResult = Result;
        dictationLanguageText.text = dictationLanguageResult;
    }
    public void SetTranslatedLanguageResult(string Result)
    {
        translationLanguageResult = Result;
        translationLanguageResultText.text = translationLanguageResult;

    }
}
