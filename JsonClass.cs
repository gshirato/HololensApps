using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// To generate a json class
/// </summary>
public class JsonClass : MonoBehaviour {

    #region Public lists
    public List<string> images;
    public List<string> DataStructure;
    public List<string> DataStructureTraining;
    #endregion

    #region Other Classes
    AppSceneManager scenemanager;
    GazeController gazecontroller;
    #endregion

    #region public variables
    public enum dataKeys{title, file, image, audio}
    public dataKeys dk;
    public int numApps, numScenes;
    #endregion


    /// <summary>
    /// Get Components
    /// </summary>
    private void Awake()
    {
        print("[JsonClass]: Start");
        scenemanager = GetComponent<AppSceneManager>();
        gazecontroller = GetComponent<GazeController>();
    }
   
    /// <summary>
    /// Pick up a json file from path and create JSONObject
    /// </summary>
    /// <param name="path">Path you can find a json file with</param>
    /// <param name="www"></param>
    /// <returns></returns>
    public JSONObject Init( string path, WWW www)
    {
        print("[JsonClass]: Init");
        JSONObject jsonObj;

        jsonObj = ReadJsonFile(path, www);
        if (gazecontroller.mode == GazeController.Mode.Menu)
        {
            initializationDataList(jsonObj);
            parseData(jsonObj);
        }
       
        return jsonObj;
    }

    /// <summary>
    /// First Step: read a json file and create JSONOBject
    /// </summary>
    /// <returns>JsonObject</returns>
    public JSONObject ReadJsonFile(string __path, WWW www)
    {
        print("[JsonClass]: ReadJsonFile");
        string textFile = "";
        JSONObject jsonObject;

        textFile = www.text;

        jsonObject = new JSONObject(textFile);

        return jsonObject;
    }
    /// <summary>
    /// Second: Preparation of lists
    /// </summary>
    /// <param name="jsonObject">JsonObject of First Step</param>
    void initializationDataList(JSONObject jsonObject)
    {
        print("[JsonClass initializationDataList]");

        numApps = jsonObject.GetField("app").Count;
        print("num Apps = " + numApps);

        for (int i = 0; i < numApps; i++)
        {
            n_additions(images, 1);

            n_additions(DataStructure, 4);
        }


    }

    /// <summary>
    /// Final Step: Parsing Data. Creating DataStructure by it
    /// </summary>
    /// <param name="jsonObject"></param>
    void parseData(JSONObject jsonObject)
    {

        print("[JsonClass ParseData]");
        for (int i = 0; i < numApps; i++)
        {
            JSONObject jsonApp = jsonObject.GetField("app")[i];
            DataStructure[(int)dataKeys.title + i * 4] = (string)jsonApp.GetField("title").str;
            DataStructure[(int)dataKeys.file + i * 4] = (string)jsonApp.GetField("file").str;
            DataStructure[(int)dataKeys.image + i * 4] = (string)jsonApp.GetField("image").str;
            DataStructure[(int)dataKeys.audio + i * 4] = (string)jsonApp.GetField("audio").str;

            //Image
            if (DataStructure[(int)dataKeys.image + i * 4] == "")
            {
                print("No image...");
                DataStructure[(int)dataKeys.image + i * 4] = "default.png";
            }
            else
            {

            }

            images[i] = DataStructure[(int)dataKeys.image + i * 4];
        }
    }


    void n_additions(List<string> string_list, int n)
    {
        for (int i = 0; i < n; i++)
        {
            string_list.Add("");
        }
    }


}
