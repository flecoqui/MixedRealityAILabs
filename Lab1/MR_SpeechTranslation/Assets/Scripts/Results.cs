using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Results : MonoBehaviour {
    public static Results instance;
    [HideInInspector] public string azureResponsecode;
    [HideInInspector] public string translationResult;
    [HideInInspector] public string dictationResult;
    [HideInInspector] public string micStatus;

    public Text azureResponseText;
    public Text translationResultText;
    public Text dictationText;
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
    }
    public void SetMicrophoneStatus(string Result)
    {
        micStatus = Result;
        microphoneStatusText.text = micStatus;
    }
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
