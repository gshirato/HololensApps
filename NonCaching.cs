using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NonCaching : MonoBehaviour {


    // Use this for initialization
    IEnumerator Start()
    {
        string url = "http://hololens-training-app.azurewebsites.net/AssetBundles/assets/assetbundles.unity3d";
        string assetName = "model.fbx";

        using (WWW www = new WWW(url))
        {
            yield return www;

            if (www.error != null)
            {
                throw new Exception("www download error: " + www.error);
            }


            AssetBundle bundle = www.assetBundle;
            if (assetName == "")
            {
                Instantiate(bundle.mainAsset);
            }
            else
            {
                Instantiate(bundle.LoadAsset(assetName));
                bundle.Unload(false);
            }

        }
    }
}
