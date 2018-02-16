using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class SpeechToText : MonoBehaviour {
    public static SpeechToText instance;
    // Audio Level
    // when the audio level is over this value, the audio samples will be recorded
    public uint selectionLevel = 100;
    // Period in Milliseconds to calculate the audio level
    public int selectionDurationMs = 800;
    // Duration of the input audio buffer
    public int clipDurationInSecond = 30;

    private string speechToTextTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
    private string hostnameString = "speech.platform.bing.com";
    private string apiString = "interactive";
    private string speechToTextEndpoint = "https://{0}/speech/recognition/{1}/cognitiveservices/v1?language={2}&format={3}";
    private const string ocpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";
    // Sobstitute the value of authorizationKey with your own Key 
    private const string speechToTextAuthorizationKey = "05fd1c63460b41968412723e6b7bb2ce";
    private string speechToTextAuthorizationToken;
    private bool IsMicrophoneDetected;
    private AudioClip clip;
    private bool isCapturingAudio;

    private int sampleRate = 16000;
    private int currentBuffer;
    private byte[][] bufferArray ;
    private int numberOfBuffer ;
    private int bufferSize ;



    private void Awake()
    {
        instance = this;
    }
    // Use this for initialization
    void Start()
    {
        StartCoroutine("GetSpeechToTextTokenCoroutine", speechToTextAuthorizationKey);
    }
    public void PrepareCapturingAudio()
    {

        if (Microphone.devices.Length > 0)
        {
            ParametersAndResults.instance.SetMicrophoneStatus("Initializing...");

            IsMicrophoneDetected = true;
        }
        else
        {
            ParametersAndResults.instance.SetMicrophoneStatus("No microphone detected");

        }

    }
    public void StartCapturingAudio()
    {
        Debug.Log("Start Capturing Audio");
        if (IsMicrophoneDetected)
        {
            numberOfBuffer = (clipDurationInSecond * 1000) / selectionDurationMs;
            bufferSize = (selectionDurationMs * sampleRate * 2) / 1000;
            // Creating buffers
            currentBuffer = 0;
            bufferArray = new byte[numberOfBuffer][];
            for(int i = 0; i<numberOfBuffer; i++)
            {
                bufferArray[i] = new byte[bufferSize];
            }


            clip = Microphone.Start(null, true, clipDurationInSecond, sampleRate);
            while (!(Microphone.GetPosition(null) > 0)) { }
            isCapturingAudio = true;
            ParametersAndResults.instance.SetMicrophoneStatus("Capturing...");

        }
    }

    private IEnumerator GetSpeechToTextTokenCoroutine(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new System.InvalidOperationException("Authorization key not set.");
        WWWForm form = new WWWForm();
        Debug.Log("Url: " + speechToTextTokenEndpoint + " key: " + key);
        using (UnityWebRequest www = UnityWebRequest.Post(speechToTextTokenEndpoint, form))
        {
            www.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
            // The download handler is responsible for bringing back the token after the request 
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            speechToTextAuthorizationToken = www.downloadHandler.text;
            if (www.isNetworkError || www.isHttpError)
            {
                ParametersAndResults.instance.azureResponseText.text = www.error;
            }
            long responseCode = www.responseCode;

            ParametersAndResults.instance.SetAzureResponse(responseCode.ToString());
        }
        // After receiving the token, begin capturing Audio with the Class 
        StopCoroutine("GetSpeechToTextTokenCoroutine");
        PrepareCapturingAudio();
        StartCapturingAudio();

        StartCoroutine("SpeechToTextWithUnityNetworking", ParametersAndResults.instance.GetLanguageString(ParametersAndResults.instance.inputLanguage));
        yield return null;
    }
    private IEnumerable<Int16> Decode(byte[] byteArray)
    {
        for (var i = 0; i < byteArray.Length - 1; i += 2)
        {
            yield return (BitConverter.ToInt16(byteArray, i));
        }
    }
 
    static bool GetInt(byte[] buffer, out int i)
    {
        i = 0;
        if ((buffer != null) && (buffer.Length >= 4))
        {
            i = BitConverter.ToInt32(buffer, 0);
            return true;
        }
        return false;
    }

    static byte[] CreateHeader(AudioClip clip, int Length = 0)
    {
        uint headerLen = 4 + 16 + 8 + 8 + 8;
        byte[] updatedBuffer = new byte[headerLen];
        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;
        uint Len = ( (Length == 0) ? (uint)clip.samples * 2 : (uint) (Length));

        if (updatedBuffer != null)
        {
            System.Text.UTF8Encoding.UTF8.GetBytes("RIFF").CopyTo(updatedBuffer, 0);
            BitConverter.GetBytes(4 + 16 + 8 + Len + 8).CopyTo(updatedBuffer, 4);
            System.Text.UTF8Encoding.UTF8.GetBytes("WAVE").CopyTo(updatedBuffer, 8);
            System.Text.UTF8Encoding.UTF8.GetBytes("fmt ").CopyTo(updatedBuffer, 12);
            BitConverter.GetBytes(16).CopyTo(updatedBuffer, 16);


            UInt16 one = 1;        
            BitConverter.GetBytes(one).CopyTo(updatedBuffer,20);
            //numChannels
            BitConverter.GetBytes(channels).CopyTo(updatedBuffer, 22); 
            //sampleRate = 
            BitConverter.GetBytes(hz).CopyTo(updatedBuffer, 24); 
            // byteRate = 
            BitConverter.GetBytes(hz * channels * 2).CopyTo(updatedBuffer, 28); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
            UInt16 blockAlign = (ushort)(channels * 2);
            BitConverter.GetBytes(blockAlign).CopyTo(updatedBuffer, 32);

            UInt16 bps = 16;
            //Byte[] bitsPerSample = 
            BitConverter.GetBytes(bps).CopyTo(updatedBuffer, 34);

            System.Text.UTF8Encoding.UTF8.GetBytes("data").CopyTo(updatedBuffer, 36);
            BitConverter.GetBytes(Len).CopyTo(updatedBuffer, 40);

        }
        return updatedBuffer;
    }



    static Byte[] ConvertClipToByteArray(AudioClip clip)
    {

        var samples = new float[clip.samples];

        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

        Byte[] bytesData = new Byte[samples.Length * 2];
        //bytesData array is twice the size of
        //dataSource array because a float converted in Int16 is 2 bytes.

        int rescaleFactor = 32767; //to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        return bytesData;
    }


    static Byte[] ConvertFloatArrayToByteArray(float[] inputBuffer)
    {

        Int16[] intData = new Int16[inputBuffer.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

        Byte[] bytesData = new Byte[inputBuffer.Length * 2];
        //bytesData array is twice the size of
        //dataSource array because a float converted in Int16 is 2 bytes.

        int rescaleFactor = 32767; //to convert float to Int16

        for (int i = 0; i < inputBuffer.Length; i++)
        {
            intData[i] = (short)(inputBuffer[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        return bytesData;
    }
    private IEnumerable<Int16> DecodeLevel(byte[] byteArray)
    {
        for (var i = 0; i < byteArray.Length - 1; i += 2)
        {
            yield return (BitConverter.ToInt16(byteArray, i));
        }
    }
    public IEnumerator SpeechToTextWithUnityNetworking(string language)
    {
        string hostname = hostnameString;
        string mode = apiString;

        Debug.Log("Speech To Text in: " + language);
        //  AudioClip clip = Microphone.Start(null, false,10,16000);
        if (clip == null)
            yield return null ;
        int writeIndex = 0;
        int readIndex = 0;
        int floatBufferSize = (selectionDurationMs * sampleRate ) / 1000;
        float[] floatBuffer = new float[floatBufferSize];
        while (true)
        {
            writeIndex = Microphone.GetPosition(null);
            if(writeIndex != readIndex)
            {
                int nFloatsToGet = (clip.samples + writeIndex - readIndex) % clip.samples;
                if (nFloatsToGet > floatBufferSize)
                {
                    clip.GetData(floatBuffer, readIndex);
                    readIndex = (readIndex + floatBufferSize) % clip.samples;

                    //Debug.Log("Capturing at Position: " + writeIndex.ToString());
                    byte[] data = ConvertFloatArrayToByteArray(floatBuffer);
                    var amplitude = DecodeLevel(data).Select(Math.Abs).Average(x => x);
                    // if the audio level sufficient to record those audio samples
                    if (amplitude > selectionLevel)
                    {
                        // Level sufficient copy the buffer
                        data.CopyTo(bufferArray[currentBuffer++], 0);
                    }

                    // Should we send the audio samples to SpeechToText services
                    if ((currentBuffer >= numberOfBuffer) ||
                        ((amplitude < selectionLevel) && (currentBuffer>0)))
                    {
                        
                        byte[] header = CreateHeader(clip, currentBuffer*bufferSize);
                        Debug.Log("Level sufficient sending the audio chunks" );
                        byte[] buffer = new byte[header.Length + currentBuffer * bufferSize];
                        header.CopyTo(buffer, 0);
                        for(int i = 0; i < currentBuffer;i++)
                            bufferArray[i].CopyTo(buffer, 44 + i*bufferSize );
                        currentBuffer = 0;
                        string resultType = "simple";  // choice "simple" or "detailed"
                                                       // mode interactive dictation conversation
                                                       //string[] LanguageArray =
                                                       // {"ca-ES","de-DE","zh-TW", "zh-HK","ru-RU","es-ES", "ja-JP","ar-EG", "da-DK","en-AU" ,"en-CA","en-GB" ,"en-IN", "en-US" , "en-NZ","es-MX","fi-FI",
                                                       //      "fr-FR","fr-CA" ,"it-IT","ko-KR" , "nb-NO","nl-NL","pt-BR" ,"pt-PT"  ,
                                                       //      "pl-PL"  ,"sv-SE", "zh-CN"  };
                        UploadHandlerRaw MyUploadHandler = new UploadHandlerRaw(buffer);
                        MyUploadHandler.contentType = "audio/wav; codec=\"audio/pcm\"; samplerate=16000";

                        using (UnityWebRequest www = new UnityWebRequest(string.Format(speechToTextEndpoint, hostname, mode, language, resultType), UnityWebRequest.kHttpVerbPOST, new DownloadHandlerBuffer(), MyUploadHandler))
                        {

                            www.useHttpContinue = true;
                            www.chunkedTransfer = true;
                            //  www.uploadHandler = MyUploadHandler;
                            //  www.downloadHandler = new DownloadHandlerBuffer();
                            www.SetRequestHeader("Authorization", "Bearer " + speechToTextAuthorizationToken);
                            www.SetRequestHeader("Content-Type", "audio/wav; codec=\"audio/pcm\"; samplerate=16000");
                            www.SetRequestHeader("Accept", "application/json;text/xml");
                            //     www.SetRequestHeader("Transfer-Encoding", "chunked");
                            //     www.SetRequestHeader("Expect", "100-continue");



                            // www.method = UnityWebRequest.kHttpVerbPOST;
                            yield return www.SendWebRequest();
                            string s = www.downloadHandler.text;
                            if (!string.IsNullOrEmpty(s))
                            {
                                Debug.Log("Response from Service: " + s);
                                if (!string.IsNullOrEmpty(s))
                                {
                                    char[] sep = { '{', '}', ',' };
                                    string[] values = s.Split(sep);
                                    if (values != null)
                                    {
                                        foreach (var val in values)
                                        {
                                            char[] locsep = { ':' };
                                            string[] value = val.Split(locsep);
                                            if ((value != null) && (value.Length == 2))
                                            {
                                                if (value[0] == "\"DisplayText\"")
                                                {
                                                    string text = value[1];
                                                    ParametersAndResults.instance.SetInputString(text);


                                                }
                                            }
                                        }
                                    }

                                }


                                if (www.isNetworkError || www.isHttpError)
                                {
                                    Debug.Log(www.error);
                                }
                            }

                        }
                    }
                
                }
                else
                {
                    // Debug.Log("Position: " + i.ToString());
                    yield return null;
                }
            }
            else
            {
                // Debug.Log("Position: " + i.ToString());
                yield return null;
            }
        }
      //  StopCoroutine("SpeechToTextWithUnityNetworking");
    }
}
