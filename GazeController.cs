using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

using System.IO;

public class GazeController : MonoBehaviour
{
    //To save the path
    string currentPath;

    #region Other classes
    CreateMenu createmenu;
    AppSceneManager scenemanager;
    GestureRecognizer recognizer;
    JsonClass jsonclass;
    Global global;
    #endregion

    WWW www;

    #region public variables
    /// <summary>
    /// Represents the hologram that is currently being gazed at.
    /// </summary>
    public GameObject FocusedObject { get; private set; }

    /// <summary>
    /// Cursor's hitInfo
    /// </summary>
    public RaycastHit hitInfo;
    /// <summary>
    /// if it's hit by a ray
    /// </summary>
    public bool IsHit;
    /// <summary>
    /// Position Vector of HitInfo
    /// </summary>
    public Vector3 posHitInfo;

    /// <summary>
    /// JsonObject for Menu and Training
    /// </summary>
    public JSONObject jsonMenuObject, jsonTrainingObject;
    /// <summary>
    /// Training state
    /// </summary>
    public enum Mode
    {
        Menu,
        ChooseTraining,
        Loading,
        Training,
        Finish
    }
    public Mode mode;
    #endregion
    /// <summary>
    /// GetComponent
    /// </summary>
    void Awake()
    {
        jsonclass = GetComponent<JsonClass>();
        scenemanager = GetComponent<AppSceneManager>();
        createmenu = GetComponent<CreateMenu>();
        global = GetComponent<Global>();
    }


    // Use this for initialization
    IEnumerator Start()
    {
        string _path = global.appPath + "json/training.json";
        www = new WWW(_path);
        yield return www;
        if(www.isDone)
        {
            //Initialize JsonObject for menu
            jsonMenuObject = jsonclass.Init(_path, www);
            //once it's done you dispose of www
            www.Dispose();
            //Create Menu
            createmenu.Init();
            //Put state to menu
            mode = Mode.Menu;
        }
  
        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
                OnClick();
        };
        recognizer.StartCapturingGestures();
    }

    void OnHold()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Figure out which hologram is focused this frame.
        GameObject oldFocusObject = FocusedObject;

        //Head position and facial direction
        var headPosition = Camera.main.transform.position;
        var headDirection = Camera.main.transform.forward;

        #region ONLY FOR DEBUGGING
        // FOR DEBUG
        if (Input.GetKeyDown("space"))
        {
            OnClick();
        }
        #endregion

        IsHit = Physics.Raycast(headPosition, headDirection, out hitInfo);
        if (IsHit)
        {
            //If the raycast hit a model
            OnGaze();
        }
        else
        {
            //If the raycast did not hit a model
            OffGaze();

        }

        //If Loading's not finished
        if (mode == Mode.Loading)
        {
            OnLoading();
        }
        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject)
        {
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
        }

        if (scenemanager.MaxSceneNumber != 0)
        {
            //If it reaches the last scene
            if (scenemanager.sceneNumber == scenemanager.MaxSceneNumber)
            {
                if (mode != Mode.Finish)
                {
                    mode = Mode.Finish;
                    scenemanager.sceneNumber = -1;
                }
            }
        }



    }

    void OnLoading()
    {
        if(!www.isDone)
        {
            print("Loading.");
        }
        else
        {
            //Once Done...
            jsonTrainingObject = jsonclass.Init(currentPath, www);
            mode = Mode.Training;
            scenemanager.Init();
            if(scenemanager.json != null)
            {
                OpenScene();
            }
            
        }
    }


    void OnClick()
    {
        //If the raycast hit a model and you tap
        switch (mode)
        {
            case Mode.Menu:
                if (IsHit)
                {
                    print("[Mode]: Menu");
                    //Create a menu from scratch    
                    ResetScene();
                    createmenu.Creating();
                    mode = Mode.ChooseTraining;
                }
                else
                {
                    //From 2nd turn
                    if(scenemanager.sceneNumber == -1)
                    {
                        scenemanager.sceneNumber = 0;

                        print("[Mode]: Menu Restarted");
                        ResetScene();
                        createmenu.Init();
                    }
                }
                break;
            case Mode.ChooseTraining:

                //Choose Training
                if (IsHit)
                {
                    print("[Mode]: Choose Training");
                    posHitInfo = hitInfo.transform.position;
                    ResetScene();                  
                }

                LoadTraining();
                scenemanager.Init();
                if (scenemanager.json != null)
                {
                    OpenScene();
                }
                else
                {
                    mode = Mode.Loading;
                }

                break;
            case Mode.Loading:
                print("Loading");
                break;

            case Mode.Training:
                print("[Mode]: Training");
                ResetScene();
                OpenScene();
                
                break;

            case Mode.Finish:
                print("[Mode]: Finish");
                ResetScene();
                scenemanager.PutText("End");
                Restart();
                break;

        }

    }

    #region Manage scenes function
    void ResetScene()
    {
        if (scenemanager.sphere.activeSelf)
        {
            scenemanager.sphere.SetActive(false);
        }
        DeleteObjects();
    }

    void Restart()
    {

        mode = Mode.Menu;
        www = null;
        scenemanager.json = null;
        jsonTrainingObject = null;
    }

    void OpenScene()
    {
        scenemanager.OpenScene();
        scenemanager.NextScene();
     }

    void LoadTraining()
    {
        print("[GazeController]: LoadTraining");
        string planeName = hitInfo.transform.name;
        string foldername = Path.GetFileNameWithoutExtension(planeName);
        string _path = global.appPath + "Data/" + foldername + "/json/" + planeName;
        currentPath = _path.Replace(" ", "%20");

        StartCoroutine(LoadScenes(currentPath));

        print(planeName);
        mode = Mode.Training;
    }

    IEnumerator LoadScenes(string pathname)
    {
        www = new WWW(pathname);
        yield return www;
        if (www.error == null)
        {
            print("no error");
            if (www.isDone)
            {
                print("done");
                jsonTrainingObject = jsonclass.Init(pathname, www);
                yield return www;
            }

        }
        else
        {
            print(www.error);
        }
        
        
        
      
    }
    #endregion

    /// <summary>
    /// while it is gazed
    /// </summary>
    void OnGaze()
    {
        FocusedObject = hitInfo.collider.gameObject;
    }
    /// <summary>
    /// while it is not gazed
    /// </summary>
    void OffGaze()
    {
        //If the raycast did not hit a model
        FocusedObject = null;

    }
    /// <summary>
    /// To initialize a scene
    /// </summary>
    public void DeleteObjects()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Objects"))
        {
            Destroy(obj);
        }
    }
}
