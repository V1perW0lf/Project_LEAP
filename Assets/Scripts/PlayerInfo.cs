using UnityEngine;
using System.Collections;

public class PlayerInfo : Photon.MonoBehaviour {

	[HideInInspector]
	public float playerNumber;

	private string playerName;
	public Font playerNameFont;
	private string otherPlayerName;

	private float playerNameLength;

	private GameObject[] players;
	private GameObject canvas;

	private Vector3 screenPosition;

	void OnLevelWasLoaded() {

		canvas = GameObject.Find("Canvas");

	}

	void Update() {

		if(photonView.isMine == false && playerName != otherPlayerName) {
			playerName = otherPlayerName;
			playerNameLength = playerName.Length * 3.2f;
		}

	}

	void Start () {

		if(photonView.isMine) {
			playerName = PlayerPrefs.GetString("playerName");
			playerNameLength = playerName.Length * 3.2f;
		}

		players = GameObject.FindGameObjectsWithTag("Player");

		if(players.Length == 1) {
			playerNumber = 1;
		}
		else if(players.Length == 2) {
			playerNumber = 2;
		}
		else if(players.Length == 3) {
			playerNumber = 3;
		}
		else if(players.Length == 4) {
			playerNumber = 4;
		}

	}

	void OnGUI() {

		GUI.skin.font = playerNameFont;

		if(canvas != null) {
			if(canvas.activeSelf) {
				float guiModifier = (float)Screen.height / 600f;
				screenPosition = Camera.main.WorldToScreenPoint(transform.position);
				screenPosition.y = Screen.height - screenPosition.y;
				GUI.Label(new Rect(screenPosition.x - (playerNameLength * guiModifier), screenPosition.y - (105 * guiModifier), 50, 50), playerName, GUIStyle.none);
			}
		} else if(Application.loadedLevelName == "insideShip") {
			float guiModifier = (float)Screen.height / 600f;
			screenPosition = Camera.main.WorldToScreenPoint(transform.position);
			screenPosition.y = Screen.height - screenPosition.y;
			GUI.Label(new Rect(screenPosition.x - (playerNameLength * guiModifier), screenPosition.y - (105 * guiModifier), 50, 50), playerName, GUIStyle.none);
		}

	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {

		if (stream.isWriting)
		{
			stream.SendNext(playerName);
		}
		else
		{
			otherPlayerName = (string)stream.ReceiveNext();
		}

	}
}
