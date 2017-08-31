using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _360degrees : MonoBehaviour {

    //public GameObject go;
    Vector3 rot;
    Vector3 gazeDirection_int;
    Vector3 offset; float distance;
    public GameObject cam;
	// Use this for initialization
	void Start () {
        rot.x = transform.eulerAngles.x;
        rot.y = transform.eulerAngles.y;
        rot.z = transform.eulerAngles.z;

        gazeDirection_int = Camera.main.transform.forward;
        offset = cam.transform.position - transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 gazeDirection = Camera.main.transform.forward;
        Vector3 diffrotation = gazeDirection - gazeDirection_int;

        rot.x += -diffrotation.y*100f;
        rot.y += diffrotation.x * 100f;
       
        print("[rot] x: " + rot.x + ", y: " + rot.y + ", z: " + rot.z);
        //transform.eulerAngles = rot;
        gazeDirection_int = gazeDirection;

    }
}
