using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
public class CachingTest : MonoBehaviour {

   
	// Use this for initialization
	void Start () {
        Caching.CleanCache();
        string url = "http://hololens-training-app.azurewebsites.net/AssetBundles/model";
        StartCoroutine(DownloadAndCache("model.fbx", url, 0));
    }
	
	
    public IEnumerator DownloadAndCache(string assetName, string url, int version)
    {
        while(!Caching.ready)
        {
            print("caching...");
            yield return null;
        }

        using (WWW www = WWW.LoadFromCacheOrDownload(url, version))
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
