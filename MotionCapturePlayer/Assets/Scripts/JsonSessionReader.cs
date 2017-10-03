// JSON recording session player
// 
// 
// Chen Chen
// 10/2/2017
// Open source under MIT License

using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using ViveMotionCapture;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class JsonSessionReader : ScriptableObject
{
	#region Session data variables

	[SerializeField]
	public static string jsonData;
	[SerializeField]
	public static TrackedData session;

	[SerializeField]
	public static float sessionTotalTime;
	[SerializeField]
	public static TrackedDevice[] deviceData;
	[SerializeField]
	public static string[] deviceName;
	[SerializeField]
	public static List<float> timeStamp;
	[SerializeField]
	public static int nDevices;
	[SerializeField]
	public static float averageFPS;

	#endregion

	[SerializeField]
	public static bool isJsonLoaded {
		get{ return !string.IsNullOrEmpty (jsonData); }
	}

	[SerializeField]
	public static bool isSessionLoaded {
		get{ return session != null; }
	}

	[MenuItem ("DataAsArt/Load Json")]
	public static void LoadJson ()
	{
		string path = EditorUtility.OpenFilePanel ("Select your json data file", "", "json");
		if (path.Length != 0) {
			WWW www = new WWW ("file:///" + path);

			if (string.IsNullOrEmpty (www.error)) {
				// Wait until finish
				while (!www.isDone) {
					EditorUtility.DisplayProgressBar ("Data As Art 2017", "Loading json file...", www.progress * 0.5f);
				}

				if (string.IsNullOrEmpty (www.text)) {
					EditorUtility.DisplayDialog ("Data As Art 2017", "Target file is empty, please double check your data file", "OK");
					return;
				} else {
					jsonData = www.text;
				}

				// Deserialize Json
				// Convert json file and load data
				EditorUtility.DisplayProgressBar ("Data As Art 2017", "Parsing json file...", 0.75f);
				session = JsonConvert.DeserializeObject <TrackedData> (jsonData);

				// Read misc items
				EditorUtility.DisplayProgressBar ("Data As Art 2017", "Load misc items...", 0.9f);
				sessionTotalTime = session.timeStamp [session.Count - 1];
				deviceData = session.trackedDevice.Values.ToArray ();
				deviceName = session.trackedDevice.Keys.ToArray ();
				timeStamp = session.timeStamp;
				nDevices = session.trackedDevice.Count;
				averageFPS = sessionTotalTime / timeStamp.Count;

				EditorUtility.DisplayProgressBar ("Data As Art 2017", "All done", 1f);
				EditorUtility.ClearProgressBar ();

				Debug.Log ("Successfully loaded session with " + nDevices + " captured devices, recording length " + sessionTotalTime + " seconds");
				EditorUtility.DisplayDialog ("Data As Art 2017", 
					"Successfully loaded session with " + nDevices + " captured devices, recording length " + sessionTotalTime + " seconds", 
					"Start Replay");
				SessionPlayback._Instance.StartPlay ();
			} else {
				EditorUtility.DisplayDialog ("Data As Art 2017", "Target file is not valid", "OK");
				EditorApplication.isPlaying = false;
			}
		} else {
			// Target path is not valid
			EditorUtility.DisplayDialog ("Data As Art 2017", "Target path is not valid", "OK");
			EditorApplication.isPlaying = false;
			return;
		}
	}

	void OnDestroy ()
	{
		Debug.Log ("Destroyed!");
	}
}