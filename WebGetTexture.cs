using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebGetTexture : MonoBehaviour {

    Global global;
    GameObject plane;
	// Use this for initialization
	void Start () {
        global = GetComponent<Global>();
        global.PutText("Start");
        StartCoroutine(GetTexture());
        plane = GameObject.Find("Plane");
      
    }
    IEnumerator GetTexture()
    {
        using (UnityWebRequest www = UnityWebRequest.GetTexture("http://hololens-training-app.azurewebsites.net/Data/Project%201/photos/icon/how2start.png"))
        {
            global.PutText("Loading...");
            yield return www.Send();

            if (www.isError)
            {
                global.PutText(www.error);
            }
            else
            {
                global.PutText("No error");
                Texture tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Renderer rend = plane.GetComponent<Renderer>();
                rend.material.mainTexture = tex;

            }
        }
    }
}
