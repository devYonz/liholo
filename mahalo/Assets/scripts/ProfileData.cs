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

    [System.Serializable]
    public class PictureInfo
    {
        public string croppedImage;
    }
    public string firstName;
    public string lastName;
    public string locationName;
    public string industryName;
    public string headline;
    public string summary;
    public BirthDate birthDate;
    public PictureInfo pictureInfo;
}