using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class FaceAPI
{
    private string subscriptionKey = "d2f5dd1babbe48a8b8e0e6cc3bf39758";
    private string personGroupId = "testliholo_v1";
    private static string baseUrl = "https://westus.api.cognitive.microsoft.com/face/v1.0/";
    private static Dictionary<string, string> urls;
    public string faceId = null;
    public string name = null;
    
    public FaceAPI()
    {
        this.initUrls();
        Debug.Log("FaceAPI initialized with defaults");
    }
    public FaceAPI(string subscription)
    {
        this.subscriptionKey = subscription;
        this.initUrls();
        Debug.Log("FaceAPI initialized with new key");
    }
    public FaceAPI(string subscription, string group)
    {        
        this.subscriptionKey = subscription;
        this.personGroupId = group;
        this.initUrls();
        Debug.Log("FaceAPI initialized with new key and group");
    }
    private void initUrls()
    {
        urls = new Dictionary<string, string>()
        {
            {"detect", baseUrl + "detect?"},
            {"person" , baseUrl + "persongroups/" + this.personGroupId + "/persons/"},
            {"identity", baseUrl + "identify?"},
        };
    }
    // Hack to extact value from a response instead of building json decoder
    private string extractPersonID(string target)
    {
        string toBeSearched = "personId";
        string searched = target.Substring(target.IndexOf(toBeSearched) + toBeSearched.Length + 3, 36);
        Debug.Log("extractPersonID found PersonId:  " + searched + "\nfrom: " + target);
        return searched;
    }

    private bool inspectResponse(WWW response)
    {
        Debug.Log("Checking Response from URL: " + response.url);
        if (!response.isDone || !string.IsNullOrEmpty(response.error))
        {
            Debug.LogError("Request to recieved an error: " + response.error);
            Debug.LogWarning("Error response text: " + response.text);
            return false;
        }
        return true;

    }

    /* Missing the over 60% confidence train with the current image functionality
    private async void addFaceAndTrainData(string imagePath, System.Guid personId)
    {
        try
        {
            using (Stream imageFileStream = File.OpenRead(imagePath))
            {
                AddPersistedFaceResult faceRes = await faceServiceClient.AddPersonFaceAsync(this.personGroupId, personId, imageFileStream);
                Console.Out.WriteLine("Added face to Person with Id " + faceRes.PersistedFaceId);
            }


            await faceServiceClient.TrainPersonGroupAsync(this.personGroupId);
            TrainingStatus status = null;
            do
            {
                status = await faceServiceClient.GetPersonGroupTrainingStatusAsync(this.personGroupId);
            } while (status.Status.ToString() != "Succeeded");
        }
        catch (FaceAPIException f)
        {
            MessageBox.Show(f.ErrorCode, f.ErrorMessage);
        }
    } **/

    public IEnumerator getFaceDataFromImage(string filePath, System.Action<Person> callback)
    {
        byte[] bytes = UnityEngine.Windows.File.ReadAllBytes(filePath);

        var headers = new Dictionary<string, string>() {
            { "Ocp-Apim-Subscription-Key",  subscriptionKey},
            { "Content-Type", " application/octet-stream" }
        };

        WWW detectImageResponse = new WWW(urls["detect"], bytes, headers);
        yield return detectImageResponse;
        if(  !this.inspectResponse(detectImageResponse))  {
            yield break;
        }


        string responseData = detectImageResponse.text;
        Debug.Log("Response to detect : " + responseData);
        // Hack to solve top level array json decoding by adding a wrapper dict
        responseData = "{\"faces\": " + responseData + "}";
        Debug.Log("Response from the request after change: " + responseData);

        AnalyzeResult a = JsonUtility.FromJson<AnalyzeResult>(responseData);
        foreach (Face face in a.faces)
        {
            Debug.Log("Face Id " + face.faceId);
            this.faceId = face.faceId;
            // Process only one face
            break;
        }

        // Identify Faces in the image
        headers = new Dictionary<string, string>() {
          { "Ocp-Apim-Subscription-Key",  subscriptionKey},
          { "Content-Type", "application/json" }
        };
        string jsonData = "{faceIds:[\"" + this.faceId + "\"]" + ",personGroupId:\"" +
            personGroupId + "\", confidenceThreshold:0.3, maxNumOfCandidatesReturned:1}";
        Debug.Log("Going to run identify request3");
        byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData);
        WWW identifyRequest = new WWW(urls["identity"], data, headers);
        yield return identifyRequest;
        if (!this.inspectResponse(identifyRequest))  {
            yield break;
        }
        
        responseData = identifyRequest.text;
        // Get personID to fetch person Data
        string personId = this.extractPersonID(responseData);
        string personUrl = urls["person"] + personId;
        
        // Fetch person and associated data
        Debug.Log("Person URL: " + personUrl);
        WWW fetchPersonResponse = new WWW(personUrl, null, headers);
        yield return fetchPersonResponse;
        if (!this.inspectResponse(fetchPersonResponse)) {
            yield break;
        }
        responseData = fetchPersonResponse.text;           
        Debug.Log("Response from person request: " + responseData);

        Person p = JsonUtility.FromJson<Person>(responseData);
        Debug.Log("Response from person request: " + p.name);
        this.name = p.name;
        callback(p);
        yield return null;
    }

}