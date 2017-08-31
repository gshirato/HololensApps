using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

using System.IO;

public class GazeController : MonoBehaviour
{
    const bool _Debug = false;

    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject { get; private set; }
    CreateMenu createmenu;
    AppSceneManager scenemanager;
    GestureRecognizer recognizer;
    JsonClass jsonclass;
    Global global;
    WWW www;
    public Vector3 posHitInfo;

    public JSONObject jsonMenuObject, jsonTrainingObject;
    string currentPath;
    //Cursor's hitInfo
    public RaycastHit hitInfo;
    public bool IsHit;
    public enum Mode
    {
        Menu,
        ChooseTraining,
        Loading,
        Training,
        Choice,
        Finish
    }
    public Mode mode;
    public int actual_scene;
    public GameObject choices_title;

    // Use this for initialization
    IEnumerator Start()
    {
        print("[Gaze Controller] Start");
        actual_scene = 0;
        previous.SetActive(false);
        following.SetActive(false);

        jsonclass = GetComponent<JsonClass>();
        scenemanager = GetComponent<AppSceneManager>();
        createmenu = GetComponent<CreateMenu>();
        global = GetComponent<Global>();

        string _path = global.appPath + "Data/json/training.json";
        www = new WWW(_path);
        yield return www;
        if (www.error == null)
        {
            if (www.isDone)
            {
                jsonMenuObject = jsonclass.Init(_path, www);
                www.Dispose();
                createmenu.Init();
                mode = Mode.Menu;
            }
        }
        else
        {
            print(www.error);
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

        //DEBUG: airtap(HoloLens) = space key(PC)
        if (Input.GetKeyDown("space"))
        {
            OnClick();
        }

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
        if (actual_scene == 0)
        {
            if (previous.activeSelf)
            {
                previous.SetActive(false);
            }
        }
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
            if (scenemanager.scene_next != null)
            {
                if (scenemanager.scene_next.ToLower() == "end")
                {
                    if (mode != Mode.Finish)
                    {
                        mode = Mode.Finish;
                    }
                }
            }

        }



    }

    void OnLoading()
    {
        if (!www.isDone)
        {
            print("Loading.");
        }
        else
        {
            jsonTrainingObject = jsonclass.Init(currentPath, www);
            mode = Mode.Training;
            scenemanager.Init();
            if (scenemanager.json != null)
            {
                OpenScene();

            }

        }
    }

    public GameObject previous, following;
    void OnClick()
    {
        if (hitInfo.transform.name == "Menu")
        {
            scenemanager.sceneNumberIndex = 0;
            Restart();
            ResetScene();
            createmenu.Init();
        }
        else
        {
            switch (mode)
            {
                case Mode.Menu:
                    if (IsHit)
                    {
                        print("[Mode]: Menu");
                        ResetScene();
                        createmenu.Creating();
                        mode = Mode.ChooseTraining;
                    }
                    else
                    {
                        if (scenemanager.sceneNumberIndex == -1)
                        {
                            scenemanager.sceneNumberIndex = 0;

                            print("[Mode]: Menu Restarted");
                            ResetScene();
                            createmenu.Init();
                        }
                    }
                    break;
                case Mode.ChooseTraining:
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
                    previous.SetActive(true);
                    following.SetActive(true);
                    if (hitInfo.collider != null)
                    {
                        if (hitInfo.transform.tag == "Previous")
                        {
                            PreviousButton();
                        }
                        else
                        {
                            MoveToNextScene();
                        }
                    }
                    else
                    {
                        MoveToNextScene();
                    }
                    break;


                case Mode.Choice:
                    print("[Mode]: Choice");
                    ResetScene();

                    previous.SetActive(true);


                    if (hitInfo.collider != null)
                    {
                        if (hitInfo.transform.tag == "Previous")
                        {
                            PreviousButton();
                            mode = Mode.Training;
                        }

                        else
                        {
                            setChoices();
                            choices_title.SetActive(true);
                            choices_title.GetComponent<TextMesh>().text = scenemanager.json.GetField("scenes")[scenemanager.sceneNumberIndex].GetField("choices_title").str;

                        }
                        if (hitInfo.transform.tag == "Choice")
                        {
                            setChoices();
                            choices_title.SetActive(true);
                            choices_title.GetComponent<TextMesh>().text = scenemanager.json.GetField("scenes")[scenemanager.sceneNumberIndex].GetField("choices_title").str;
                            switch (hitInfo.transform.name)
                            {
                                case "A":
                                    scenemanager.scene_next = scenemanager.json.GetField("scenes")[scenemanager.sceneNumberIndex].GetField("choices")[0].GetField("next").str;
                                    scenemanager.sceneNumberIndex = scenemanager.searchingIndex(scenemanager.scene_next, "id", scenemanager.json);
                                    scenemanager.Choices.SetActive(false);
                                    mode = Mode.Training;
                                    OpenScene();
                                    break;

                                case "B":
                                    scenemanager.scene_next = scenemanager.json.GetField("scenes")[scenemanager.sceneNumberIndex].GetField("choices")[1].GetField("next").str;
                                    scenemanager.sceneNumberIndex = scenemanager.searchingIndex(scenemanager.scene_next, "id", scenemanager.json);
                                    scenemanager.Choices.SetActive(false);
                                    mode = Mode.Training;
                                    OpenScene();
                                    break;
                                case "C":
                                    scenemanager.scene_next = scenemanager.json.GetField("scenes")[scenemanager.sceneNumberIndex].GetField("choices")[2].GetField("next").str;
                                    scenemanager.sceneNumberIndex = scenemanager.searchingIndex(scenemanager.scene_next, "id", scenemanager.json);
                                    scenemanager.Choices.SetActive(false);
                                    mode = Mode.Training;
                                    OpenScene();
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;


                case Mode.Finish:
                    //To improve
                    print("[Mode]: Finish");
                    ResetScene();
                    previous.SetActive(true);

                    scenemanager.PutText("End", Color.magenta);



                    Restart();
                    break;

            }
        }
    }
    void MoveToNextScene()
    {

        actual_scene++;
        scenemanager.IsHistory = true;
        OpenScene();

    }
    void PreviousButton()
    {

        scenemanager.IsHistory = false;

        if (scenemanager.sceneNumberIndex > 0)
        {
            actual_scene--;
            scenemanager.scene_next = scenemanager.scene_history[actual_scene];
            scenemanager.sceneNumberIndex = scenemanager.searchingIndex(scenemanager.scene_next, "id", scenemanager.json);
        }
        else
        {
            previous.SetActive(false);
        }

        OpenScene();
    }


    void setChoices()
    {
        scenemanager.Choices.SetActive(true);
        //choose choices*
        TextMesh textA = scenemanager.ChoiceA.GetComponentInChildren<TextMesh>();
        TextMesh textB = scenemanager.ChoiceB.GetComponentInChildren<TextMesh>();
        TextMesh textC = scenemanager.ChoiceC.GetComponentInChildren<TextMesh>();

        textA.text = scenemanager.json.GetField("scenes")[scenemanager.sceneNumberIndex].GetField("choices")[0].GetField("title").str;
        textB.text = scenemanager.json.GetField("scenes")[scenemanager.sceneNumberIndex].GetField("choices")[1].GetField("title").str;
        textC.text = scenemanager.json.GetField("scenes")[scenemanager.sceneNumberIndex].GetField("choices")[2].GetField("title").str;


    }
    void ResetScene()
    {
        if (following.activeSelf)
        {
            following.SetActive(false);
        }
        if (previous.activeSelf)
        {
            previous.SetActive(false);
        }
        if (scenemanager.sphere.activeSelf)
        {
            scenemanager.sphere.SetActive(false);
        }
        if (scenemanager.go_transparent.activeSelf)
        {
            scenemanager.go_transparent.SetActive(false);
        }
        if (scenemanager.subtitle.activeSelf)
        {
            scenemanager.subtitle.SetActive(false);
        }
        DeleteObjects();
    }

    void Restart()
    {
        //previous.SetActive(true);
        //following.SetActive(true);

        scenemanager.scene_next = null;
        mode = Mode.Menu;
        www = null;
        scenemanager.json = null;
        jsonTrainingObject = null;
        actual_scene = 0;
        scenemanager.scene_history.Clear();
    }

    void OpenScene()
    {
        scenemanager.OpenScene();
    }

    void LoadTraining()
    {
        print("[GazeController]: LoadTraining");
        string planeName = "training.json";
        string foldername = Path.GetFileNameWithoutExtension(hitInfo.transform.name);
        string _path = global.appPath + "Data/" + foldername + "/" + planeName;
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

    void OnGaze()
    {
        FocusedObject = hitInfo.collider.gameObject;
    }

    void OffGaze()
    {
        //If the raycast did not hit a model
        FocusedObject = null;

    }
    public void DeleteObjects()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Objects"))
        {
            Destroy(obj);
        }
    }
}
