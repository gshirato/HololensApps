using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovieTest : MonoBehaviour {


    public MovieTexture movie;
    private AudioSource audioSource;


	// Use this for initialization
	void Start () {
        GetComponent<MeshRenderer>().material.mainTexture = movie;
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = movie.audioClip;

        movie.loop = false;

        movie.Play();

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
