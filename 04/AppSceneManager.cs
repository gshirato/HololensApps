using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;
using System.IO;
public class AppSceneManager : MonoBehaviour
{    /// <summary>
     /// GameObjects such as cursor, menupanel, hololens camera and directional light (Each name on Hierarchy window should be "Cursor", "MenuPanel", "HoloLensCamera" and "Directional Light")
     /// </summary>
    GameObject Cursor, MenuPanel, HoloLensCamera, DirectionalLight;

    // For Use public data of other classes
    GazeController gController;CreateMenu createmenu;JsonClass jsonclass;Global global;
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
    /// next scene ID
    /// </summary>
    public string scene_next;
    /// <summary>
    /// Current scene number(scene = training)
    /// </summary>
    public int sceneNumberIndex;
    /// <summary>
    /// Highest scene number namely the number of trainings(Read Only)
    /// </summary>
    public int MaxSceneNumber { get; private set; }

    /// <summary>
    /// 3D text for a message you wanna put
    /// </summary>
    public GameObject textPrefab;

    /// <summary>
    /// 3D text for subtitles
    /// </summary>
    public GameObject subtitle;

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
    // Use this for initialization
    void Start()
    {
        scene_history = new List<string>();
        Application.runInBackground = true;
        sceneNumberIndex = 0;
        Cursor = GameObject.Find("Cursor");
        MenuPanel = GameObject.Find("MenuPanel");
        HoloLensCamera = GameObject.Find("HoloLensCamera");
        DirectionalLight = GameObject.Find("Directional Light");
    }

    public void Init()
    {
        print("[SceneManager Init]");

        json = gController.jsonTrainingObject;
        if (json != null)
        {
            string start = json.GetField("start").str;
            scene_history.Add(start);
            sceneNumberIndex = searchingIndex(start, "id", json);
            if (json.GetField("scenes").Count > 0)
            {
                MaxSceneNumber = json.GetField("scenes").Count;
            }
        }

    }
    #endregion
    /// <summary>
    /// search Index 
    /// </summary>
    /// <param name="str"></param>
    /// <param name="key"></param>
    /// <param name="json"></param>
    /// <returns>Index</returns>
    public int searchingIndex(string str, string key,JSONObject json)
    {
        for(int i = 0; i<json.GetField("scenes").Count;i++)
        {
            if(str == json.GetField("scenes")[i].GetField(key).str)
            {
                return i;
            }
        }
        return -1;
    }
    #region scene function

    //Memorize the record of scenes
    public List<string> scene_history;
    public bool IsHistory;
    //How to play movie
    public enum movieMode { VideoPlayer, MovieTexture }
    //GameObject which represents choices
    public GameObject Choices, ChoiceA, ChoiceB, ChoiceC;
    /// <summary>
    /// Put Subtitles
    /// </summary>
    /// <param name="title">subtitles you wanna show</param>
    /// <param name="go">GameObject you wanna show subtitles with</param>
    void putSubtitle(string title,GameObject go)
    {
        subtitle.SetActive(true);
        TextMesh t_mesh = subtitle.GetComponent<TextMesh>();
        t_mesh.text = title;
        t_mesh.text += "\n" + json.GetField("scenes")[sceneNumberIndex].GetField("description").str;
        int lineLength = 80;
        if (t_mesh.text.Length > lineLength)
        {
            t_mesh.text = StylingText(t_mesh.text, lineLength);
        }
        Vector3 pos = go.transform.localPosition;Vector3 rot = new Vector3(0,0,0); Vector3 scl = new Vector3(0.1f, 0.1f, 0.1f);
        SetTransform(subtitle, pos, rot, scl, "Subtitle");
        addHistory();
        sceneNumberIndex = searchingIndex(scene_next, "id", json);

    }

    public void anywayNext()
    {
        addHistory();
        scene_next = json.GetField("scenes")[sceneNumberIndex].GetField("next").str;
        sceneNumberIndex = searchingIndex(scene_next, "id", json);
    }

    public void OpenScene()
    {
        print("OpenScene");
        
        string type = json.GetField("scenes")[sceneNumberIndex].GetField("type").str;
        string FileName = json.GetField("scenes")[sceneNumberIndex].GetField(type).str ;
        string ext = Path.GetExtension(FileName);
        string FolderName = json.GetField("id").str;
        bool Is360 = (type == "image360" || type == "video360") ? true : false ;
        if (type == "bundle")
        {
            StartCoroutine(Put3dModel(FolderName, FileName));
        }
        else
        {
            switch (ext.ToLower())
            {
                case ".jpeg":
                case ".jpg":
                case ".png":
                case ".gif":
                case ".tif":
                case ".tiff":
                case ".bmp":
                    if (Is360)
                    {
                        //   StartCoroutine(Put360Images(FolderName, FileName));
                    }
                    else
                    {
                        StartCoroutine(PutImages(FolderName, FileName));
                    }
                    break;

                case ".txt":
                case ".json":
                case ".xml":
                    StartCoroutine(PutText(FolderName, FileName));
                    break;

                case ".fbx":
                    StartCoroutine(Put3dModel(FolderName, FileName));
                    break;

                case ".ogv":
                case ".mp4":

                    if (Is360)
                    {
                        Put360Movie(FolderName, FileName, movieMode.VideoPlayer);
                    }
                    else
                    {
                        PutMovie(FolderName, FileName, movieMode.VideoPlayer);
                    }

                    break;

                case ".mpg":
                case ".mpeg":
                    fileNotSupported(FileName);

                    break;


                default:
                    print("[Extention Not Matching]: " + ext);
                    break;
            }

        }
     
        if(sceneNumberIndex != -1)
        {
            if (json.GetField("scenes")[sceneNumberIndex].GetField("next_type").str.ToLower() == "simple")
            {
                addHistory();
                scene_next = json.GetField("scenes")[sceneNumberIndex].GetField("next").str;
            }
            else
            {
                
                gController.mode = GazeController.Mode.Choice;
            }
        }
        else
        {
            print("Argument out of range");
        }

    }
    #endregion
    #region  put assets
    public void addHistory()
    {
        if(IsHistory)
        {
            if(scene_history.Count >0)
            {
                string newWord = json.GetField("scenes")[sceneNumberIndex].GetField("id").str;
                foreach (string history in scene_history)
                {
                    if (history == newWord)
                    {
                        return;
                    }
                }
                scene_history.Add(newWord);
            }
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
        SetTransform(movieGo, _pos, _rot, _scale, "Objects");

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

        PutText("Now Loading...");

        print("nowLoading");
        int thisSceneNumber = sceneNumberIndex;
        yield return videoPlayer.url;

        videoPlayer.prepareCompleted += DeleteText;
        if (videoPlayer.isPrepared)
        {
            if (!videoPlayer.isPlaying)
            {
                videoPlayer.Play();
                putSubtitle(json.GetField("scenes")[thisSceneNumber].GetField("title").str, movieGo);
            }
        }

        

    }

    public void Put360Movie(string _folderName, string _fileName,movieMode mode)
    {

        print("Put Movie(" + _folderName + "," + _fileName + ")");

        string __path = global.appPath + "Data/" + _folderName + "/" + _fileName;
        __path = __path.Replace(" ", "%20");

        if (mode == movieMode.VideoPlayer)
        {
            StartCoroutine(movie360set(__path));
        }
        else
        {
           
        }
    }

    public GameObject go_transparent;
    public void fileNotSupported(string filename)
    {
        GameObject trans = go_transparent;
        Vector3 _pos = gController.posHitInfo; Vector3 _rot = new Vector3(-90, 0, 0); Vector3 _scale = new Vector3(-0.32f, 1f, -0.24f);

        trans.SetActive(true);
        SetTransform(trans, _pos, _rot, _scale, "Untagged");
        PutText("File name: "+filename +  "\nFile Type Not Supported");
        subtitle.SetActive(false);

        anywayNext();
    }
    public void PutMovie(string _folderName, string _fileName, movieMode mode)
    {

        print("Put Movie(" + _folderName + "," + _fileName + ")");

        string __path = global.appPath + "Data/" + _folderName + "/" + _fileName;
        __path = __path.Replace(" ", "%20");
        if (mode == movieMode.VideoPlayer)
        {
            StartCoroutine(movieset(__path));
        }
        else
        {
         
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
        int thisSceneNumber = sceneNumberIndex;

        yield return videoPlayer.url;
        print("nowLoading");
        GameObject movieGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Vector3 _pos = gController.posHitInfo; Vector3 _rot = new Vector3(-90, 0, 0); Vector3 _scale = new Vector3(-0.32f, 1f, -0.24f);
        SetTransform(movieGo, _pos, _rot, _scale, "Objects");
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

        putSubtitle(json.GetField("scenes")[thisSceneNumber].GetField("title").str, movieGo);
    }

    IEnumerator Put3dModel(string _folderName, string _fileName)
    {
        GameObject model;
        string filenameWithoutExt = Path.GetFileNameWithoutExtension(_fileName);
        string _path = global.appPath + "/Data/" + _folderName + "/" + filenameWithoutExt;
        _path = _path.Replace(" ", "%20");
        WWW www = new WWW(_path);
        int thisSceneNumber = sceneNumberIndex;
        yield return www;

        if (www.error == null)
        {
            if (www.isDone)
            {
                // Choose how to read the file (By assetBundles or by reading binary file)
                AssetBundle bundle = www.assetBundle;
                print(_fileName);
                    model = Instantiate(bundle.LoadAsset("model.fbx")) as GameObject;
                    bundle.Unload(false);

                    Vector3 _pos = gController.posHitInfo;
                    Vector3 _rot = new Vector3(0, 0, 0);
                    Vector3 _scale = new Vector3(0.001f, 0.001f, 0.001f);

                    SetTransform(model, _pos, _rot, _scale, "Objects");
                    putSubtitle(json.GetField("scenes")[thisSceneNumber].GetField("title").str, model);
            } 
        }
        else
        {
            print(www.error);
            if (www.error == "404 Not Found\r")
            {

                MessageNotFound404(_fileName);
                scene_next = json.GetField("scenes")[thisSceneNumber].GetField("next").str;
                sceneNumberIndex = searchingIndex(scene_next, "id", json);
            }
        }

 


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

        SetTransform(textGO, _pos, _rot, _scale,"Objects");
    }
    public void PutText(string text,Color _col)
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
        text3d.color = _col;
        Vector3 _pos = gController.posHitInfo;
        Vector3 _rot = new Vector3(0, 0, 0);
        Vector3 _scale = new Vector3(0.1f, 0.14f, 1f);

        SetTransform(textGO, _pos, _rot, _scale, "Objects");
    }
    IEnumerator PutText(string _foldername, string _filename)
    {
        print("Put Text(" + _foldername + "," + _filename + ")");
        GameObject textGO = Instantiate(textPrefab);
        TextMesh text3d = textGO.GetComponent<TextMesh>();
        Vector3 _pos = gController.posHitInfo;
        Vector3 _rot = new Vector3(0, 0, 0);
        Vector3 _scale = new Vector3(0.1f, 0.14f, 1f);
        string _path = global.appPath + "/Data/" + _foldername + "/" + _filename;
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


                SetTransform(textGO, _pos, _rot, _scale, "Objects");
            }
        }
        else
        {
            print(www.error);
            if (www.error == "404 Not Found\r")
            {
                MessageNotFound404(_filename);
            }
        }



    }

    void Put360Images(string _foldername, string _filename)
    {
        print("Put Image(" + _foldername + "," + _filename + ")");
        sphere.SetActive(true);
        GameObject imageGo = sphere;
        string __path = "Data/" + _foldername + "/" + _filename;
        Vector3 _pos = new Vector3(0, 0, -15f);
        Vector3 _rot = new Vector3(0, -90f, 0);
        Vector3 _scale = new Vector3(100f, 100f, -100f);

        Renderer rend = imageGo.GetComponent<Renderer>();
        Texture2D tex2d = new Texture2D(1, 1);
        tex2d = Resources.Load(__path) as Texture2D;


        SetTransform(imageGo, _pos, _rot, _scale, "Objects");
        sphere.tag = "Untagged";

        rend.material.mainTexture = tex2d;

        putSubtitle(json.GetField("scenes")[sceneNumberIndex].GetField("title").str, imageGo);
    }
    IEnumerator PutImages(string _foldername, string _filename)
    {
        print("Put Image(" + _foldername + "," + _filename + ")");
     
        string __path = global.appPath + "Data/" + _foldername + "/" + _filename;
        __path = __path.Replace(" ", "%20");
        int thisSceneNumber = sceneNumberIndex;
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

                putSubtitle(json.GetField("scenes")[thisSceneNumber].GetField("title").str, image);
            }
        }
        else
        {
         
            print(www.error);
            if (www.error == "404 Not Found\r")
            {
                
                MessageNotFound404(_filename);
            }
        }
        print("photo");

    }
    #endregion

    void SetTransform(GameObject go, Vector3 pos, Vector3 rot, Vector3 scale,string tag)
    {
        //name ;
        go.tag = tag;

        go.transform.localPosition = pos;

        //Rotation
        go.transform.localEulerAngles = rot;

        //Scale
        go.transform.localScale = scale;

    }
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

    #region text edit
    void DeleteText(VideoPlayer vp)
    {
        if (GameObject.Find("Text"))
        {
            print("delete text");

            Destroy(GameObject.Find("Text"));
        }

    }
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
           
        }

        if (_start < text.Length)
        {
            r_text += text.Substring(_start, text.Length - _start);
      
        }
        print(r_text);


        return r_text;
    }
    string StylingText(string text,int n)
    {
        char _space;
        string r_text = "";
        int _start = 0;
        int n_char = n-1, charRest = text.Length;

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
            n_char += n;
            charRest -= n_char - _start;
      
        }

        if (_start < text.Length)
        {
            r_text += text.Substring(_start, text.Length - _start);
            
        }


        print(r_text);
        return r_text;
    }
    #endregion 

    void MessageNotFound404(string name )
    {
        PutText("404 "+name +": Not Found. \nPlease confirm your file is in the proper folder.",Color.red);
    }
}
