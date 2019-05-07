using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Respawn : Photon.MonoBehaviour {

	private GameObject player;
	public bool isAlive;
	public List<GameObject> extraPieces;
	private string causeOfDeath;

	void Start() {

		isAlive = true;

		DontDestroyOnLoad(gameObject);

		if(gameObject.name == "nomadPrefab(Clone)") {
			player = transform.FindChild ("nomad").gameObject;
		} else if(gameObject.name == "duelistPrefab(Clone)") {
			player = transform.FindChild ("duelist").gameObject;
		}
		else if(gameObject.name == "gladiatorPrefab(Clone)") {
			player = transform.FindChild ("gladiator").gameObject;
		}

	}

	void Update() {

		if(player.activeSelf == false) {

			if(player.name == "nomad") {
				if(transform.FindChild("nomad_child2_child2") != null) {
					extraPieces.Add (transform.FindChild("nomad_child1").gameObject);
					extraPieces.Add (transform.FindChild("nomad_child2").gameObject);
					extraPieces.Add (transform.FindChild("nomad_child1_child1").gameObject);
					extraPieces.Add (transform.FindChild("nomad_child1_child2").gameObject);
					extraPieces.Add (transform.FindChild("nomad_child2_child1").gameObject);
					extraPieces.Add (transform.FindChild("nomad_child2_child2").gameObject);
				}
				for(int i = 0; i < extraPieces.Count; i++) {
					Destroy(extraPieces[i]);
				}
			}
			else if(player.name == "duelist") {
				if(transform.FindChild("duelist_child2_child2") != null) {
					extraPieces.Add (transform.FindChild("duelist_child1").gameObject);
					extraPieces.Add (transform.FindChild("duelist_child2").gameObject);
					extraPieces.Add (transform.FindChild("duelist_child1_child1").gameObject);
					extraPieces.Add (transform.FindChild("duelist_child1_child2").gameObject);
					extraPieces.Add (transform.FindChild("duelist_child2_child1").gameObject);
					extraPieces.Add (transform.FindChild("duelist_child2_child2").gameObject);
				}
				for(int i = 0; i < extraPieces.Count; i++) {
					Destroy(extraPieces[i]);
				}
			}
			else if(player.name == "gladiator") {
				if(transform.FindChild("gladiator_child2_child2") != null) {
					extraPieces.Add (transform.FindChild("gladiator_child1").gameObject);
					extraPieces.Add (transform.FindChild("gladiator_child2").gameObject);
					extraPieces.Add (transform.FindChild("gladiator_child1_child1").gameObject);
					extraPieces.Add (transform.FindChild("gladiator_child1_child2").gameObject);
					extraPieces.Add (transform.FindChild("gladiator_child2_child1").gameObject);
					extraPieces.Add (transform.FindChild("gladiator_child2_child2").gameObject);
				}
				for(int i = 0; i < extraPieces.Count; i++) {
					Destroy(extraPieces[i]);
				}
			}

		}

		if(isAlive == false && player.activeSelf == true) {

			GetComponentInChildren<HitBoxManager>().ApplyUnrecievedDeath(causeOfDeath);
			
		}

	}
	void OnGUI() {

		if(player.activeSelf == false && photonView.isMine) {
			if(GUI.Button(new Rect(Screen.width / 2, Screen.height / 2, 100, 100), "Respawn")) {
				photonView.RPC ("RespawnPlayer", PhotonTargets.All);
			}
		}

	}

	[RPC] void RespawnPlayer() {
		photonView.RPC ("Alive", PhotonTargets.All);
		player.SetActive(true);
		player.GetComponent<HealthBar>().healthBarArray[0].SetActive(true);
		player.GetComponent<HealthBar>().healthBarArray[1].SetActive(true);
		player.GetComponent<HitBoxManager>().isRespawning = true;
		player.GetComponent<PlayerControl>().canDash = true;
		player.GetComponent<PlayerControl>().canDodge = true;
		player.transform.position = new Vector2(156, 24);
	}

	[RPC] void Alive() {

		isAlive = true;

	}

	[RPC] void Dead(string death) {

		//Debug.Log("he's dead jim");
		causeOfDeath = death;
		isAlive = false;
		
	}

	public void FireDeadRPC(string death) {

		photonView.RPC ("Dead", PhotonTargets.All, death);

	}
	
}
