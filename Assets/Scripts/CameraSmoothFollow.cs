using UnityEngine;
using System.Collections;

public class CameraSmoothFollow : Photon.MonoBehaviour
{
	
	public GameObject cameraTarget; // object to look at or follow
	
	public float smoothTime = 0.1f; // time for dampen
	public Vector2 velocity; 		// speed of camera movement
	public Vector2 maxXAndY;		// The maximum x and y coordinates the camera can have.
	public Vector2 minXAndY;		// The minimum x and y coordinates the camera can have.
	
	public float shake = 0f;
	private float shakeAmount = 15f;
	private float decreaseFactor = 1.0f;
	private Vector3 posHolder;

	void Start()
	{
		if(Application.loadedLevelName == "Skyline") {
			maxXAndY = new Vector2(314f, 20f);
			minXAndY = new Vector2(-396.5f, -35.5f);
		} else if(Application.loadedLevelName == "Desert City") {
			maxXAndY = new Vector2(141f, 44f);
			minXAndY = new Vector2(-132.8f, 13f);
		}
	}
	
	void Update()
	{

		if (shake > 0) {
			posHolder = Random.insideUnitSphere * shakeAmount;
			transform.position = new Vector3(posHolder.x, posHolder.y, -10);
			shake -= Time.deltaTime * decreaseFactor;
			
		} else {
			shake = 0.0f;
		}

		if(GameObject.FindGameObjectsWithTag ("Player") != null) {
			for(int i = 0; i < GameObject.FindGameObjectsWithTag ("Player").Length; i++) {
				if(GameObject.FindGameObjectsWithTag("Player")[i] != null && GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<PhotonView>() != null) {
					if(GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<PhotonView>().isMine) {
						cameraTarget = GameObject.FindGameObjectsWithTag("Player")[i];
					}
				}
			}
		}

		if(cameraTarget != null) {

			float targetX = cameraTarget.transform.position.x;
			float targetY = cameraTarget.transform.position.y;

			targetX = Mathf.Clamp(targetX, minXAndY.x, maxXAndY.x);
			targetY = Mathf.Clamp(targetY, minXAndY.y, maxXAndY.y);

			transform.position = new Vector3(Mathf.SmoothDamp(transform.position.x, targetX, ref velocity.x, smoothTime), Mathf.SmoothDamp(transform.position.y, targetY, ref velocity.y, smoothTime), transform.position.z);
		}
	}

	void OnPlayerDisconnected(NetworkPlayer player) {
		if(GetComponent<NetworkView>().isMine) {
			Network.RemoveRPCs(player);
			Network.DestroyPlayerObjects(player);
		}
	}
}
