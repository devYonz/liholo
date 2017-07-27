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
public class AnalyzeResult
{
    public Face[] faces;
}


[Serializable]
public class Person
{
    public string personId;
    public List<string> persistedFaceIds;
    public string name;
}