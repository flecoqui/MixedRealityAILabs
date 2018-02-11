using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR.WSA.WebCam;


public class ImageCapture : MonoBehaviour {
    public static ImageCapture instance;
    public int tapsCount;
    PhotoCapture photoCaptureObject = null;
    GestureRecognizer recognizer;

    private void Awake()
    {
        instance = this;
    }
    
    // Use this for initialization
    void Start () {
        recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap);
        recognizer.Tapped += TapHandler;
        recognizer.StartCapturingGestures();
	}
    private void TapHandler(TappedEventArgs obj)
    {
        tapsCount++;
        ResultsLabel.instance.CreateLabel();
        ExecuteImageCaptureAndAnalysis();
    }
	void OnCapturePhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if(photoCaptureObject!=null)
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
        StartCoroutine(VisionManager.instance.AnalyseLastImageCaptured());
    }
    // Update is called once per frame
    private void ExecuteImageCaptureAndAnalysis () {
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        Texture2D targetTexture = new Texture2D(cameraResolution.width,cameraResolution.height);

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
                string filename = string.Format(@"CapturedImage{0}.jpg", tapsCount );
                string filepath = Path.Combine(Application.persistentDataPath, filename);
                VisionManager.instance.imagePath = filepath;
                Debug.Log("Saving Photo into file:" + filepath);
                photoCaptureObject.TakePhotoAsync(filepath, PhotoCaptureFileOutputFormat.JPG, OnCapturePhotoToDisk);
            }
            );



        }
        );
	}
}
