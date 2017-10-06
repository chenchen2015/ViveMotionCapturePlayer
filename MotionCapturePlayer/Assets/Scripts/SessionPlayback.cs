// JSON recording session player
// 
// 
// Chen Chen
// 10/2/2017
// Open source under MIT License

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System;
using ViveMotionCapture;
using UnityEditor;

public class SessionPlayback : MonoBehaviour
{
	public static SessionPlayback _Instance;

	#region Playback state variables

	private bool isPlaying = false;
	private bool pausePlay = false;
	private float playbackStartTime;
	int currentFrame = 0;

	private float currentPlaybackTime {
		get{ return Time.time - playbackStartTime; }
	}

	private bool isSessionLoaded {
		get{ return JsonSessionReader.isSessionLoaded; }
	}

	#endregion

	#region Playback data variables

	private GameObject[] pbGameObj;
	private Transform[] pbTransform;
	private GameObject parentGameObj;

	#endregion

	#region Playback visual related parameters

	private readonly Vector3 pbObjScale = new Vector3 (0.025f, 0.05f, 0.025f);
	private readonly Vector3 cameraOffset = new Vector3 (-0f, 0f, -2f);
	private Vector3 meanPosition;
	private Camera mainCamera;

	#endregion

	void Awake ()
	{
		// Store instance
		_Instance = this;
	}

	void Start ()
	{
		mainCamera = Camera.main;

		if (!JsonSessionReader.isSessionLoaded) {
			JsonSessionReader.LoadJson ();
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			if (isPlaying) {
				pausePlay = !pausePlay;
				PausePlay (pausePlay);
			} else {
				StartPlay ();
			}
		}
	}

	#region Playback helper functions

	public void PausePlay (bool pauseStatus)
	{
		if (pauseStatus) {
			// if resumed playback
			CancelInvoke ("FixedFPSReplay");
			Debug.Log ("Playback paused");
		} else {
			// if resumed playback
			InvokeRepeating ("FixedFPSReplay", 0.0f, 0.5f / JsonSessionReader.averageFPS);
			Debug.Log ("Playback resumed");
		}
	}

	public void StartPlay ()
	{
		InitGameObjects ();

		playbackStartTime = Time.time;
		pausePlay = false;
		currentFrame = 0;
		isPlaying = true;
		InvokeRepeating ("FixedFPSReplay", 0.0f, 1f / 50f);
		Debug.Log ("Playback started");
	}

	public void StopPlay ()
	{
		isPlaying = false;
		CancelInvoke ("FixedFPSReplay");
		pausePlay = false;
		playbackStartTime = 0.0f;

		// Destroy instantiated game objects
		foreach (GameObject go in pbGameObj) {
			Destroy (go);
		}
		Destroy (parentGameObj);
		parentGameObj = null;
		pbGameObj = null;
		pbTransform = null;

		Debug.Log ("Playback finished");
	}

	void InitGameObjects ()
	{
		pbGameObj = new GameObject[JsonSessionReader.nDevices];
		pbTransform = new Transform[JsonSessionReader.nDevices];
		meanPosition = Vector3.zero;

		// Instantiate parent
		parentGameObj = new GameObject ("Tracked Devices (Your captured devices are here)");
		parentGameObj.transform.SetPositionAndRotation (Vector3.zero, Quaternion.identity);

		int idx = 0;
		foreach (string td in JsonSessionReader.deviceName) {
			// Instantiate visual game object and store reference
			pbGameObj [idx] = GameObject.CreatePrimitive (PrimitiveType.Cube);
			pbTransform [idx] = pbGameObj [idx].transform;
			pbTransform [idx].parent = parentGameObj.transform;

			// Rename gameobject
			pbGameObj [idx].name = td;

			// Initialize visuals
			pbTransform [idx].localPosition = Vector3.zero;
			pbTransform [idx].localScale = pbObjScale;

			idx += 1;
		}
	}

	void FixedFPSReplay ()
	{
		if (isPlaying && isSessionLoaded) {
			// Check if playback is done
			if (currentPlaybackTime >= JsonSessionReader.sessionTotalTime || currentFrame >= (JsonSessionReader.timeStamp.Count - 1)) {
				StopPlay ();
				return;
			}

			meanPosition = Vector3.zero;
			// Loop through all devices
			for (int i = 0; i < JsonSessionReader.nDevices; i++) {
				pbTransform [i].position = SerializedVec3.ToVec3 (JsonSessionReader.deviceData [i].position [currentFrame]);
				pbTransform [i].rotation = SerializedVec3.ToQuat (JsonSessionReader.deviceData [i].rotation [currentFrame]);

				meanPosition += pbTransform [i].position;
			}

			mainCamera.transform.position = meanPosition / JsonSessionReader.nDevices + cameraOffset;

			if (currentFrame % 100 == 0) {
				Debug.Log (String.Format ("Current replay time {0:F3} seconds", currentPlaybackTime));
			}

			currentFrame += 2;
		}
	}

	#endregion
}
