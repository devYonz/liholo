using UnityEngine;
// using Microsoft.ProjectOxford.Face;
using HoloToolkit.Unity.InputModule;
using UnityEngine.VR.WSA.WebCam;
using System.Linq;
using System.Collections;

using System.Collections.Generic;

/// <summary>
/// Functionality built to change color of sphere on AirTap
/// Stop camera on Hello keyword
/// Take a picture on Mahalo keyword
/// </summary>
public class Overwatch : MonoBehaviour, ISpeechHandler, IInputClickHandler
{
    bool mahalo;
    bool cameraReady;
    private string latestCapturePath;
    PhotoCapture _photoCaptureObject = null;
	public string subscription_key;
	public string personGroupId;
    FaceAPI faceAPIClient;


    void Start()
    {
        mahalo = false;
        cameraReady = false;
        Debug.Log("Camera is OFF");
        PhotoCapture.CreateAsync(true, OnPhotoCaptureCreated);

        if (!string.IsNullOrEmpty(this.subscription_key) && !string.IsNullOrEmpty(this.personGroupId)) {
            this.faceAPIClient = new FaceAPI(subscription_key, personGroupId);
        }
        else if (!string.IsNullOrEmpty(this.subscription_key))
        {
            this.faceAPIClient = new FaceAPI(subscription_key);
        }
        else  {
            this.faceAPIClient = new FaceAPI();
        }

    }

    void Update()
    {
        if (mahalo)
        {
            Debug.Log("Mahalo frame capture getting invoked");
            mahalo = false;
            TakePhoto();
        }

    }
    public void onFaceIdentifyCompleted(Person identifiedPerson)
    {
        Debug.Log("onFaceIdentifyCompleted callback completed with Person: " + identifiedPerson);
        StartCoroutine(ProfileFetch.fetchProfile(identifiedPerson, onProfileFecthComplete));
    }

    public void onProfileFecthComplete(string jsonProfile, Person person)
    {
        Debug.Log("onProfileFecthComplete Callback invoked with json:" + jsonProfile + "Person object" + person);

    }
    
    #region ISpeechHandler
    public void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
    {
        string myWord = eventData.RecognizedText.ToLower();
        switch (myWord)
        {
            case "mahalo":                
            case "hello":
                Debug.Log("Recieved a " + myWord + " keyword");
                gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
                mahalo = true;
                break;
            case "start camera":
                Debug.Log("Recieved start camera keyword");
                StartCamera();
                break;
            case "stop camera":
                Debug.Log("Recieved stop camera keyword");
                StartCamera();
                break;
            default:
                break;
        }
    }
    #endregion ISpeechHandler

    #region CameraRegion
    public void StartCamera()
    {
        PhotoCapture.CreateAsync(true, OnPhotoCaptureCreated);
    }
    public void StopCamera()
    {
        if (cameraReady)
        {
            _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
    }
    public void TakePhoto()
    {
        if (cameraReady)
        {
            string file = string.Format(@"Image_{0:yyyy-MM-dd_hh-mm-ss-tt}.jpg", System.DateTime.Now);
            latestCapturePath = System.IO.Path.Combine(Application.persistentDataPath, file);

            _photoCaptureObject.TakePhotoAsync(latestCapturePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
        }
        else
        {
            Debug.LogWarning("The camera is not ready.");
        }
    }

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
        cameraReady = result.success;
        if (result.success)
        {
            Debug.Log("Camer is Ready");
        }
        else
        {
            Debug.LogError("Unable to get Camera ready, capture functionality will not work");
        }
    }

	private void getProfileData(System.Guid personId) {
		// Returns the json from userData for the given person
		// faceServiceClient.GetPersonAsync(this.personGroupId, id);
		// return json_obj 
	}

	private void getFaceFromImage() {
		// Returns the Face object in the image.
	}

	private void identifyFaceFromPersonGroup() {
		//Returns the personId for the matching person in the image.
	}

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
			Debug.Log("Photo captured at: " + latestCapturePath);
            
            // Identify person alias
            StartCoroutine(this.faceAPIClient.getFaceDataFromImage(latestCapturePath, onFaceIdentifyCompleted));

            // face = getFaceFromImage()
            // personIds = identifyFaceFromPersonGroup(faceIds)
            // getProfileData(personId)
            // add componentObject(profileData)
        }
        else
        {
            Debug.LogError("ERROR\n**************\n\nFailed to save Photo to disk.");
        }
        _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        _photoCaptureObject.Dispose();
        _photoCaptureObject = null;
        Debug.Log("Camera OFF");
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