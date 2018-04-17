
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


public class TextToSpeech : MonoBehaviour
{
    private AudioSource audioSource;
    public static TextToSpeech instance;
    string textToSpeechTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
    string textToSpeechEndpoint = "https://speech.platform.bing.com/synthesize";
    private const string ocpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";
    // Sobstitute the value of authorizationKey with your own Key 
    private const string textToSpeechAuthorizationKey = "c7752a93be604b97a2beb44ac241d922";
    private string textToSpeechAuthorizationToken;


    private void Awake()
    {
        instance = this;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine("GetTextToSpeechTokenCoroutine", textToSpeechAuthorizationKey);
    }


    void Update()
    {
        if ((audioSource==null)||(!audioSource.isPlaying))
        {

            // When TextToSpeech is over, enable Audio Capture for SpeechToText
            SpeechToText.instance.EnableAudioCapture(true);

        }
        else
        {

            // Disable Audio Capture for SpeechToText
            SpeechToText.instance.EnableAudioCapture(false);
        }
    }



    private IEnumerator GetTextToSpeechTokenCoroutine(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException("Authorization key not set.");
        WWWForm form = new WWWForm();
        Debug.Log("Url: " + textToSpeechTokenEndpoint + " key: " + key);
        using (UnityWebRequest www = UnityWebRequest.Post(textToSpeechTokenEndpoint, form))
        {



            www.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
            // The download handler is responsible for bringing back the token after the request 
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            textToSpeechAuthorizationToken = www.downloadHandler.text;
            if (www.isNetworkError || www.isHttpError)
            {
                ParametersAndResults.instance.azureResponseText.text = www.error;
            }
            long responseCode = www.responseCode;

            ParametersAndResults.instance.SetAzureResponse(responseCode.ToString());
        }
        // After receiving the token, begin capturing Audio with the Class 
        StopCoroutine("GetTextToSpeechTokenCoroutine");
        yield return null;
    }
    string GetVoiceName(string lang, string gender)
    {
        string voiceName = "Microsoft Server Speech Text to Speech Voice (en-US, BenjaminRUS)";

        switch (lang.ToLower())
        {
            case "ar-eg":
                voiceName = "Microsoft Server Speech Text to Speech Voice (ar-EG, Hoda)";
                break;
            case "de-de":
                if (gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (de-DE, Hedda)";
                else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (de-DE, Stefan, Apollo)";
                break;
            case "en-au":
                voiceName = "Microsoft Server Speech Text to Speech Voice (en-AU, Catherine)";
                break;
            case "en-ca":
                voiceName = "Microsoft Server Speech Text to Speech Voice (en-CA, Linda)";
                break;
            case "en-gb":
                if (gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (en-GB, Susan, Apollo)";
                else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (en-GB, George, Apollo)";
                break;
            case "en-in":
                voiceName = "Microsoft Server Speech Text to Speech Voice (en-IN, Ravi, Apollo)";
                break;
            case "en-us":
                if (gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)";
                else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (en-US, BenjaminRUS)";
                break;
            case "es-es":
                if (gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (es-ES, Laura, Apollo)";
                else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (es-ES, Pablo, Apollo)";
                break;
            case "es-mx":
                voiceName = "Microsoft Server Speech Text to Speech Voice (es-MX, Raul, Apollo)";
                break;
            case "fr-ca":
                voiceName = "Microsoft Server Speech Text to Speech Voice (fr-CA, Caroline)";
                break;
            case "fr-fr":
                if (gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (fr-FR, Julie, Apollo)";
                else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (fr-FR, Paul, Apollo)";
                break;
            case "it-it":
                voiceName = "Microsoft Server Speech Text to Speech Voice (it-IT, Cosimo, Apollo)";
                break;
            case "ja-jp":
                if (gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (ja-JP, Ayumi, Apollo)";
                else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (ja-JP, Ichiro, Apollo)";
                break;
            case "pt-br":
                voiceName = "Microsoft Server Speech Text to Speech Voice (pt-BR, Daniel, Apollo)";
                break;
            case "ru-ru":
                if (gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (ru-RU, Irina, Apollo)";
                else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (ru-RU, Pavel, Apollo)";
                break;
            case "zh-cn":
                if (gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-CN, HuihuiRUS)";
                else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-CN, Kangkang, Apollo)";
                break;
            case "zh-hk":
                if (gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-HK, Tracy, Apollo)";
                else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-HK, Danny, Apollo)";
                break;
            case "zh-tw":
                if (gender == "Female")
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-TW, Yating, Apollo)";
                else
                    voiceName = "Microsoft Server Speech Text to Speech Voice (zh-TW, Zhiwei, Apollo)";
                break;
        }
        return voiceName;

    }
    string GetLanguage(string inputLanguage)
    {
        string output = "en-US";
        switch (inputLanguage)
        {
            case "en":
                output = "en-US";
                break;
            case "fr":
                output = "fr-FR";
                break;
            case "it":
                output = "it-IT";
                break;
            case "de":
                output = "de-DE";
                break;
            case "ja":
                output = "ja-JP";
                break;
        }
        return output;
    }
    private string GenerateSsml(string locale, string gender, string name, string text)
    {
        var ssmlDoc = new XDocument(
                          new XElement("speak",
                              new XAttribute("version", "1.0"),
                              new XAttribute(XNamespace.Xml + "lang", "en-US"),
                              new XElement("voice",
                                  new XAttribute(XNamespace.Xml + "lang", locale),
                                  new XAttribute(XNamespace.Xml + "gender", gender),
                                  new XAttribute("name", name),
                                  text)));
        return ssmlDoc.ToString();
    }
    /// <summary>
    /// Dynamically creates an <see cref="AudioClip"/> that represents raw Unity audio data.
    /// </summary>
    /// <param name="name"> The name of the dynamically generated clip.</param>
    /// <param name="audioData">Raw Unity audio data.</param>
    /// <param name="sampleCount">The number of samples in the audio data.</param>
    /// <param name="frequency">The frequency of the audio data.</param>
    /// <returns>The <see cref="AudioClip"/>.</returns>
    private static AudioClip ToClip(string name, float[] audioData, int sampleCount, int frequency)
    {
        var clip = AudioClip.Create(name, sampleCount, 1, frequency, false);
        clip.SetData(audioData, 0);
        return clip;
    }
    /// <summary>
    /// Converts two bytes to one float in the range -1 to 1.
    /// </summary>
    /// <param name="firstByte">The first byte.</param>
    /// <param name="secondByte"> The second byte.</param>
    /// <returns>The converted float.</returns>
    private static float BytesToFloat(byte firstByte, byte secondByte)
    {
        // Convert two bytes to one short (little endian)
        short s = (short)((secondByte << 8) | firstByte);

        // Convert to range from -1 to (just below) 1
        return s / 32768.0F;
    }

    /// <summary>
    /// Converts an array of bytes to an integer.
    /// </summary>
    /// <param name="bytes"> The byte array.</param>
    /// <param name="offset"> An offset to read from.</param>
    /// <returns>The converted int.</returns>
    private static int BytesToInt(byte[] bytes, int offset = 0)
    {
        int value = 0;
        for (int i = 0; i < 4; i++)
        {
            value |= ((int)bytes[offset + i]) << (i * 8);
        }
        return value;
    }
    /// <summary>
    /// Converts raw WAV data into Unity formatted audio data.
    /// </summary>
    /// <param name="wavAudio">The raw WAV data.</param>
    /// <param name="sampleCount">The number of samples in the audio data.</param>
    /// <param name="frequency">The frequency of the audio data.</param>
    /// <returns>The Unity formatted audio data. </returns>
    private static float[] ToUnityAudio(byte[] wavAudio, out int sampleCount, out int frequency)
    {
        // Determine if mono or stereo
        int channelCount = wavAudio[22];  // Speech audio data is always mono but read actual header value for processing

        // Get the frequency
        frequency = BytesToInt(wavAudio, 24);

        // Get past all the other sub chunks to get to the data subchunk:
        int pos = 12; // First subchunk ID from 12 to 16

        // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
        while (!(wavAudio[pos] == 100 && wavAudio[pos + 1] == 97 && wavAudio[pos + 2] == 116 && wavAudio[pos + 3] == 97))
        {
            pos += 4;
            int chunkSize = wavAudio[pos] + wavAudio[pos + 1] * 256 + wavAudio[pos + 2] * 65536 + wavAudio[pos + 3] * 16777216;
            pos += 4 + chunkSize;
        }
        pos += 8;

        // Pos is now positioned to start of actual sound data.
        sampleCount = (wavAudio.Length - pos) / 2;  // 2 bytes per sample (16 bit sound mono)
        if (channelCount == 2) { sampleCount /= 2; }  // 4 bytes per sample (16 bit stereo)

        // Allocate memory (supporting left channel only)
        var unityData = new float[sampleCount];

        // Write to double array/s:
        int i = 0;
        while (pos < wavAudio.Length)
        {
            unityData[i] = BytesToFloat(wavAudio[pos], wavAudio[pos + 1]);
            pos += 2;
            if (channelCount == 2)
            {
                pos += 2;
            }
            i++;
        }

        return unityData;
    }

    public IEnumerator TextToSpeechWithUnityNetworking(string text, string lang)
    {


        Debug.Log("Text to Speech in: " + lang);
        string gender = "Female";
        string contentString = GenerateSsml(lang, gender, GetVoiceName(lang, gender), text);


        using (UnityWebRequest www = new UnityWebRequest(textToSpeechEndpoint, UnityWebRequest.kHttpVerbPOST))
        {
            UploadHandlerRaw MyUploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(contentString));
            MyUploadHandler.contentType = "application/ssml+xml";
            www.uploadHandler = MyUploadHandler;
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + textToSpeechAuthorizationToken);
            www.SetRequestHeader("X-Search-AppId", "07D3234E49CE426DAA29772419F436CA");
            www.SetRequestHeader("X-Search-ClientID", "1ECFAE91408841A480F00935DC390960");
            www.SetRequestHeader("X-Microsoft-OutputFormat", "riff-16khz-16bit-mono-pcm");
            www.SetRequestHeader("User-Agent", "TTSClient");
            www.SetRequestHeader("Accept", "");
            www.SetRequestHeader("Accept-Encoding", "");

            // www.SetRequestHeader("Content-Type", "application/ssml+xml");


            www.useHttpContinue = true;
            www.chunkedTransfer = true;
           // www.method = UnityWebRequest.kHttpVerbPOST;
            yield return www.SendWebRequest();
            byte[] s = www.downloadHandler.data;
            if ((s != null) && (s.Length > 44) && (s[0]=='R') && (s[1] == 'I') && (s[2] == 'F') && (s[3] == 'F'))
            {
                int sampleCount = 0;
                int frequency = 0;
                var unityData = ToUnityAudio(s, out sampleCount, out frequency);

                // Convert to an audio clip
                var clip = ToClip("Speech", unityData, sampleCount, frequency);

                // Set the source on the audio clip
                audioSource.clip = clip;

                // Play audio
                audioSource.Play();
                Debug.Log("AudioSource Playing :" + text);
                audioSource.loop = false;
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
            }
            else
                Debug.Log("Cognitive Services didn't return WAV stream");


        }
        StopCoroutine("TextToSpeechWithUnityNetworking");
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