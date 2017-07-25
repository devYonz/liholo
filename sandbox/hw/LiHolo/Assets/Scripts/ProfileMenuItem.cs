using UnityEngine;
using HoloToolkit.Unity.InputModule; 

public class ProfileMenuItem : MonoBehaviour, IFocusable {

    [HideInInspector]
    public string text; 

    private ProfileMenu profileMenu; 
    private Animation anim; 

	void Start()
    {
        profileMenu = gameObject.GetComponentInParent<ProfileMenu>(); 
        anim = gameObject.GetComponent<Animation>(); 
	}

    void IFocusable.OnFocusEnter()
    {
        profileMenu.UpdateFocusedItem(this);
    }

    public void Focus()
    {
        anim.PlayQueued("a_Focus");
    }

    public void UnFocus()
    {
        anim.PlayQueued("a_UnFocus");
    }

    void IFocusable.OnFocusExit() { }
}
