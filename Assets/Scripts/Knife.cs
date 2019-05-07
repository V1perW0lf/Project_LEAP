using UnityEngine;
using System.Collections;

public class Knife : Photon.MonoBehaviour {

	private Vector3 knifePos;
	private Vector3 trueKnifePos;
	private GameObject playerHit;
	private bool buildingHit;
	private Animator healthBar1Anim;
	private GameObject glass;

	void Update() {

		if(playerHit != null) {

			trueKnifePos = playerHit.transform.position - knifePos;
			gameObject.transform.position = trueKnifePos;
			gameObject.transform.rotation = Quaternion.identity;

			if(playerHit.activeSelf == false) {
				PhotonNetwork.Destroy(gameObject);
			}

		} else if(buildingHit) {
			gameObject.transform.position = knifePos;
			gameObject.transform.rotation = Quaternion.identity;
		}

	}

	void OnCollisionEnter2D (Collision2D col) {
		
		Collider2D collider = col.collider;

		if(collider.tag == "Player" && GetComponent<Animator>().enabled) {

			knifePos = new Vector3(col.transform.position.x - gameObject.transform.position.x, col.transform.position.y - gameObject.transform.position.y, 0);

			playerHit = col.gameObject;

			gameObject.GetComponent<Collider2D>().enabled = false;

			if(collider.gameObject.GetComponent<HitBoxManager>().localShield < 700) {

				healthBar1Anim = collider.gameObject.GetComponent<HealthBar>().healthBarArray[1].GetComponent<Animator>();
				
				if(healthBar1Anim.GetBool("isEmpty") == false) {
					
					healthBar1Anim.SetBool("isEmpty", true);
					
				} else {

					playerHit.GetComponent<HealthBar>().healthBarArray[0].SetActive(false);
					playerHit.GetComponent<HealthBar>().healthBarArray[1].SetActive(false);
					playerHit.GetComponent<HitBoxManager>().DeadUI();
					playerHit.SetActive(false);
					
				}
				
			} else {
				
				playerHit.GetComponent<HitBoxManager>().localShield = 0;
				
			}
			
		}

		else if(collider.tag == "Glass") {

			glass = col.gameObject;
			glass.GetComponent<GlassBreak>().GlassBreakMethod();

		}

		else {

			gameObject.GetComponent<Collider2D>().enabled = false;
			knifePos = transform.position;
			buildingHit = true;

		}

		gameObject.AddComponent<FadeAndDestroy>();
		gameObject.GetComponent<Animator>().enabled = false;

	}

}
