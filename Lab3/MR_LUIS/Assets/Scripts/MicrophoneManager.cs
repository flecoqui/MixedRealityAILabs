using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
public class MicrophoneManager : MonoBehaviour {
    public static MicrophoneManager instance;
    int frequency = 44100;
    AudioSource audioSource;
    public TextMesh dictationText;

    DictationRecognizer dictationRecognizer;
    private void Awake()
    {
        instance = this;
    }
    // Use this for initialization
    void Start () {

        if (Microphone.devices.Length > 0)
        {
            
            audioSource = GetComponent<AudioSource>();
            StartCapturingAudio();
            Debug.Log("Mic Detected");


        }
        else
        {
            Debug.Log("Mic Not Detected");
        }


    }
	public void StartCapturingAudio()
    {
        


            audioSource.clip = Microphone.Start(null,true,30, frequency);
            audioSource.loop = true;
            audioSource.Play();

            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
            dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;
            dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;
            dictationRecognizer.DictationError += DictationRecognizer_DictationError;

            dictationRecognizer.Start();
            Debug.Log("Capturing Audio...");


    }

    public void StopCapturingAudio()
    {
        Debug.Log("Stop Capturing Audio");

        Microphone.End(null);
        dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
        dictationRecognizer.DictationHypothesis -= DictationRecognizer_DictationHypothesis;
        dictationRecognizer.DictationComplete -= DictationRecognizer_DictationComplete;
        dictationRecognizer.DictationError -= DictationRecognizer_DictationError;
        dictationRecognizer.Dispose();
    }
    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidense)
    {

        StartCoroutine(LuisManager.instance.SubmitRequestToLuis(text));
        Debug.Log("Dictation " + text);
        dictationText.text = text;
    }

    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)

    {

        // If Timeout occurs, the user has been silent for too long.

        // With dictation, the default timeout after a recognition is 20 seconds.

        // The default timeout with initial silence is 5 seconds.

        if (cause == DictationCompletionCause.TimeoutExceeded)
        {
            Debug.Log("Dictation Complete Timeout");
        }
        else
        {
            Debug.Log("Dictation complete: " + cause.ToString());
        }
        
        Debug.Log("Dictation Start");
        dictationRecognizer.Start();

    }

    private void DictationRecognizer_DictationHypothesis(string text)

    {

        // Set DictationDisplay text to be textSoFar and new hypothesized text

        // Currently unused
        Debug.Log("Dictation Hypothesis: " + text);
        Debug.Log("Dictation Start");
        

    }
    private void DictationRecognizer_DictationError(string error, int hresult)

    {
        Debug.Log("Dictation Error: " + error);

        StopCapturingAudio();

    }


}
