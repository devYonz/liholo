using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule; 

public class ProfileMenuItem : MonoBehaviour, IFocusable {

    private Animation anim; 

	void Start()
    {
        anim = gameObject.GetComponent<Animation>(); 
	}

    void IFocusable.OnFocusEnter()
    {
        print("Enter");

        anim.PlayQueued("a_Highlight");
    }

    void IFocusable.OnFocusExit()
    {
        print("Exit");

        anim.PlayQueued("a_UnHighlight"); 
    }
}
