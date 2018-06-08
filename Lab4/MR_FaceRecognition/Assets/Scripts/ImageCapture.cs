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
    private int tapsCount; // counter for tap input     
    private PhotoCapture photoCaptureObject = null; // object resulting from photo capture     
    private GestureRecognizer recognizer; // Hololens input recognizer 
    private bool bCapturing = false;
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
        bCapturing = false;
    }
    /// <summary>     
    /// 
    /// Respond to Tap Input.     
    /// </summary>     
    private void TapHandler(TappedEventArgs obj)
    {
        if (bCapturing == false)
        {
            bCapturing = true;
            // increment taps count, used to name images when saving        
            tapsCount++;
            // Begins the image capture and analysis procedure         

            ExecuteImageCaptureAndAnalysis();
        }
    }
    /// <summary>
    /// Begin process of Image Capturing and send To Azure Computer Vision service.
    /// </summary>
    private void ExecuteImageCaptureAndAnalysis()
    {
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending
            ((res) => res.width * res.height).First();
        Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject)
        {
            photoCaptureObject = captureObject;

            CameraParameters c = new CameraParameters();
            c.hologramOpacity = 0.0f;
            c.cameraResolutionWidth = targetTexture.width;
            c.cameraResolutionHeight = targetTexture.height;
            c.pixelFormat = CapturePixelFormat.BGRA32;

            captureObject.StartPhotoModeAsync(c, delegate (PhotoCapture.PhotoCaptureResult result)
            {
                string filename = string.Format(@"CapturedImage{0}.jpg", tapsCount);
                string filePath = Path.Combine(Application.persistentDataPath, filename);

                // Set the image path on the FaceAnalysis class
                FaceAnalysis.Instance.imagePath = filePath;
                Debug.Log("CapturedImage: " + filePath );
                photoCaptureObject.TakePhotoAsync
                (filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
            });
        });
    }

    /// <summary>
    /// Called right after the photo capture process has concluded
    /// </summary>
    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    /// <summary>
    /// Register the full execution of the Photo Capture. If successfull, it will begin the Image Analysis process.
    /// </summary>
    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;

        // Request image caputer analysis
        StartCoroutine(FaceAnalysis.Instance.DetectFacesFromImage());

        bCapturing = false;
    }


 





}
