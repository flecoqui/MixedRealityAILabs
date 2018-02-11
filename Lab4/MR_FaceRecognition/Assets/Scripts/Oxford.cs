using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR.WSA.WebCam;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Threading.Tasks;
using System.IO;
//using Microsoft.ProjectOxford.Face;
//using Microsoft.ProjectOxford.Face.Contract;


public class Oxford : MonoBehaviour {

    public static Oxford instance; // static instance of this class     
    public TextMesh labelText;  

    [HideInInspector] public byte[] imageBytes;
    [HideInInspector] public string imagePath;

    //private const string authorizationKey = "a4132ce253614f07b32c5173de93857f";
    private const string authorizationKey = "94543cea34ef48c0bc65b04a9238061e";
    private const string personGroupId = "firstgroupid";

   // private FaceServiceClient faceServiceClient; //used by Oxford libraries to communicate with FaceAPI     
   // private PersonGroup knownGroup; // reference of the group to query for candidates 


    private void Awake()
    { // allows this instance to behave like a singleton       
        instance = this;    
    }


    void Start() {
    //    faceServiceClient = new FaceServiceClient(authorizationKey);
        labelText.text = ".";
    }

    //private async void AnalyseImageWithFaceAPI()
    //{   
    //    await GetGroup();

    //    if (knownGroup != null)
    //    {
    //        imageBytes = GetImageAsByteArray(imagePath); 

    //        using (Stream stream = new MemoryStream(imageBytes))
    //        {
    //            Person p = await GetPersonFromImage(stream);
    //        }
    //    }
    //}


    static byte[] GetImageAsByteArray(string imageFilePath)
    {
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);
        return binaryReader.ReadBytes((int)fileStream.Length);
    }



    //internal async Task<Person> GetPersonFromImage(Stream imageStream)
    //{
    //    Person person = null;
    //    if (null != faceServiceClient)
    //    {
    //        try
    //        {
    //            Guid[] faceIds = null;
    //            // try to DETECT faces in the image
    //            Face[] faces = await faceServiceClient.DetectAsync(imageStream);
    //            if (null != faces && faces.Length > 0)
    //            {
    //                // if faces are found assign a Guid                    
    //                faceIds = new Guid[faces.Length];
    //                for (int i = 0; i < faces.Length; i++)
    //                {
    //                    faceIds[i] = faces[i].FaceId;
    //                }
    //                // try to IDENTIFY the face found in the image   
    //                // by retrieving a series of candidates from the queried group  
    //                IdentifyResult[] idResults =  await faceServiceClient.IdentifyAsync(personGroupId, faceIds);
    //                foreach (IdentifyResult idResult in idResults)
    //                {
    //                    double bestConfidence = 0;
    //                    Guid personId = Guid.Empty;
    //                    // try to match the candidate to the face found in the image using a confidence value       
    //                    foreach (Candidate candidate in idResult.Candidates)
    //                    {
    //                        if (bestConfidence < candidate.Confidence)
    //                        {
    //                            bestConfidence = candidate.Confidence;
    //                            personId = candidate.PersonId;
    //                        }
    //                    }
    //                    if (Guid.Empty != personId)
    //                    {
    //                        //display the candidate with the highest confidence 
    //                        person = await faceServiceClient.GetPersonAsync(personGroupId, personId);
    //                        labelText.text = person.Name;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                labelText.text = ("No faces found");
    //            }
    //        }
    //        catch (FaceAPIException ex)
    //        {
    //            labelText.text = ("FaceAPIException ex" + ex.Message);
    //        }
    //        catch (Exception ex)
    //        {
    //            labelText.text = ("Exception ex" + ex.Message);
    //        }
    //    }
    //    if (person == null)
    //    {
    //        labelText.text = ("No person found");
    //    }
    //    return person;
    //}




    //private async Task GetGroup()
    //{
    //    knownGroup = await faceServiceClient.GetPersonGroupAsync(personGroupId);
    //    labelText.text = "Group: " + knownGroup.Name;
    //}

    public IEnumerator DetectFaces()
    {
      //  AnalyseImageWithFaceAPI();
        yield return new WaitForSeconds(6);
        labelText.text = ".";
    }
 }
