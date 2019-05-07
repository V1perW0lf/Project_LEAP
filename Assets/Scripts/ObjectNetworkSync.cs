using UnityEngine;
using System.Collections;

public class ObjectNetworkSync : Photon.MonoBehaviour {

	private Vector3 realPos;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if(photonView.isMine) {

			
		} else {
			
			transform.position = Vector3.Lerp(transform.position, realPos, Time.deltaTime * 50f);
			
		}
	
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {

		realPos = Vector3.zero;
		
		if (stream.isWriting) {
			
			stream.SendNext(transform.position);
			stream.SendNext(GetComponent<Rigidbody2D>().velocity);

		}
		else {
			
			realPos = (Vector3)stream.ReceiveNext();
			GetComponent<Rigidbody2D>().velocity = (Vector2)stream.ReceiveNext();
			
		}

	}

}
