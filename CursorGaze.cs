
using UnityEngine;

/// <summary>
/// To visualize what you see
/// </summary>
public class CursorGaze : MonoBehaviour{

    #region private variables 
    //Cursor's mesh and hitInfo
    private MeshRenderer meshRenderer;
    private RaycastHit hitInfo;
    #endregion

    // Use this for initialization
    void Start () {
        GrabMesh();
	}


    /// <summary>
    /// Cursor grabs its mesh
    /// </summary>
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
        DisplayCursor();
    }
    void OffGaze()
    {
        //If the raycast did not hit a mode
        HideCursor();
    }

    /// <summary>
    /// Display the cursor 
    /// </summary>
    void DisplayCursor()
    {
        meshRenderer.enabled = true;
        //Move the cursor to the point where the raycast hit.
        this.transform.position = hitInfo.point;
        this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
    }

    /// <summary>
    /// Hide the cursor
    /// </summary>
    void HideCursor()
    {
        //Hide meshRender so as to hide the cursor
        meshRenderer.enabled = false;
    }  

}
