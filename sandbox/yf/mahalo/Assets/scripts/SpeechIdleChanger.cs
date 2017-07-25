using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.VR.WSA.WebCam;
using UnityEngine.Windows;
using System.Linq;

public class SpeechIdleChanger : MonoBehaviour, ISpeechHandler, IInputClickHandler
{
    bool mahalo;
    PhotoCapture _photoCaptureObject = null;

    void Start()
    {
        mahalo = false;   
    }

    void Update()
    {
        if (mahalo)
        {
            Debug.Log("Mahalo frame capture to be invoked");
            mahalo = false;
            // PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        }

    }
    #region ISpeechHandler
    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
    {
        string myWord = eventData.RecognizedText.ToLower();
        switch (myWord)
        {
            case "mahalo":
                Debug.Log("Recieved a Mahalo keyword");
                gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                mahalo = true;
                break;
            case "hello":
                Debug.Log("Recieved a Hello keyword");
                break;
            default:
                break;
        }
    }
    #endregion ISpeechHandler

    #region CameraRegion
    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        _photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);

    }
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"screen_pic.jpg");
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            //doing this to get formatted image
            _photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);

        }
        else
        {
            Debug.Log("Say: Unable to start photo mode! Hasta la vista, baby.");
        }
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"terminator_analysis.jpg");
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

            byte[] image = File.ReadAllBytes(filePath);
            // member_id = pankit(image | filepath)
            //  profileData = getProfileData (member_id)
            // add componentObject(profileData)
            

        }
        else
        {
            Debug.Log("DIAGNOSTIC\n**************\n\nFailed to save Photo to disk.");
        }
        _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        _photoCaptureObject.Dispose();
        _photoCaptureObject = null;
    }
    #endregion Camera Region
    #region IInputClickHandler
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (gameObject.GetComponent<MeshRenderer>().material.color != Color.red)
        {
            gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 255, 255);
        }
    }
    #endregion IInputClickHandler
}