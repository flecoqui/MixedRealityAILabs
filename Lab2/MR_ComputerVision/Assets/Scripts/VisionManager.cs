using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class TagData
{
    public string name;
    public float confidence;

}
[System.Serializable]
public class AnalyseObject
{
    public TagData[] tags;
    public string requestId;
    public object metadata;

}

public class VisionManager : MonoBehaviour {
    public static VisionManager instance;
    private string AuthorizationKey = "2a5cf0e3513543b4ab321f4466379612";
    private const string ocpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";
    private string visionAnalysisEndPoint = "https://northeurope.api.cognitive.microsoft.com/vision/v1.0/analyze?visualfeatures=Tags";
    //private string visionAnalysisEndPoint = "https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualfeatures=Tags";
    [HideInInspector] public byte[] imageBytes;

    [HideInInspector] public string imagePath;

    private void Awake()
    {
        instance = this;
    }
    public IEnumerator AnalyseLastImageCaptured()
    {
        WWWForm form = new WWWForm();
        using (UnityWebRequest www = UnityWebRequest.Post(visionAnalysisEndPoint, form))
        {
            Debug.Log("Reading file:" + imagePath);
            imageBytes = GetImageAsByteArray(imagePath);
            if (imageBytes != null)
            {
                Debug.Log("Sending file to ComputerVision:" + imagePath);
                www.SetRequestHeader("Content-Type", "application/octet-stream");
                www.SetRequestHeader(ocpApimSubscriptionKeyHeader, AuthorizationKey);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.uploadHandler = new UploadHandlerRaw(imageBytes);
                www.uploadHandler.contentType = "application/octet-stream";
                yield return www.SendWebRequest();
                long responseCode = www.responseCode;

                try
                {
                    string jsonResponse = null;

                    jsonResponse = www.downloadHandler.text;

                   // using (Stream stream = GenerateStreamFronString(jsonResponse))
                   // {
                     //   StreamReader reader = new StreamReader(stream);
                        AnalyseObject analyseObject = new AnalyseObject();
                        analyseObject = JsonUtility.FromJson<AnalyseObject>(jsonResponse);
                        if (analyseObject.tags == null)
                        {
                            Debug.Log("analyseObject.tagData is null");
                        }
                        else
                        {
                            Dictionary<string, float> tagsDictionary = new Dictionary<string, float>();
                            foreach (TagData td in analyseObject.tags)
                            {
                                TagData tag = td as TagData;
                                tagsDictionary.Add(tag.name, tag.confidence);

                            }

                            ResultsLabel.instance.SetTagsToLastLabel(tagsDictionary);
                        }

                   // }

                }
                catch (System.Exception e)
                {
                    Debug.Log("Json e.Message: " + e.Message);
                }
            }
        }
        yield return null;
    }
    public byte[] GetImageAsByteArray(string s)
    {
        FileStream fileStream = null;
        BinaryReader binaryReader = null;
        try
        {
            fileStream = new FileStream(s, FileMode.Open, FileAccess.Read);
            if(fileStream!=null)
            binaryReader = new BinaryReader(fileStream);
        }
        catch(System.Exception ex)
        {
            Debug.Log("Exception while reading file: " + ex.Message);
        }
        if(binaryReader!=null)
        return binaryReader.ReadBytes((int) fileStream.Length);
        return null;
    }
    public static Stream GenerateStreamFronString(string s)
    {
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
