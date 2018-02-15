using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

/// <summary>
/// class Chunk
/// </summary>
/// <info>
/// Class used to read the Chunk in the WAV file header 
/// (for instance:"fmt ", "data", "JUNK" chunks).
/// </info>
class Chunk
{
    public byte[] tag;
    public uint length;
    public byte[] data;

    public Chunk()
    {
        tag = null;
        length = 0;
        data = null;
    }
    public Chunk(byte[] Tag, uint Length, byte[] Data)
    {
        if (Tag != null)
        {
            this.tag = new byte[Tag.Length];
            for (int i = 0; i < Tag.Length; i++)
                this.tag[i] = Tag[i];
        }
        else
            this.tag = Tag;

        this.length = Length;

        if (Data != null)
        {
            this.data = new byte[Data.Length];
            for (int j = 0; j < Data.Length; j++)
                this.data[j] = Data[j];
        }

    }
}
public class SpeechToText : MonoBehaviour {
    public static SpeechToText instance;
    string speechToTextTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
    private string hostnameString = "speech.platform.bing.com";
    private string apiString = "interactive";
    string speechToTextEndpoint = "https://{0}/speech/recognition/{1}/cognitiveservices/v1?language={2}&format={3}";
    private const string ocpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";
    // Sobstitute the value of authorizationKey with your own Key 
    private const string speechToTextAuthorizationKey = "05fd1c63460b41968412723e6b7bb2ce";
    private string speechToTextAuthorizationToken;
    bool microphoneDetected;
    AudioClip clip;
    bool isCapturingAudio;
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
            Results.instance.SetMicrophoneStatus("Initializing...");

            microphoneDetected = true;
        }
        else
        {
            Results.instance.SetMicrophoneStatus("No microphone detected");

        }

    }
    public void StartCapturingAudio()
    {
        Debug.Log("Start Capturing Audio");
        if (microphoneDetected)
        {
            
            clip = Microphone.Start(null, true, 5, 16000);
            while (!(Microphone.GetPosition(null) > 0)) { }
            isCapturingAudio = true;
            Results.instance.SetMicrophoneStatus("Capturing...");
            Results.instance.SetDictationLanguageResult("en");
            Results.instance.SetTranslatedLanguageResult("it");
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
                Results.instance.azureResponseText.text = www.error;
            }
            long responseCode = www.responseCode;

            Results.instance.SetAzureResponse(responseCode.ToString());
        }
        // After receiving the token, begin capturing Audio with the Class 
        StopCoroutine("GetSpeechToTextTokenCoroutine");
        PrepareCapturingAudio();
        StartCapturingAudio();

        StartCoroutine("SpeechToTextWithUnityNetworking", "en-US");
        yield return null;
    }
    private IEnumerable<Int16> Decode(byte[] byteArray)
    {
        for (var i = 0; i < byteArray.Length - 1; i += 2)
        {
            yield return (BitConverter.ToInt16(byteArray, i));
        }
    }
    //private uint ParseAndGetWAVHeaderLength(byte[] buffer)
    //{

    //    int length = 0;
    //    uint source = 0;
    //    if (IsTag(buffer, "RIFF") == true)
    //    {
    //        source += 4;
    //        if (GetInt(buffer.AsBuffer().ToArray(source, 4), out length) == true)
    //        {
    //            source += 4;
    //            if (IsTag(buffer.AsBuffer().ToArray(source, 4), "WAVE") == true)
    //            {
    //                source += 4;
    //                Chunk c = new Chunk();
    //                while ((source + 8 <= buffer.Length) && (ReadChunkHeader(buffer.AsBuffer().ToArray(source, 8), c) == true))
    //                {
    //                    source += 8;
    //                    if (IsTag(c.tag, "fmt ") == true)
    //                    {
    //                        fmt = new Chunk(c.tag, c.length, c.data);

    //                        if ((source + c.length < buffer.Length) && (ReadChunkData(buffer.AsBuffer().ToArray(source, (int)c.length), fmt) == true))
    //                        {
    //                            nChannels = BitConverter.ToUInt16(fmt.data, 2);
    //                            nSamplesPerSec = BitConverter.ToUInt32(fmt.data, 4);
    //                            nAvgBytesPerSec = BitConverter.ToUInt32(fmt.data, 8);
    //                            nBlockAlign = BitConverter.ToUInt16(fmt.data, 12);
    //                            wBitsPerSample = BitConverter.ToUInt16(fmt.data, 14);
    //                            thresholdDurationInBytes = (nAvgBytesPerSec * tresholdDuration) / 1000;
    //                            source += c.length;
    //                        }
    //                        else
    //                            return 0;
    //                    }
    //                    else if (IsTag(c.tag, "data") == true)
    //                    {
    //                        // Almost done
    //                        if (fmt.length > 0)
    //                        {
    //                            //source += 8;
    //                            data = new Chunk(c.tag, c.length, c.data);
    //                            break;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        source += c.length;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return source;
    //}
    //static bool ReadChunkHeader(byte[] buffer, Chunk c)
    //{
    //    if ((buffer != null) && (buffer.Length >= 8) && (c != null))
    //    {
    //        c.tag = new byte[4];
    //        buffer.CopyTo(0, c.tag.AsBuffer(), 0, 4);
    //        {
    //            byte[] array = new byte[4];
    //            buffer.CopyTo(4, array.AsBuffer(), 0, 4);
    //            c.length = BitConverter.ToUInt32(array, 0);
    //            if (c.length >= 0)
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}
    //static bool ReadChunkData(byte[] buffer, Chunk c)
    //{
    //    if ((buffer != null) && (buffer.Length >= 8) && (c != null))
    //    {
    //        c.data = new byte[c.length];
    //        buffer.CopyTo(0, c.data.AsBuffer(), 0, (int)c.length);
    //        return true;
    //    }
    //    return false;
    //}
    static bool ReadTag(System.IO.FileStream ifs, string tag)
    {
        if ((ifs != null) && (tag != null))
        {
            byte[] a = System.Text.UTF8Encoding.UTF8.GetBytes(tag);
            if (a != null)
            {
                byte[] buffer = new byte[a.Length];
                if (ifs.Read(buffer, 0, a.Length) == a.Length)
                {
                    for (int i = 0; i < a.Length; i++)
                    {
                        if (a[i] != buffer[i])
                            return false;
                    }
                    return true;
                };
            }
        }
        return false;
    }
    static bool IsTag(byte[] buffer, string tag)
    {
        if ((buffer != null) && (tag != null))
        {
            byte[] a = System.Text.UTF8Encoding.UTF8.GetBytes(tag);
            if (a != null)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] != buffer[i])
                        return false;
                }
                return true;
            }
        }
        return false;
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
    //public byte[] CreateWAVHeaderBuffer(uint Len)
    //{
    //    uint headerLen = 4 + fmt.length + 8 + 8 + 8;
    //    byte[] updatedBuffer = new byte[headerLen];
    //    if (updatedBuffer != null)
    //    {
    //        System.Text.UTF8Encoding.UTF8.GetBytes("RIFF").CopyTo(0, updatedBuffer.AsBuffer(), 0, 4);
    //        BitConverter.GetBytes(4 + fmt.length + 8 + Len + 8).CopyTo(0, updatedBuffer.AsBuffer(), 4, 4);
    //        System.Text.UTF8Encoding.UTF8.GetBytes("WAVE").CopyTo(0, updatedBuffer.AsBuffer(), 8, 4);
    //        System.Text.UTF8Encoding.UTF8.GetBytes("fmt ").CopyTo(0, updatedBuffer.AsBuffer(), 12, 4);
    //        BitConverter.GetBytes(fmt.length).CopyTo(0, updatedBuffer.AsBuffer(), 16, 4);
    //        fmt.data.CopyTo(0, updatedBuffer.AsBuffer(), 20, (int)fmt.length);
    //        System.Text.UTF8Encoding.UTF8.GetBytes("data").CopyTo(0, updatedBuffer.AsBuffer(), 20 + fmt.length, 4);
    //        BitConverter.GetBytes(Len).CopyTo(0, updatedBuffer.AsBuffer(), 24 + fmt.length, 4);
    //    }
    //    return updatedBuffer;
    //}
    static byte[] CreateHeader(AudioClip clip, int Length = 0)
    {
        uint headerLen = 4 + 16 + 8 + 8 + 8;
        byte[] updatedBuffer = new byte[headerLen];
        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;
        uint Len = ( (Length == 0) ? (uint)clip.samples * 2 : (uint) (Length*2));

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
    //public byte[] CreateWAVHeaderBuffer(uint Len)
    //{
    //    uint headerLen = 4 + fmt.length + 8 + 8 + 8;
    //    byte[] updatedBuffer = new byte[headerLen];
    //    if (updatedBuffer != null)
    //    {
    //        System.Text.UTF8Encoding.UTF8.GetBytes("RIFF").CopyTo(0, updatedBuffer.AsBuffer(), 0, 4);
    //        BitConverter.GetBytes(4 + fmt.length + 8 + Len + 8).CopyTo(0, updatedBuffer.AsBuffer(), 4, 4);
    //        System.Text.UTF8Encoding.UTF8.GetBytes("WAVE").CopyTo(0, updatedBuffer.AsBuffer(), 8, 4);
    //        System.Text.UTF8Encoding.UTF8.GetBytes("fmt ").CopyTo(0, updatedBuffer.AsBuffer(), 12, 4);
    //        BitConverter.GetBytes(fmt.length).CopyTo(0, updatedBuffer.AsBuffer(), 16, 4);
    //        fmt.data.CopyTo(0, updatedBuffer.AsBuffer(), 20, (int)fmt.length);
    //        System.Text.UTF8Encoding.UTF8.GetBytes("data").CopyTo(0, updatedBuffer.AsBuffer(), 20 + fmt.length, 4);
    //        BitConverter.GetBytes(Len).CopyTo(0, updatedBuffer.AsBuffer(), 24 + fmt.length, 4);
    //    }
    //    return updatedBuffer;
    //}


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
        int floatBufferSize = clip.samples / 2;
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

                    Debug.Log("Capturing at Position: " + writeIndex.ToString());
                    var hz = clip.frequency;
                    var channels = clip.channels;
                    var samples = clip.samples;
                    byte[] header = CreateHeader(clip, floatBufferSize);
                    byte[] data = ConvertFloatArrayToByteArray(floatBuffer);
                    var amplitude = DecodeLevel(data).Select(Math.Abs).Average(x => x);
                    if (amplitude > 100)
                    {
                        Debug.Log("Level sufficient sending the audio chunks" );
                        byte[] buffer = new byte[header.Length + data.Length];
                        header.CopyTo(buffer, 0);
                        data.CopyTo(buffer, 44);
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
                                                    Results.instance.SetDictationResult(text);

                                                    StartCoroutine(Translator.instance.TranslateWithUnityNetworking(text, Results.instance.dictationLanguageResult, Results.instance.translationLanguageResult));

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
