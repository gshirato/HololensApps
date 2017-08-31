using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using System.IO;

/// <summary>
/// Class To manage scenes
/// </summary>
public class AppSceneManager : MonoBehaviour
{

    // For Use public data of other classes
    GazeController gController;
    CreateMenu createmenu;
    JsonClass jsonclass;
    Global global;
    /// <summary>
    /// GameObjects such as cursor, menupanel, hololens camera and directional light (Each name on Hierarchy window should be "Cursor", "MenuPanel", "HoloLensCamera" and "Directional Light")
    /// </summary>
    GameObject Cursor, MenuPanel, HoloLensCamera, DirectionalLight;

    #region public variables
    /// <summary>
    /// Sphere for 360° mode. Need to fbx file Sphere100. 
    /// </summary>
    public GameObject sphere;

    /// <summary>
    /// JSONObject for a training
    /// </summary>
    public JSONObject json;

    /// <summary>
    /// Current scene number(scene = training)
    /// </summary>
    public int sceneNumber;

    /// <summary>
    /// Highest scene number namely the number of trainings(Read Only)
    /// </summary>
    public int MaxSceneNumber { get; private set; }

    /// <summary>
    /// 3D text
    /// </summary>
    public GameObject textPrefab;

    #endregion

    #region init
    /// <summary>
    /// GetComponent of each class
    /// </summary>
    void Awake()
    {
        global = GetComponent<Global>();
        gController = GetComponent<GazeController>();
        createmenu = GetComponent<CreateMenu>();
        jsonclass = GetComponent<JsonClass>();
    }

    /// <summary>
    /// Initialization of GameObjects and sceneNumber
    /// </summary>
    void Start()
    {
        Application.runInBackground = true;
        sceneNumber = 0;

        //(Each name on Hierarchy window should be "Cursor", "MenuPanel", "HoloLensCamera" and "Directional Light")
        Cursor = GameObject.Find("Cursor");
        MenuPanel = GameObject.Find("MenuPanel");
        HoloLensCamera = GameObject.Find("HoloLensCamera");
        DirectionalLight = GameObject.Find("Directional Light");
    }
    

    /// <summary>
    /// Initialisation of this class.
    /// use Json Object of GazeController class
    /// </summary>
    public void Init()
    {
        print("[SceneManager Init]");

        json = gController.jsonTrainingObject;
        //JsonObject can be not loaded.
        if (json != null)
        {
            //See if "files" is not empty
            if (json.GetField("files").Count > 0)
            {
                //MaxSceneNumber is the number of "files"
                MaxSceneNumber = json.GetField("files").Count;
            }
        }

    }
    #endregion

    /// <summary>
    /// Open a scene [_Name_._EXT_]. IF IT DOEN NOT WORK, TRY WITH ANOTHER EXT.
    /// </summary>
    public void OpenScene()
    {
        print("OpenScene");
        string ext = json.GetField("files")[sceneNumber].GetField("type").str.ToLower(); //_EXT_
        string FileName = json.GetField("files")[sceneNumber].GetField("name").str + "." + ext; //_Name_
        string FolderName = json.GetField("title").str;
        bool Is360 = json.GetField("files")[sceneNumber].GetField("Is360View").b;
        switch (ext)
        {

            //images
            case "jpeg":
            case "jpg":
            case "png":
            case "gif":
            case "tif":
            case "tiff":
            case "bmp":
                if (Is360)
                {
                    //360 Degrees
                    PutImages(FolderName, FileName,Is360);
                }
                else
                {
                    //Normal photo
                    PutImages(FolderName, FileName,Is360);
                }
                break;

            case "txt":
            case "json":
            case "xml":
                StartCoroutine(PutText(FolderName, FileName));
                break;

            case "fbx":
                StartCoroutine(Put3dModel(FolderName, FileName));
                break;

            case "ogv":
            case "mp4":

                    PutMovie(FolderName, FileName,Is360);


                break;

            default:
                print("[Extention Not Matching]: " + ext);
                break;
        }


    }

    #region Movies

    void DeleteText(VideoPlayer vp)
    {
        if (GameObject.Find("Text"))
        {
            print("delete text");

            Destroy(GameObject.Find("Text"));
        }

    }

    IEnumerator movie360set(string __path)
    {
        sphere.SetActive(true);
        GameObject movieGo = sphere;
        sphere.tag = "Untagged";

        Vector3 _pos = new Vector3(0, 0, -15f); Vector3 _rot = new Vector3(0, -90f, 0); Vector3 _scale = new Vector3(100f, 100f, -100f);
        if (movieGo.GetComponent<Collider>())
        {
            Destroy(movieGo.GetComponent<Collider>());
        }
        SetTransform(movieGo, _pos, _rot, _scale);

        GameObject camera = GameObject.Find("HoloLensCamera");
        VideoPlayer videoPlayer;
        AudioSource audioSource;
        if (sphere.GetComponent<AudioSource>() == null)
        {
            audioSource = sphere.AddComponent<AudioSource>();
        }
        else
        {
            audioSource = sphere.GetComponent<AudioSource>();
        }

        if (camera.GetComponent<VideoPlayer>() == null)
        {
            videoPlayer = camera.AddComponent<VideoPlayer>();
        }
        else
        {
            videoPlayer = camera.GetComponent<VideoPlayer>();
        }

        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;

        videoPlayer.targetMaterialRenderer = movieGo.GetComponent<Renderer>();

        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.url = __path;

        //Show "NowLoading..." while being loaded
        PutText("Now Loading...");

        print("nowLoading");
        yield return videoPlayer.url;

        //Delete "Now Loading..." when it's done
        videoPlayer.prepareCompleted += DeleteText;
        if (videoPlayer.isPrepared)
        {
            if (!videoPlayer.isPlaying)
            {
                videoPlayer.Play();
            }
        }

    }

    IEnumerator movieset(string __path)
    {

        GameObject camera = GameObject.Find("HoloLensCamera");
        VideoPlayer videoPlayer;
        if (camera.GetComponent<VideoPlayer>() == null)
        {
            videoPlayer = camera.AddComponent<VideoPlayer>();
        }
        else
        {
            videoPlayer = camera.GetComponent<VideoPlayer>();
        }

        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;




        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        print(videoPlayer.IsAudioTrackEnabled(0));
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.url = __path;
        yield return videoPlayer.url;
        print("nowLoading");
        GameObject movieGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Vector3 _pos = gController.posHitInfo; Vector3 _rot = new Vector3(-90, 0, 0); Vector3 _scale = new Vector3(-0.32f, 1f, -0.24f);
        SetTransform(movieGo, _pos, _rot, _scale);
        videoPlayer.targetMaterialRenderer = movieGo.GetComponent<Renderer>();

        if (movieGo.GetComponent<Collider>())
        {
            Destroy(movieGo.GetComponent<Collider>());
        }

        videoPlayer.prepareCompleted += DeleteText;
        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
        }

    }
    public void PutMovie(string _folderName, string _fileName, bool Is360)
    {

        print("Put Movie(" + _folderName + "," + _fileName + ")");

        string __path = global.appPath + "Data/" + _folderName + "/movies/" + _fileName;
        __path = __path.Replace(" ", "%20");
        if (Is360)
        {
            StartCoroutine(movie360set(__path));
        }
        else
        {
            StartCoroutine(movieset(__path));
        }

    }

    #endregion

    #region Images


    public void PutImages(string _folderName, string _fileName, bool Is360)
    {
        print("Put Image(" + _folderName + "," + _fileName + ")");

        string __path = global.appPath + "Data/" + _folderName + "/photos/" + _fileName;
        __path = __path.Replace(" ", "%20");

        if (Is360)
        {
            StartCoroutine(LoadImages(__path));
        }
        else
        {
            StartCoroutine(Load360Images(__path));
        }

    }
    IEnumerator LoadImages(string __path)
    {
      
        WWW www = new WWW(__path);
        yield return www;
        if (www.error == null)
        {

            if (www.isDone)
            {
                GameObject image = GameObject.CreatePrimitive(PrimitiveType.Plane);
                Vector3 _pos = gController.posHitInfo;
                Vector3 _rot = new Vector3(-90, 0, 0);
                //Renderer
                Renderer rend = image.GetComponent<Renderer>();
                Texture2D tex2d = new Texture2D(1, 1);

                tex2d = www.texture;
                SetTransform(image, _pos, _rot, tex2d);

                rend.material.mainTexture = tex2d;
            }
        }
        else
        {
            print(www.error);
            if (www.error == "404 Not Found\r")
            {
                MessageNotFound404();
            }
            print(www.error);
            if (www.error == "404 Not Found\r")
            {
                MessageNotFound404();
            }
        }
        print("photo");

    }
    IEnumerator Load360Images(string __path)
    {
        sphere.SetActive(true);
        GameObject imageGo = sphere;


        WWW www = new WWW(__path);
        yield return www;
        if (www.error == null)
        {

            if (www.isDone)
            {
                Vector3 _pos = new Vector3(0, 0, -15f);
                Vector3 _rot = new Vector3(0, -90f, 0);
                Vector3 _scale = new Vector3(100f, 100f, -100f);
                //Renderer

                Renderer rend = imageGo.GetComponent<Renderer>();
                Texture2D tex2d = new Texture2D(1, 1);

                tex2d = www.texture;
                SetTransform(imageGo, _pos, _rot, tex2d);

                rend.material.mainTexture = tex2d;

                sphere.tag = "Untagged";
            }
        }
        else
        {
            print(www.error);
            if (www.error == "404 Not Found\r")
            {
                MessageNotFound404();
            }
            print(www.error);
            if (www.error == "404 Not Found\r")
            {
                MessageNotFound404();
            }
        }


    }

    #endregion

    #region Texts


    /// <summary>
    /// Put a new line every 30 letters
    /// </summary>
    /// <param name="text">Input text</param>
    /// <returns>Output text with new lines</returns>
    string StylingText(string text)
    {
        char _space;
        string r_text = "";
        int _start = 0;
        int n_char = 29, charRest = text.Length;

        while (charRest >= 0)
        {
            if (n_char >= text.Length)
            {
                r_text += text.Substring(_start, text.Length - _start);
                return r_text;
            }
            _space = text[n_char];
            while (_space != ' ')
            {
                n_char--;
                _space = text[n_char];

            }
            r_text += text.Substring(_start, n_char - _start) + "\n";
            _start = n_char;
            n_char += 30;
            charRest -= n_char - _start;
            print(r_text);
        }

        if (_start < text.Length)
        {
            r_text += text.Substring(_start, text.Length - _start);
            print(r_text);
        }



        return r_text;
    }
    public void PutText(string text)
    {
        print("Put Text(" + text + ")");
        GameObject textGO = Instantiate(textPrefab);
        TextMesh text3d = textGO.GetComponent<TextMesh>();
        if (text.Length > 30)
        {
            text = StylingText(text);
        }

        text3d.name = "Text";
        text3d.text = text;

        Vector3 _pos = gController.posHitInfo;
        Vector3 _rot = new Vector3(0, 0, 0);
        Vector3 _scale = new Vector3(0.1f, 0.14f, 1f);

        SetTransform(textGO, _pos, _rot, _scale);
    }
    IEnumerator PutText(string _foldername, string _filename)
    {
        print("Put Text(" + _foldername + "," + _filename + ")");
        GameObject textGO = Instantiate(textPrefab);
        TextMesh text3d = textGO.GetComponent<TextMesh>();
        Vector3 _pos = gController.posHitInfo;
        Vector3 _rot = new Vector3(0, 0, 0);
        Vector3 _scale = new Vector3(0.1f, 0.14f, 1f);
        string _path = global.appPath + "/Data/" + _foldername + "/texts/" + _filename;
        _path = _path.Replace(" ", "%20");
        WWW www = new WWW(_path);
        textGO.name = "text";

        yield return www;
        if (www.error == null)
        {
            if (www.isDone)
            {
                string text = www.text;
                if (text.Length > 30)
                {
                    text = StylingText(text);
                }
                text3d.text = text;


                SetTransform(textGO, _pos, _rot, _scale);
            }
        }
        else
        {
            print(www.error);
            if (www.error == "404 Not Found\r")
            {

                MessageNotFound404();
            }
        }



    }

    #endregion

    #region 3D Models

    IEnumerator Put3dModel(string _folderName, string _fileName)
    {
        GameObject model;
        string filenameWithoutExt = Path.GetFileNameWithoutExtension(_fileName);
        string _path = global.appPath + "/Data/" + _folderName + "/models/" + filenameWithoutExt;
        _path = _path.Replace(" ", "%20");
        WWW www = new WWW(_path);

        yield return www;

        if (www.error == null)
        {
            if (www.isDone)
            {
                AssetBundle bundle = www.assetBundle;

                    model = Instantiate(bundle.LoadAsset(_fileName)) as GameObject;
                    bundle.Unload(false);

                    Vector3 _pos = gController.posHitInfo;
                    Vector3 _rot = new Vector3(0, 0, 0);
                    Vector3 _scale = new Vector3(0.001f, 0.001f, 0.001f);

                    SetTransform(model, _pos, _rot, _scale);
            } 
        }
        else
        {
            print(www.error);
            if (www.error == "404 Not Found\r")
            {

                MessageNotFound404();
            }
        }


       

    }

    #endregion


    #region SetTrasform

    /// <summary>
    /// Change GO's transform with 3 Transform paremeters (! GO's tag will be "Objects")
    /// </summary>
    /// <param name="go">GameObject you change</param>
    /// <param name="pos">Position</param>
    /// <param name="rot">Rotation</param>
    /// <param name="scale">Scale</param>
    void SetTransform(GameObject go, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        //name ;
        go.tag = "Objects";

        go.transform.localPosition = pos;

        //Rotation
        go.transform.localEulerAngles = rot;

        //Scale
        go.transform.localScale = scale;

    }

    /// <summary>
    /// Change GO's transform with tex2d (for normal image/movie) (! GO's tag will be "Objects")
    /// </summary>
    /// <param name="go">GameObject you change</param>
    /// <param name="pos">Position</param>
    /// <param name="rot">Rotation</param>
    /// <param name="tex2d">tex2d</param>
    void SetTransform(GameObject go, Vector3 pos, Vector3 rot, Texture2D tex2d)
    {

        Vector3 scale = go.transform.localScale;
        float ratio_HpW = (float)tex2d.height / tex2d.width;
        //name ;
        go.tag = "Objects";

        go.transform.localPosition = pos;

        //Rotation
        go.transform.localEulerAngles = rot;



        //Scale
        scale.x = -1f;
        scale.y = 1;
        scale.z = scale.x * ratio_HpW;

        go.transform.localScale = new Vector3(scale.x, scale.y, scale.z);
    }
    #endregion


    /// <summary>
    /// Change a scene
    /// </summary>
    public void NextScene()
    {
        print("[Scene Number]: " + sceneNumber);
        sceneNumber++;
    }

    void MessageNotFound404()
    {
        PutText("404 Not Found. Please confirm your file is in the proper folder.");
    }
}
