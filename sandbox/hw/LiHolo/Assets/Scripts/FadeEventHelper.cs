using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeEventHelper : MonoBehaviour {

    private ProfileMenu profileMenu; 

	void Start()
    {
        profileMenu = gameObject.GetComponentInParent<ProfileMenu>(); 
	}
	
    public void OnFadeOutComplete()
    {
        profileMenu.UpdateText(); 
    }
}
