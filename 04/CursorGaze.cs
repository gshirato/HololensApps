using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorGaze : MonoBehaviour{
    //Cursor's mesh and hitInfo
    private MeshRenderer meshRenderer;
    private RaycastHit hitInfo;

    // Use this for initialization
    void Start () {
        //Cursor grabs its mesh and initialization of shaders
        GrabMesh();
	}

    
	void GrabMesh()
    {
        meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update () {
        //Head position and facial direction
        var headPosition = Camera.main.transform.position;
        var headDirection = Camera.main.transform.forward; 

        if(Physics.Raycast(headPosition,headDirection,out hitInfo))
        {
            //If the raycast hit a model
            OnGaze();
        }
        else
        {
            //If the raycast did not hit a model
            OffGaze();
        }
	}

    void OnGaze()
    {
        //If the raycast hit a model
        //Display the red cursor
        DisplayCursor();
    }
    void OffGaze()
    {
        //If the raycast did not hit a model
        //Hide the cursor
        HideCursor();
    }

    void DisplayCursor()
    {
        meshRenderer.enabled = true;
        //Move the cursor to the point where the raycast hit.
        this.transform.position = hitInfo.point;
        this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
    }

    void HideCursor()
    {
        //Hide meshRender so as to hide the cursor
        meshRenderer.enabled = false;
    }  

}
