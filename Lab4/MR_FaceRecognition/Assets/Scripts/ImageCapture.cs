using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR.WSA.WebCam;

public class ImageCapture : MonoBehaviour {

    public static ImageCapture instance;   // static instance of this class           
    public int tapsCount; // counter for tap input     
    PhotoCapture photoCaptureObject = null; // object resulting from photo capture     
    GestureRecognizer recognizer; // Hololens input recognizer 

    private void Awake()
    {   // allows this instance to behave like a singleton      
        instance = this;
    } 

    void Start()
    {  // subscribing to the Hololens API gesture recognizer to track user gestures         
        recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap);
        recognizer.Tapped += TapHandler;
        recognizer.StartCapturingGestures();
    }
    /// <summary>     
    /// 
    /// Respond to Tap Input.     
    /// </summary>     
    private void TapHandler(TappedEventArgs obj)
    {         // increment taps count, used to name images when saving        
        tapsCount++;
        // Begins the image capture and analysis procedure         

        ExecuteImageCaptureAndAnalysis();
    }


    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        // Call StopPhotoMode once the image has successfully captured
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    } 

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Dispose from the object in memory and request the image analysis          
        // to the Oxford class 
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
        StartCoroutine(Oxford.instance.DetectFaces());
    }


    private void ExecuteImageCaptureAndAnalysis()
    {
        // Set the camera resolution to be the highest possible
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Begin capture process, set the image format
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject)
        {
            photoCaptureObject = captureObject; 

            CameraParameters c = new CameraParameters();
            c.hologramOpacity = 0.0f;
            c.cameraResolutionWidth = targetTexture.width;
            c.cameraResolutionHeight = targetTexture.height;
            c.pixelFormat = CapturePixelFormat.BGRA32;

            // Capture the image from the camera and save it in the App internal folder
            captureObject.StartPhotoModeAsync(c, delegate (PhotoCapture.PhotoCaptureResult result)
            {
                string filename = string.Format(@"CapturedImage{0}.jpg", tapsCount);
                string filePath = Path.Combine(Application.persistentDataPath, filename);
                Oxford.instance.imagePath = filePath;
                photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);

            });
        });
    }






            // Update is called once per frame
            void Update () {
		
	}
}
