using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProfileDataController : MonoBehaviour {

    [HideInInspector]
    public ProfileData profileData;

    public void LoadProfileData(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName); 

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            profileData = JsonUtility.FromJson<ProfileData>(jsonData);
        }
        else
        {
            Debug.LogError("Error loading profile data from JSON"); 
        }
    }
}
