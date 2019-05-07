using UnityEngine;
using System.Collections;

public class ChangeScene : Photon.MonoBehaviour {

	private GameObject player;

	void Start() {
		PhotonNetwork.automaticallySyncScene = true;
	}

	void Update() {
		if(player == null) {
			player = GameObject.FindGameObjectWithTag("Player");
		}
	}

	void LoadSkyline() {
		PhotonNetwork.LoadLevel ("Skyline");
	}

	void OnGUI() {

		if (PhotonNetwork.room == null) return;

		if(PhotonNetwork.isMasterClient && player != null) {
			if(GUI.Button(new Rect(Screen.width - 100, 0, 100, 100), "load Skyline")) {
				LoadSkyline();
			}
		}
	}
	
}