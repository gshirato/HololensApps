using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// In order to debug on Unity Editor
/// </summary>
public class CameraDebug : MonoBehaviour {



    private Vector3 oldPos;
    private const float Mag=10;

    /// <summary>
    /// OuiCestFrancais for AZERTY keyboard
    /// Noway for QWERTY keyboard
    /// </summary>
    public enum KeyBoardType 
    {
        OuiCestFrancais,
        NoWay
    }
    private KeyBoardType keyboardtype;


    // Use this for initialization
    void Start ()
    {
        //You can change 
        keyboardtype = KeyBoardType.OuiCestFrancais;
    }
	

	// Update is called once per frame
	void Update ()
    {
        //Debug DrawRay
        RayRefresh();

        //Left Click
		if(Input.GetMouseButton(1))
        {
            //Camera Moving
            
            if (keyboardtype == KeyBoardType.OuiCestFrancais)
            {
                //For FRENCH Keyboard
                FR_CameraWork();
            }
            else
            {
                //For the other Keyboards
                Other_CameraWorks();
            }

            //Mouse Dragging 
            MouseDrag();
        }
	}
    /// <summary>
    /// RayRefresh for debug
    /// Looking forward
    /// </summary>
    private void RayRefresh()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 100;
        Debug.DrawRay(transform.position, forward, Color.red);
    }

    //Mouse Drag
    private void MouseDrag()
    {
        var diff = Input.mousePosition - oldPos;
        if (Mathf.Abs(diff.x) < 10f && Mathf.Abs(diff.y) < 10f)
        {
            transform.Rotate(Vector3.up, diff.x / Mag);
            transform.Rotate(transform.right, -diff.y / Mag);
        }

        oldPos = Input.mousePosition;
    }


    //For French Keyboard
    #region FR Keyboard

        /// <summary>
        /// A Z E
        ///  Q S D 
        /// </summary>
    private void FR_CameraWork()
    {
        CameraTranslationXY_FR();
        CameraTranslationZ_FR();
    }

    private void CameraTranslationXY_FR()
    {
        if (Input.GetKey(KeyCode.D))
            transform.position += transform.right/Mag;
        if (Input.GetKey(KeyCode.Q))
            transform.position -= transform.right / Mag;

        if (Input.GetKey(KeyCode.Z))
            transform.position += transform.up / Mag;
        if (Input.GetKey(KeyCode.S))
            transform.position -= transform.up / Mag;
    }
    private void CameraTranslationZ_FR()
    { 
        float uppos = 0.0f;
        if(Input.GetKey(KeyCode.A)) uppos = -1/ Mag;
        if(Input.GetKey(KeyCode.E)) uppos = 1/ Mag;
        transform.position += transform.up * uppos;
    }
    #endregion


    //For the other keyboards
    #region The others

        /// <summary>
        /// Q W E
        ///  A S D
        /// </summary>
    private void Other_CameraWorks()
    {
        CameraTranslationXY_Others();
        CameraTranslationZ_Others();
    }
    private void CameraTranslationXY_Others()
    {
        transform.position += transform.right * Input.GetAxis("Horizontal") / Mag;
        transform.position += transform.up * Input.GetAxis("Vertical") / Mag;
    }
    private void CameraTranslationZ_Others()
    {
        float uppos = 0.0f;
        if (Input.GetKey(KeyCode.Q)) uppos = -1/Mag;
        if (Input.GetKey(KeyCode.E)) uppos = 1/Mag;
        transform.position += transform.up * uppos;
    }
    #endregion
}

