using UnityEngine;
using System.Collections;

public class SmokeBomb : Photon.MonoBehaviour {

	private Animator anim;

	void Start() {

		anim = GetComponent<Animator>();

	}

	void OnCollisionEnter2D (Collision2D col) {

		Collider2D collider = col.collider;

		if(collider.tag == "desertCityBuilding") {

			anim.SetBool("hitBuilding", true);
			gameObject.AddComponent<FadeAndDestroy>();

		}

	}

}
