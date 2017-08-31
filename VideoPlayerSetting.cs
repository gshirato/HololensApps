using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
public class VideoPlayerSetting : MonoBehaviour {

    VideoPlayer vp;
    public VideoClip vc;
    AudioSource audioSource;

	// Use this for initialization
	void Start () {
        Application.runInBackground = true;
        //PlayVideo(Application.streamingAssetsPath + "/Resources/Data/Project 3/movies/1_demo.ogv");
        //PlayVideo(Application.dataPath + "/Assets/000Store/3_MegaCoaster.mp4");
        PlayVideo();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void PlayVideo()
    {
        //GameObject camera = GameObject.Find("HoloLensCamera");
        GameObject camera = GameObject.Find("HoloLensCamera");
        vp = camera.AddComponent<VideoPlayer>();
        audioSource = this.gameObject.AddComponent<AudioSource>();
        vp.controlledAudioTrackCount = 1;
        vp.renderMode = VideoRenderMode.CameraNearPlane;
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
 
       // vp.url = _url;
        
        vp.isLooping = true;

        //vp.EnableAudioTrack(0, true);
        vp.SetTargetAudioSource(0, audioSource);
        vp.clip = vc;

        vp.loopPointReached += Loop;
        //vp.Prepare();

        //audioSource.Play();

        vp.Play();


        
    }


    void Loop(VideoPlayer vp)
    {
        print("Loop");
    }
}

