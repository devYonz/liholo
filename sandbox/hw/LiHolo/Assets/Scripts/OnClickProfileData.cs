using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class OnClickProfileData : MonoBehaviour {

    public string fileName; 

    private ProfileDataController profileDataController; 

	void Start()
    {
        Button button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        profileDataController = gameObject.GetComponent<ProfileDataController>(); 
	}
	
	void OnClick()
    {
        profileDataController.LoadProfileData(fileName);
        print(JsonUtility.ToJson(profileDataController.profileData, true)); 
    }
}
