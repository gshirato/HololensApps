using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// To create a menu
/// </summary>
public class CreateMenu : MonoBehaviour
{


    enum dataKeys { title, file, image, audio }

    //List of Menu elements
    List<GameObject> planeObjects;

    //MenuPanel (is it loaded earlier than one in AppSceneManager ?)
    GameObject menuPanel;
    //classes
    AppSceneManager scenemanager;
    JsonClass jsonclass;
    Global global;
    GameObject ClickHere;
 
    //Size of menu
    int ViewWidth, ViewHeight;
    
    ///GetCompoment of the class and definition of the menu size
    private void Start()
    {
        print("[CreateMenu Start]");
        ViewHeight = 9600;
        ViewWidth = 14400;
        jsonclass = GetComponent<JsonClass>();
        global = GetComponent<Global>();
 
    }
    /// <summary>
    /// Show the title 
    /// </summary>
    public void Init()
    {
        global.PutText("[CreateMenu Init]");
        //Afficher Opening
        print("[CreateMenu Init]");
        ClickHere = GameObject.CreatePrimitive(PrimitiveType.Plane);
        global.PutText("[SetPlane Clickhere]");
        SetPlane(ClickHere);
        global.PutText("ReadFile");
        StartCoroutine(ReadFile(ClickHere, global.appPath + "Files/airtaphere.png"));

        ClickHere.tag = "Objects";
    }

    /// <summary>
    /// Creating windows
    /// </summary>
    public void Creating()
    {
        print("[CreateMenu Creating]");
        menuPanel = GameObject.Find("MenuPanel");
        planeObjects = new List<GameObject>();

        for (int i = 0; i < jsonclass.numApps; i++)
        {
            planeObjects.Add(GameObject.CreatePrimitive(PrimitiveType.Plane));
            SetPlane(i);
            StartCoroutine(ReadFile(i));

        }
    }
    #region Functions

    /// <summary>
    /// For a title panel
    /// </summary>
    /// <param name="go">ClickHere</param>
    /// <param name="_path">path of photo for ClickHere</param>
    /// <returns></returns>
    IEnumerator ReadFile(GameObject go, string _path)
    {
        print("[CreateMenu ReadFile(" + go.name + ", " + _path + ")");
        WWW www = new WWW(_path);
        yield return www;
        if (www.error == null)
        {
            if (www.isDone)
            {
                Renderer rend = go.GetComponent<Renderer>();
                Texture2D tex2d = new Texture2D(1, 1);
                tex2d = www.texture;
                rend.material.mainTexture = tex2d;
            }
        }
        else
        {
            print(www.error);
        }
    }

    /// <summary>
    /// Read icons by searching directely on Hierarchy window
    /// </summary>
    /// <param name="i">index</param>
    /// <returns>IEnumerator</returns>
    IEnumerator ReadFile(int i)
    {
        print("[CreateMenu ReadFile(" + i + ")");
        //From Here
        string __path;
        string __name;
        string __file;

        //Renderer
        __file = Path.GetFileNameWithoutExtension(jsonclass.DataStructure[(int)dataKeys.file + i * 4]);


        __name = jsonclass.images[i];
        __path = global.appPath + "Data/" + __file + "/photos/icon/" + __name;
        __path = __path.Replace(" ", "%20");
        print(__path);
        WWW www = new WWW(__path);
        yield return www;
        if (www.error==null)
        {
            if (www.isDone)
            {
                global.PutText("Read Done");
                Renderer rend = planeObjects[i].GetComponent<Renderer>();
                Texture2D tex2d = new Texture2D(1, 1);
                tex2d = www.texture;

                rend.material.mainTexture = tex2d;
            }
        }
        else
        {
            global.PutText(www.error);
            print(www.error);
        }

    }

    /// <summary>
    /// Change transform for a title
    /// </summary>
    /// <param name="go">ClickHere</param>
    void SetPlane(GameObject go)  
    {
        print("[CreateMenu SetPlane(" + go.name + ")]");
        //Function
        Vector3 scalePlane = go.transform.localScale;
        Vector3 posPlane = go.transform.localPosition;

        posPlane.x = 0;
        posPlane.y = 0;
        posPlane.z = 2;

        go.transform.localPosition = new Vector3(posPlane.x, posPlane.y, posPlane.z);

        //Rotation
        go.transform.Rotate(-90, 0, 0);
        //Scale
        scalePlane.x = -0.4f;
        scalePlane.y = 1f;
        scalePlane.z = -0.3f;

        go.transform.localScale = new Vector3(scalePlane.x, scalePlane.y, scalePlane.z);
    }
    /// <summary>
    /// Change transform for menu windows
    /// </summary>
    /// <param name="i"></param>
    void SetPlane(int i)
    {
        print("[CreateMenu SetPlane("+i+")]");
        //Function
        Vector3 scalePlane = planeObjects[i].transform.localScale;
        Vector3 posPlane = planeObjects[i].transform.localPosition;

        int columnNumber = (jsonclass.numApps > 5) ? 5 : jsonclass.numApps;
        int rowNumber = (jsonclass.numApps <= 5) ? 1 : Mathf.CeilToInt((float)jsonclass.numApps / columnNumber);

        int sizeBetweenColumns = ViewWidth / (columnNumber - 1);
        int sizeBetweenRows = (rowNumber == 1) ? ViewHeight : ViewHeight / (rowNumber - 1);
        //int sizeBetweenRows = 900;
        //Position

        /* x:  0  1  2  3  4 | z    
             ----------------+--- 
          i =  0  1  2  3  4 | 0 
               5  6  7  8  9 | 1
              10 11 12 13 14 | 2
              15 16 17 18 19 | 3
              20 21 22 23 24 | 4 */


        planeObjects[i].transform.parent = menuPanel.transform;
        //name 
        planeObjects[i].name = jsonclass.DataStructure[(int)dataKeys.file + i * 4];
        planeObjects[i].tag = "Objects";

        posPlane.x = sizeBetweenColumns * (i % 5) - 0.5f *ViewWidth;
        posPlane.y = +sizeBetweenRows * Mathf.FloorToInt(i / 5) /3f;
        posPlane.z = 0;

        planeObjects[i].transform.localPosition = new Vector3(posPlane.x, posPlane.y, posPlane.z);

        //Rotation
        planeObjects[i].transform.Rotate(-90, 0, 0);
        //Scale
        scalePlane.x = -320f;
        scalePlane.y = 1f;
        scalePlane.z = -240f;


        planeObjects[i].transform.localScale = new Vector3(scalePlane.x, scalePlane.y, scalePlane.z);
    }
    #endregion

}
