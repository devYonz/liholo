using UnityEngine;
using HoloToolkit.Unity.InputModule; 

public class ProfileMenuItem : MonoBehaviour, IFocusable {

    public string action;

    public GameObject[] sharedConnections = new GameObject[3]; 

    [HideInInspector]
    public string text; 

    private ProfileMenu profileMenu; 
    private Animation anim; 

	void Start()
    {
        profileMenu = gameObject.GetComponentInParent<ProfileMenu>(); 
        anim = gameObject.GetComponent<Animation>();
        HideSharedConnections(); 
	}

    void IFocusable.OnFocusEnter()
    {
        profileMenu.UpdateFocusedItem(this);
    }

    public void Focus()
    {
        anim.PlayQueued("a_FocusIn");

        if (action.Equals("SharedConnections"))
        {
            ShowSharedConnections(); 
        }
    }

    public void UnFocus()
    {
        anim.PlayQueued("a_FocusOut");

        if (action.Equals("SharedConnections"))
        {
            //HideSharedConnections(); 
        }
    }

    public void ShowSharedConnections()
    {
        float count = 0.0f; 
        foreach (GameObject obj in sharedConnections)
        {
            obj.SetActive(true);
            obj.GetComponent<Animation>().Play("a_ScaleIn"); 
            obj.GetComponent<Animation>()["a_SineZ"].time = count; 
            obj.GetComponent<Animation>().Blend("a_SineZ", 1.0f, 0.5f);
            count += 1.0f; 
        }
    }

    public void HideSharedConnections()
    {
        foreach (GameObject obj in sharedConnections)
        {
            obj.SetActive(false);
        }
    }

    void IFocusable.OnFocusExit() { }
}
