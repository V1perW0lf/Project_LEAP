using UnityEngine;
using System.Collections;

public class GameManager : Photon.MonoBehaviour {
	
	// this is a object name (must be in any Resources folder) of the prefab to spawn as player avatar.
	// read the documentation for info how to spawn dynamically loaded game objects at runtime (not using Resources folders)
	private string playerPrefabName;
	public string playerObjectName;
	private GameObject player;
	private GameObject canvas;
	private GameObject[] players;


	void Start() {

		canvas = GameObject.Find("Canvas");

	}
	void OnJoinedRoom() {

		StartGame();

	}

	void Update() {

		if(Application.loadedLevelName == "insideShip") {

			players = GameObject.FindGameObjectsWithTag("Player");
			
			for(int i = 0; i < players.Length; i++) {
				
				if(players[i].GetComponent<PlayerInfo>().playerNumber == 1) {

					players[i].transform.position = transform.position;
					
				}
				else if(players[i].GetComponent<PlayerInfo>().playerNumber == 2) {
					
					players[i].transform.position = transform.position + new Vector3(5, 0, 0);
					
				}
				else if(players[i].GetComponent<PlayerInfo>().playerNumber == 3) {
					
					players[i].transform.position = transform.position + new Vector3(10, 0, 0);
					
				}
				else if(players[i].GetComponent<PlayerInfo>().playerNumber == 4) {
					
					players[i].transform.position = transform.position + new Vector3(15, 0, 0);
					
				}
				
			}
			
		}

	}

	void OnPhotonPlayerDisconnected() {

		StartCoroutine("onPlayerLeft");

	}

	IEnumerator onPlayerLeft() {

		yield return new WaitForSeconds(0.3f);

		if(Application.loadedLevelName == "insideShip") {
			
			players = GameObject.FindGameObjectsWithTag("Player");
			
			for(int i = 0; i < players.Length; i++) {

				if(PhotonNetwork.playerList.Length == 3) {
					if(players[i].GetComponent<PlayerInfo>().playerNumber == 1) {
						for(int g = 0; g < players.Length; g++) {
							if(players[g].GetComponent<PlayerInfo>().playerNumber == 2) {
								for(int h = 0; h < players.Length; h++) {
									if(players[h].GetComponent<PlayerInfo>().playerNumber == 4) {
										players[h].GetComponent<PlayerInfo>().playerNumber = 3;
									}
								}
							}
							else if(players[g].GetComponent<PlayerInfo>().playerNumber == 3) {
								for(int h = 0; h < players.Length; h++) {
									if(players[h].GetComponent<PlayerInfo>().playerNumber == 4) {
										players[g].GetComponent<PlayerInfo>().playerNumber = 2;
										players[h].GetComponent<PlayerInfo>().playerNumber = 3;
									}
								}
							}
						}
					}
			
					else if(players[i].GetComponent<PlayerInfo>().playerNumber == 2) {
						for(int g = 0; g < players.Length; g++) {
							if(players[g].GetComponent<PlayerInfo>().playerNumber == 3) {
								for(int h = 0; h < players.Length; h++) {
									if(players[h].GetComponent<PlayerInfo>().playerNumber == 4) {
										players[i].GetComponent<PlayerInfo>().playerNumber = 1;
										players[g].GetComponent<PlayerInfo>().playerNumber = 2;
										players[h].GetComponent<PlayerInfo>().playerNumber = 3;
									}
								}
							}
						}

					}
					
				}

				else if(PhotonNetwork.playerList.Length == 2) {

					if(players[i].GetComponent<PlayerInfo>().playerNumber == 1) {
						for(int g = 0; g < players.Length; g++) {
							if(players[g].GetComponent<PlayerInfo>().playerNumber == 3) {
									players[g].GetComponent<PlayerInfo>().playerNumber = 2;
							}
						}
					}

					else if(players[i].GetComponent<PlayerInfo>().playerNumber == 2) {
						for(int g = 0; g < players.Length; g++) {
							if(players[g].GetComponent<PlayerInfo>().playerNumber == 3) {
								players[i].GetComponent<PlayerInfo>().playerNumber = 1;
								players[g].GetComponent<PlayerInfo>().playerNumber = 2;
							}
						}
					}

				}

				else if(PhotonNetwork.playerList.Length == 1) {

					if(players[i].GetComponent<PlayerInfo>().playerNumber == 2) {
						players[i].GetComponent<PlayerInfo>().playerNumber = 1;
					}

				}
				
			}
			
		}
	}

	IEnumerator OnLeftRoom() {
		//Easy way to reset the level: Otherwise we'd manually reset the camera
		
		//Wait until Photon is properly disconnected (empty room, and connected back to main server)
		while(PhotonNetwork.room!=null || PhotonNetwork.connected==false)
			yield return 0;
		
		Application.LoadLevel("insideShip");
		
	}
	
	void StartGame() {
		//Camera.main.farClipPlane = 1000; //Main menu set this to 0.4 for a nicer BG    
		
		//prepare instantiation data for the viking: Randomly diable the axe and/or shield
		bool[] enabledRenderers = new bool[2];
		enabledRenderers[0] = Random.Range(0,2)==0;//Axe
		enabledRenderers[1] = Random.Range(0, 2) == 0; ;//Shield
		
		object[] objs = new object[1]; // Put our bool data in an object array, to send
		objs[0] = enabledRenderers;

	}
	
	void OnGUI() {
		if (PhotonNetwork.room == null) return; //Only display this GUI when inside a room

		if(canvas != null) {
			if(canvas.activeSelf) {
				if (GUILayout.Button("Leave Room"))
				{
					PhotonNetwork.LeaveRoom();
				}
			}
		} else if(Application.loadedLevelName == "insideShip") {

			if (GUILayout.Button("Leave Room"))
			{
				PhotonNetwork.LeaveRoom();
			}
			
		}

		if(player == null && Application.loadedLevelName == "insideShip") {

			if(GUI.Button(new Rect(Screen.width / 3, Screen.height / 3, 100, 100), "Nomad")) {
				playerPrefabName = "nomadPrefab";
				SpawnPlayer();
				playerObjectName = "nomad";
			} else if(GUI.Button(new Rect(Screen.width / 2, Screen.height / 3, 100, 100), "Duelist")) {
				playerPrefabName = "duelistPrefab";
				SpawnPlayer();
				playerObjectName = "duelist";
			}
			else if(GUI.Button(new Rect((Screen.width / 2) + 125, Screen.height / 3, 100, 100), "Gladiator")) {
				playerPrefabName = "gladiatorPrefab";
				SpawnPlayer();
				playerObjectName = "gladiator";
			}
		}

	}

	void SpawnPlayer() {

		if(PhotonNetwork.isMasterClient) {
			player = PhotonNetwork.Instantiate(this.playerPrefabName, transform.position, Quaternion.identity, 0);
		} else if(PhotonNetwork.playerList.Length == 2){
			player = PhotonNetwork.Instantiate(this.playerPrefabName, transform.position + new Vector3(5, 0, 0), Quaternion.identity, 0);
		} else if(PhotonNetwork.playerList.Length == 3){
			player = PhotonNetwork.Instantiate(this.playerPrefabName, transform.position + new Vector3(10, 0, 0), Quaternion.identity, 0);
		} else if(PhotonNetwork.playerList.Length == 4){
			player = PhotonNetwork.Instantiate(this.playerPrefabName, transform.position + new Vector3(15, 0, 0), Quaternion.identity, 0);
		}

	}
	
	void OnDisconnectedFromPhoton() {
		//Debug.LogWarning("OnDisconnectedFromPhoton");
	}    
	
}