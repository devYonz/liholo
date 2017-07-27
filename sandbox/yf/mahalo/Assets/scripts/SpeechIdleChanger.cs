using System.Collections;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.VR.WSA.WebCam;
using System.Linq;
using System.Collections.Generic;
using System;


[Serializable]
public class Face
{
    public string faceId;

    [Serializable]
    public class FaceRectangle
    {
        public int top;
        public int width;
        public int height;
        public int left;
    }

    public FaceRectangle faceRectangle;
}


[Serializable]
public class AnalyzeResult {
    public Face[] faces;
}


[Serializable]
public class Person {
    public string personId;
    public List<string> persistedFaceIds;
    public string name;
}

public class FaceAPIClient
{
    public static string personGroupId = "testliholo_v1";
    static string baseUrl = "https://westus.api.cognitive.microsoft.com/face/v1.0/detect?";
    public string faceId = null;
    public string name = null;
    public FaceAPIClient()
    {
         
    }

    static Dictionary<string, string> urls = new Dictionary<string, string>()
    {
        {"detect", "https://westus.api.cognitive.microsoft.com/face/v1.0/detect?"},
        {"person" , "https://westus.api.cognitive.microsoft.com/face/v1.0/persongroups/testlihalo_v3/persons/"},
        {"identity", "https://westus.api.cognitive.microsoft.com/face/v1.0/identify?"},
    };

    static string subscriptionKey = "d2f5dd1babbe48a8b8e0e6cc3bf39758";

    public IEnumerator getFaceDataFromImage(string filePath)
    {
        byte[] bytes = UnityEngine.Windows.File.ReadAllBytes(filePath);

        var headers = new Dictionary<string, string>() {
            { "Ocp-Apim-Subscription-Key",  subscriptionKey},
            { "Content-Type", " application/octet-stream" }
        };
 
        WWW www = new WWW(urls["detect"], bytes, headers);
        yield return www;
        string responseData = www.text;
        Debug.Log("Response from the request: " + responseData);
        responseData = "{\"faces\": " + responseData + "}";
        Debug.Log("Response from the request after change: " + responseData);
        AnalyzeResult a = JsonUtility.FromJson< AnalyzeResult> (responseData);
        foreach (Face face in a.faces)
        {
            Debug.Log("Face Id " + face.faceId);
            this.faceId = face.faceId;
            break;
        }

        // Identify
        headers = new Dictionary<string, string>() {
          { "Ocp-Apim-Subscription-Key",  subscriptionKey},
          { "Content-Type", "application/json" }
        };
        string jsonData = "{faceIds:[\"" + this.faceId + "\"]" + ",personGroupId:\"" + personGroupId + "\", confidenceThreshold:0.3, maxNumOfCandidatesReturned:1}";
        Debug.Log("Going to run identify request3");
        byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
        WWW www2 = new WWW(urls["identity"], data, headers);
        yield return www2;
        responseData = www2.text;
        string toBeSearched = "personId";
        string searched = responseData.Substring(responseData.IndexOf(toBeSearched) + toBeSearched.Length + 3, 36);
        Debug.Log("PersonId:  " + searched);
        string personUrl = urls["person"] + searched;
        Debug.Log("Person URL: " + personUrl);
        WWW www3 = new WWW(personUrl, null, headers);
        yield return www3;
        responseData = www3.text;
        Debug.Log("Response from person request: " + responseData);
        Person p = JsonUtility.FromJson<Person>(responseData);
        Debug.Log("Response from person request: " + p.name);
        this.name = p.name;
        yield return null;
    }
}

/// <summary>
/// Functionality built to change color of sphere on AirTap
/// Stop camera on Hello keyword
/// Take a picture on Mahalo keyword
/// </summary>
public class SpeechIdleChanger : MonoBehaviour, ISpeechHandler, IInputClickHandler
{
    bool mahalo;
    bool cameraReady;
    private string latestCapturePath;
    PhotoCapture _photoCaptureObject = null;
	string sub_key = "<sub_key_here>";
	string personGroupId = "<person_group_here>";
    


    void Start()
    {
        mahalo = false;
        cameraReady = false;
        Debug.Log("Camera is OFF");
        PhotoCapture.CreateAsync(true, OnPhotoCaptureCreated);
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
                StopCamera();
                break;
            default:
                break;
        }
    }
    #endregion ISpeechHandler

    #region CameraRegion
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
    public void StopCamera()
    {
        if (cameraReady)
        {
            _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
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
            //string filepath = @"C:\Users\pthapar\workspaces\testCognitive\TestDAta\pankit.jpg";
            // string filePath = 
            FaceAPIClient faceAPiClient = new FaceAPIClient();
            StartCoroutine(faceAPiClient.getFaceDataFromImage(latestCapturePath));
 
            // Debug.Log("Recieved person data for " + faceAPiClient.name);

            // StartCoroutine(faceAPiClient.IdentifyFacesFromFaceId());
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