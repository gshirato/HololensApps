using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour {

    public string appPath = "http://hololens-training-app.azurewebsites.net/";
    public GameObject textPrefab;
    public float f = 0;
    public bool IsDebugMode = false;
    public void PutText(string text)
    {
        if(IsDebugMode)
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

            Vector3 _pos = new Vector3(-3, f, 0);
            Vector3 _rot = new Vector3(0, 0, 0);
            Vector3 _scale = new Vector3(0.1f, 0.14f, 1f);

            SetTransform(textGO, _pos, _rot, _scale);
            f += 0.5f;
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
            print(r_text);
        }

        if (_start < text.Length)
        {
            r_text += text.Substring(_start, text.Length - _start);
            print(r_text);
        }



        return r_text;
    }
    void SetTransform(GameObject go, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        //name ;
        go.tag = "Debug";

        go.transform.localPosition = pos;

        //Rotation
        go.transform.localEulerAngles = rot;

        //Scale
        go.transform.localScale = scale;

    }
}
