using UnityEngine;
using System.Collections;

public class GlassBreak : Photon.MonoBehaviour {

	private GameObject glassObject;
	private Animator anim;

	void Start() {

		glassObject = gameObject;
		anim = glassObject.GetComponent<Animator>();

	}

	public void GlassBreakMethod() {

		photonView.RPC ("RPCGlassBreak", PhotonTargets.All);

	}

	[RPC] 
	void RPCGlassBreak() {

		anim.SetBool ("isBroken", true);
		glassObject.GetComponent<Collider2D>().enabled = false;

	}
}
