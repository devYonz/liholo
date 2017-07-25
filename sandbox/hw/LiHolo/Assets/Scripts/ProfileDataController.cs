using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI; 

public class ProfileDataController : MonoBehaviour {

    public string fileName; 

    [HideInInspector]
    public ProfileData profileData;

    public void Start()
    {
        LoadProfileData(fileName); 
    }

    public void LoadProfileData(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName); 

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            profileData = JsonUtility.FromJson<ProfileData>(jsonData);

            gameObject.GetComponentInChildren<Text>().text = profileData.firstName + " " + profileData.lastName; 
        }
        else
        {
            Debug.LogError("Error loading profile data from JSON"); 
        }
    }
}

[System.Serializable]
public class ProfileData
{

    [System.Serializable]
    public class BirthDate
    {
        public int month;
        public int year;
        public int day;
    }

    public string firstName;
    public string lastName;
    public string locationName;
    public string industryName;
    public string headline;
    public string summary;
    public BirthDate birthDate;
}
