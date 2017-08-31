using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Model : MonoBehaviour {

    GameObject prefab;

	// Use this for initialization
	void Start () {
        prefab = Resources.Load("Data/Files/model") as GameObject;
        Instantiate(prefab);
        print(prefab);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
