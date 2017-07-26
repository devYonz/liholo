using UnityEngine;
using UnityEngine.UI;
using System.IO; 

public class ProfileMenu : MonoBehaviour {

    public string dataFileName; 
    public Text nameText;
    public Text infoText; 

    private ProfileData data; 
    private ProfileMenuItem[] menuItems;
    private ProfileMenuItem focusedItem;
    private Animation infoTextAnim; 

	void Start()
    {
        data = LoadProfileData(dataFileName); 

        nameText.text = data.firstName + " " + data.lastName;
        infoText.text = ""; 

        menuItems = gameObject.GetComponentsInChildren<ProfileMenuItem>();
        menuItems[0].text = data.headline; 
        menuItems[1].text = data.locationName;
        menuItems[2].text = data.summary;
        menuItems[3].text = data.industryName;

        infoTextAnim = infoText.GetComponent<Animation>(); 
    }

    public void UpdateFocusedItem(ProfileMenuItem item)
    {
        if (item == focusedItem)
            return;

        if (focusedItem)
        {
            focusedItem.UnFocus();
            infoTextAnim.Play("a_FadeOut");
        }

        focusedItem = item;
        focusedItem.Focus();
    }

    public ProfileData LoadProfileData(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            return JsonUtility.FromJson<ProfileData>(jsonData);
        }
        else
        {
            Debug.LogError("Error loading profile data from JSON");
            return new ProfileData(); 
        }
    }

    public void UpdateText()
    {
        infoText.text = focusedItem.text;
        infoTextAnim.Play("a_FadeIn"); 
    }
}
