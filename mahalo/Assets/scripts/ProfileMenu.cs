using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;



public class ProfileMenu : MonoBehaviour {

    public string dataFileName;
    public GameObject picture; 
    public Text nameText;
    public Text jobText;
    public Text schoolText;
    public Text locationText; 

    private ProfileData data; 
    private ProfileMenuItem[] menuItems;
    private ProfileMenuItem focusedItem;
    private Animation infoTextAnim; 

	void Start()
    {
        data = LoadProfileData(dataFileName);

        RefreshText(); 

        menuItems = gameObject.GetComponentsInChildren<ProfileMenuItem>();
        //menuItems[0].text = data.headline; 
        //menuItems[1].text = data.locationName;
        //menuItems[2].text = data.summary;
        //menuItems[3].text = data.industryName;

        //infoTextAnim = infoText.GetComponent<Animation>();
    }

    private void Update()
    {
        
    }
    public void UpdateFocusedItem(ProfileMenuItem item)
    {
        if (item == focusedItem)
            return;

        if (focusedItem)
        {
            focusedItem.UnFocus();
            //infoTextAnim.Play("a_FadeOut");
        }

        focusedItem = item;
        focusedItem.Focus();
    }

    public ProfileData LoadProfileDataFromFile(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            return this.LoadProfileData(jsonData);
        }
        else
        {
            Debug.LogError("Error loading profile data from JSON");
            return new ProfileData(); 
        }
    }

    public ProfileData LoadProfileData(string jsonObject)
    {
        return JsonUtility.FromJson<ProfileData>(jsonObject);
    }


    public void ReDraw(string jsonProfile, Person p, Face[] faces)
    {
        this.data = this.LoadProfileData(jsonProfile);
        // On taking picture: Store camera location / rotation 
        // Given face rect left/top coordinates, and estimated face distance (from rect size), 
        // use camera FOV and aspect ratio to unproject from 2D screen space to 3D world space
        // gameObject.transform.localPosition = new Vector3(x, y, z); 
        RefreshText();
        if (this.data.pictureInfo == null || this.data.pictureInfo.croppedImage == null)
        {
            Debug.Log("Public profile image is not available to render" + this.data);
            return;
        }
        StartCoroutine(RefreshPicture(this.data.pictureInfo.croppedImage));

    }

    private void RefreshText()
    {
        nameText.text = data.firstName + " " + data.lastName;
        jobText.text = data.headline;
        schoolText.text = "Not Shared Publicly";
        locationText.text = data.locationName; 
    }
    private IEnumerator RefreshPicture(string imageUrl)
    {
        string MEDIA_URL = "https://media.licdn.com/mpr/mpr/";
        string url = MEDIA_URL + imageUrl;

        Texture2D tex;
        tex = new Texture2D(100, 100, TextureFormat.DXT1, false);
        WWW www = new WWW(url);
        yield return www;
        www.LoadImageIntoTexture(tex);
        picture.GetComponent<Renderer>().material.mainTexture = tex;

    }
}
