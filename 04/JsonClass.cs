using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
public class JsonClass : MonoBehaviour {

    public List<string> images;
    public List<string> DataStructure;
    public List<string> DataStructureTraining;

    AppSceneManager scenemanager;
    GazeController gazecontroller;

    public enum dataKeys{id, title, description, image}
    public dataKeys dk;
    public int numApps, numScenes;


    void Start()
    {
        print("[JsonClass]: Start");
        scenemanager = GetComponent<AppSceneManager>();
        gazecontroller = GetComponent<GazeController>();
    }
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
    void parseData(JSONObject jsonObject)
    {

        print("[JsonClass ParseData]");
        for (int i = 0; i < numApps; i++)
        {
            JSONObject jsonApp = jsonObject.GetField("trainings")[i];
            DataStructure[(int)dataKeys.id + i * 4] = (string)jsonApp.GetField("id").str;
            DataStructure[(int)dataKeys.title + i * 4] = (string)jsonApp.GetField("title").str;
            DataStructure[(int)dataKeys.description + i * 4] = (string)jsonApp.GetField("description").str;
            DataStructure[(int)dataKeys.image + i * 4] = (string)jsonApp.GetField("image").str;

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

    void initializationDataList(JSONObject jsonObject)
    {
        print("[JsonClass initializationDataList]");

        numApps = jsonObject.GetField("trainings").Count;
        print("num Apps = " + numApps);

        for (int i = 0; i < numApps; i++)
        {
            n_additions(images, 1);

            n_additions(DataStructure, 4);
        }


    }
    void n_additions(List<string> string_list, int n)
    {
        for (int i = 0; i < n; i++)
        {
            string_list.Add("");
        }
    }


    public JSONObject ReadJsonFile( string __path, WWW www)
    {
        print("[JsonClass]: ReadJsonFile");
        string textFile = "";
        JSONObject jsonObject;

        textFile = www.text;

        jsonObject = new JSONObject(textFile);

        return jsonObject;
    }

}
