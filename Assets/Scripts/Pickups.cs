using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Pickups : MonoBehaviour {

	private Image[] images;
	private Text[] texts;
	private PlayerControl playerStats;
	public Sprite knife;
	public Sprite smokeBomb;
	
	void Start () {

		images = FindObjectOfType<Canvas>().GetComponentsInChildren<Image>();
		texts = FindObjectOfType<Canvas>().GetComponentsInChildren<Text>();
		for(int i = 0; i < GameObject.FindGameObjectsWithTag("Player").Length; i++) {
			Physics2D.IgnoreCollision(GetComponent<BoxCollider2D>(), GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<Collider2D>());
		}
	
	}

	void OnTriggerEnter2D(Collider2D collider) {

		if(collider.gameObject.tag == "Player") {
			
			playerStats = collider.gameObject.GetComponent<PlayerControl>();
			
			if(playerStats.hasKnife == false && playerStats.hasSmokeBomb == false) {
				
				if(gameObject.tag == "KnifePickup") {
					
					gameObject.SetActive(false);
					
					collider.gameObject.GetComponent<PlayerControl>().hasKnife = true;
					
					if(collider.gameObject.GetComponent<PhotonView>().isMine) {
						
						foreach (Image image in images) {
							if(image.name == "Equip") {
								image.enabled = true;
								image.sprite = knife;
							}
						}
						
						foreach (Text text in texts) {
							if(text.name == "EquipCurrent") {
								text.text = "1";
							}
						}
						
					}
					
				}
				
				else if(gameObject.tag == "SmokeBombPickup") {
					
					gameObject.SetActive(false);
					
					collider.gameObject.GetComponent<PlayerControl>().hasSmokeBomb = true;
					
					if(collider.gameObject.GetComponent<PhotonView>().isMine) {
						
						foreach (Image image in images) {
							if(image.name == "Equip") {
								image.enabled = true;
								image.sprite = smokeBomb;
							}
						}
						
						foreach (Text text in texts) {
							if(text.name == "EquipCurrent") {
								text.text = "1";
							}
						}
						
					}
					
				}
				
			}
			
		}
		
	}

	/*void OnCollisionEnter2D (Collision2D col) {
		
		Collider2D collider = col.collider;

		if(collider.gameObject.tag == "Player") {

			playerStats = collider.gameObject.GetComponent<PlayerControl>();

			if(playerStats.hasKnife == false && playerStats.hasSmokeBomb == false) {
		
				if(gameObject.tag == "KnifePickup") {
					
					gameObject.SetActive(false);
					
					collider.gameObject.GetComponent<PlayerControl>().hasKnife = true;
					
					if(collider.gameObject.GetComponent<PhotonView>().isMine) {
						
						foreach (Image image in images) {
							if(image.name == "Equip") {
								image.enabled = true;
								image.sprite = knife;
							}
						}
						
						foreach (Text text in texts) {
							if(text.name == "EquipCurrent") {
								text.text = "1";
							}
						}
						
					}
					
				}

				else if(gameObject.tag == "SmokeBombPickup") {
					
					gameObject.SetActive(false);
					
					collider.gameObject.GetComponent<PlayerControl>().hasSmokeBomb = true;
					
					if(collider.gameObject.GetComponent<PhotonView>().isMine) {
						
						foreach (Image image in images) {
							if(image.name == "Equip") {
								image.enabled = true;
								image.sprite = smokeBomb;
							}
						}
						
						foreach (Text text in texts) {
							if(text.name == "EquipCurrent") {
								text.text = "1";
							}
						}
						
					}
					
				}

			}

		}
		
	}*/

}
