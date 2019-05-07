using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerName : MonoBehaviour {

	private string playerName;

	// Use this for initialization
	void Start () {
		playerName = PlayerPrefs.GetString("playerName");
		GameObject.Find("PlayerName").GetComponentInChildren<Text>().text = playerName;
	}

}
