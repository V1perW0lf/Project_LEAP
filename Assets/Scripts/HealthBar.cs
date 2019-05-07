using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealthBar : Photon.MonoBehaviour
{
	
	public GameObject cameraTarget; // playerPrefab
	public GameObject healthBar;	// healthBarPrefab
	private GameObject canvas;

	public List<GameObject> healthBarArray;
	private float numberOfHealthBars;	// Pretty obvious, isn't it?
	private float distBetweenHealth;	// The in game distance between health bars

	void OnLevelWasLoaded() {

		canvas = GameObject.Find("Canvas");

	}
	// Use this for initialization
	void Start()
	{
		distBetweenHealth = -0.5f;

		if(gameObject.name == ("duelist")) {
			numberOfHealthBars = 2;
		}
		else if(gameObject.name == ("nomad")) {
			numberOfHealthBars = 2;
		}
		else if(gameObject.name == ("gladiator")) {
			numberOfHealthBars = 2;
		}

		for(int i = 0; i < numberOfHealthBars; i++) {
			healthBarArray.Add (Instantiate(healthBar, new Vector3(500, 500, 0), Quaternion.identity) as GameObject);
			DontDestroyOnLoad(healthBarArray[i]);
		}
	}

	void Awake() {
		healthBarArray = new List<GameObject>(5);
	}
	
	// Update is called once per frame
	void Update()
	{
		if(Application.loadedLevelName != "insideShip") {
			for(int i = 0; i < numberOfHealthBars; i++) {
				healthBarArray[i].transform.position = new Vector3(cameraTarget.transform.position.x + (i + distBetweenHealth), cameraTarget.transform.position.y + 4);
			}
		}

		if(canvas != null) {
			if(canvas.activeSelf) {
				if(gameObject.activeSelf) {
					healthBarArray[0].SetActive(true);
					healthBarArray[1].SetActive(true);
				}
			} else {
				healthBarArray[0].SetActive(false);
				healthBarArray[1].SetActive(false);
			}
		}
		
	}

	public void SetHealthBarsToInactive() {
		for(int i = 0; i < numberOfHealthBars; i++) {
			if(healthBarArray[i].GetActive()) {
				healthBarArray[i].SetActive(false);
			} else {
				healthBarArray[i].SetActive(true);
			}
		}
	}

	public void fireLowerHealthRPC(int healthNum) {

		photonView.RPC("lowerHealth", PhotonTargets.All, healthNum);

	}

	[RPC] void lowerHealth(int health) {
		
		healthBarArray[health].GetComponent<Animator>().SetBool ("isEmpty", true);
		
	}

	void OnPlayerDisconnected(NetworkPlayer player) {
		Destroy (healthBarArray [0]);
		Destroy (healthBarArray [1]);
	}

	void OnDestroy() {
		Destroy (healthBarArray [0]);
		Destroy (healthBarArray [1]);
	}
	
}