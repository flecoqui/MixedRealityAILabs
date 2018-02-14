using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;



public class MicrophoneManager : MonoBehaviour {


    public static MicrophoneManager instance;
#if WINDOWS_UWP_FAKE
    SpeechToTextClient client;
    ulong maxSize = 3840000;
    System.UInt16 level = 300;
    System.UInt16 duration = 1000;
            string[] LanguageArray = 
            {"ca-ES","de-DE","zh-TW", "zh-HK","ru-RU","es-ES", "ja-JP","ar-EG", "da-DK","en-AU" ,"en-CA","en-GB" ,"en-IN", "en-US" , "en-NZ","es-MX","fi-FI",
              "fr-FR","fr-CA" ,"it-IT","ko-KR" , "nb-NO","nl-NL","pt-BR" ,"pt-PT"  ,             
              "pl-PL"  ,"sv-SE", "zh-CN"  };
#endif

    int frequency = 44100;
    AudioSource audioSource;
    bool microphoneDetected;
    bool isCapturingAudio;
    DictationRecognizer dictationRecognizer;
    private void Awake()
    {
        instance = this;
#if WINDOWS_UWP_FAKE
        // Create Cognitive Service SpeechToText Client
        client = new SpeechToTextClient();
#endif
    }
    // Use this for initialization

    void Start () {




    }
    public
#if WINDOWS_UWP_FAKE
    async
#endif
    void PrepareCapturingAudio()
    {

        if (Microphone.devices.Length > 0)
        {
            Results.instance.SetMicrophoneStatus("Initializing...");
#if WINDOWS_UWP_FAKE

            client.SetAPI("speech.platform.bing.com", (string)"detailed");
            //string s = await client.GetToken("05fd1c63460b41968412723e6b7bb2ce");
            //if (!string.IsNullOrEmpty(s))
            //    Debug.Log("Getting Token successful Token: " + s.ToString());
            //else
            //    Debug.Log("Getting Token failed for subscription Key: " + "05fd1c63460b41968412723e6b7bb2ce");
#endif

            audioSource = GetComponent<AudioSource>();
            microphoneDetected = true;
        }
        else
        {
            Results.instance.SetMicrophoneStatus("No microphone detected");

        }


    }

    public
#if WINDOWS_UWP_FAKE
        async
#endif
        void StartCapturingAudio()
    {
        Debug.Log("Start Capturing Audio");
        if (microphoneDetected)
        {
            isCapturingAudio = true;
            audioSource.clip = Microphone.Start(null,true,30, frequency);
            audioSource.loop = true;
            audioSource.Play();


#if WINDOWS_UWP_FAKE
            if ((!client.HasToken()) && (!string.IsNullOrEmpty("05fd1c63460b41968412723e6b7bb2ce")))
            {
                Debug.Log("Getting Token for subscription key: " + "05fd1c63460b41968412723e6b7bb2ce");
                string token = await client.GetToken("05fd1c63460b41968412723e6b7bb2ce");
                if (!string.IsNullOrEmpty(token))
                {
                    Debug.Log("Getting Token successful Token: " + token.ToString());

                }
            }
            if (client.HasToken())
            {
                if (client.IsRecording() == false)
                {
                    if (await client.CleanupRecording())
                    {

                        if (await client.StartContinuousRecording(maxSize, duration, level))
                        {
                         //   isRecordingContinuously = true;
                            client.BufferReady += Client_BufferReady;
                            client.AudioLevel += Client_AudioLevel;
                            client.AudioCaptureError += Client_AudioCaptureError;
                            Debug.Log("Start Recording...");
                        }
                        else
                            Debug.Log("Start Recording failed");
                    }
                    else
                        Debug.Log("CleanupRecording failed");
                }
                else
                {
                    Debug.Log("Stop Recording...");
                    await client.StopRecording();
                    client.BufferReady -= Client_BufferReady;
                    client.AudioLevel -= Client_AudioLevel;
                    client.AudioCaptureError -= Client_AudioCaptureError;

                }
            }
#else
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
            dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;
            dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;
            dictationRecognizer.DictationError += DictationRecognizer_DictationError;

            dictationRecognizer.Start();
#endif

            Results.instance.SetMicrophoneStatus("Capturing...");
            Results.instance.SetDictationLanguageResult("en");
            Results.instance.SetTranslatedLanguageResult("it");
        }
    }
#if WINDOWS_UWP_FAKE
    private async void Client_AudioCaptureError(object sender, string message)
    {
        Debug.Log("Audio Capture Error: " + message);
        Debug.Log("Stop Recording...");
        await client.StopRecording();
    //    isRecordingInMemory = false;
        client.AudioLevel -= Client_AudioLevel;
        client.AudioCaptureError -= Client_AudioCaptureError;

    }
    string resultText = string.Empty;
     void Client_AudioLevel(object sender, double reading)
    {

    }
    private async void Client_BufferReady(object sender)
    {
      //  await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
      // async () =>
      // {
           SpeechToTextAudioStream stream;
           while ((stream = client.GetAudioStream()) != null)
           {
               string locale = "fr-FR";
               string resulttype = "detailed";
               double start = stream.startTime.TotalSeconds;
               double end = stream.endTime.TotalSeconds;
               Debug.Log("Sending Sub-Buffer: " + stream.Size.ToString() + " bytes for buffer from: " + start.ToString() + " seconds to: " + end.ToString() + " seconds");
               SpeechToTextResponse result = await client.SendAudioStream(locale, resulttype, stream);
               if (result != null)
               {
                   string httpError = result.GetHttpError();
                   if (!string.IsNullOrEmpty(httpError))
                   {
                       resultText = httpError;
                       Debug.Log("Http Error: " + httpError.ToString());

                   }
                   else
                   {
                       if (result.Status() == "error")
                       {
                           resultText = "error";

                       }
                       else
                       {
                           resultText = result.Result();

                       }
                       Debug.Log("Result for buffer from: " + start.ToString() + " seconds to: " + end.ToString() + " seconds duration : " + (end - start).ToString() + " seconds \r\n" + result.ToString());
                   }
               }
               else
                   Debug.Log("Error while sending buffer");
           }
      // });
    }
#endif
    public
#if WINDOWS_UWP_FAKE
        async
#endif
        void StopCapturingAudio()
    {
        Debug.Log("Stop Capturing Audio");
        Results.instance.SetMicrophoneStatus("Microphone sleeping");
        isCapturingAudio = false;
#if WINDOWS_UWP_FAKE
        await client.StopRecording();
#else
        Microphone.End(null);
        dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
        dictationRecognizer.DictationHypothesis -= DictationRecognizer_DictationHypothesis;
        dictationRecognizer.DictationComplete -= DictationRecognizer_DictationComplete;
        dictationRecognizer.DictationError -= DictationRecognizer_DictationError;
        dictationRecognizer.Dispose();
#endif
    }
    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidense)
    {
        Results.instance.SetDictationResult(text);

        StartCoroutine(Translator.instance.TranslateWithUnityNetworking(text, Results.instance.dictationLanguageResult, Results.instance.translationLanguageResult));
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

        

    }
    private void DictationRecognizer_DictationError(string error, int hresult)

    {
        Debug.Log("Dictation Error: " + error);

        StopCapturingAudio();

    }


}
