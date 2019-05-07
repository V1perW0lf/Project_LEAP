using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HitBoxManager : Photon.MonoBehaviour {

	[HideInInspector] public bool isRespawning;
	
	// Player bodies
	public PolygonCollider2D idle;
	public PolygonCollider2D jump;
	public PolygonCollider2D falling;
	public PolygonCollider2D running;
	public PolygonCollider2D wallGrab;
	public PolygonCollider2D downAttackBody;

	// Attacks
	public PolygonCollider2D downAttack;
	public PolygonCollider2D groundAttack;
	public PolygonCollider2D midAirAttack1;
	public PolygonCollider2D midAirAttack2;

	public Sprite healthBarFull;
	public Sprite healthBarEmpty;

	public float localShield;

	private GameObject glass;
	private GameObject canvas;
	private GameObject healthBar0;
	private GameObject healthBar1;
	private GameObject playerPrefab;
	private GameObject colliderGameObject;

	private Animator anim;
	private Animator glassAnim;
	private Animator healthAnim;
	private Animator colliderAnimator;
	
	private Image[] images;
	private Image healthBarImage; 

	private Text[] texts;
	private Text healthCurrent;

	private Texture health;
	private Texture healthBackground;

	private float colliderShield;
	
	private Slider shield;

	private GameObject[] players;
	private GameObject[] playerPrefabs;

	private Vector3 playerPos;
	private Vector3 screenPosition;
	
	private Vector2 colliderPosition;

	// Collider on this game object
	private PolygonCollider2D localCollider;
	
	// Used for organization
	private PolygonCollider2D[] collidersAttack;

	private List<SpriteSlicer2DSliceInfo> SlicedSpriteInfo = new List<SpriteSlicer2DSliceInfo>();
	
	// We say box, but we're still using polygons.
	public enum hitBoxes
	{
		groundAttackBox,
		midAirAttackBox1,
		midAirAttackBox2,
		downAttack,
		clear, // special case to remove all boxes
	}

	void OnLevelWasLoaded() {

		images = FindObjectOfType<Canvas>().GetComponentsInChildren<Image>();
		texts = FindObjectOfType<Canvas>().GetComponentsInChildren<Text>();
		shield = FindObjectOfType<Canvas>().GetComponentInChildren<Slider>();
		InvokeRepeating("rechargeShield", 0.000001f, 0.1f);
		canvas = GameObject.Find("Canvas");

		KeepPlayersGhosts();

	}

	void rechargeShield () {
		localShield += 10f;
		if(localShield > 700) {
			localShield = 700;
		}
		if(photonView.isMine) {
			shield.value = localShield;
		}
	}
	
	void Start()
	{
		// Set up an array so our script can more easily set up the hit boxes
		collidersAttack = new PolygonCollider2D[]{groundAttack, midAirAttack1, midAirAttack2, downAttack};
		
		// Create a polygon collider
		localCollider = gameObject.AddComponent<PolygonCollider2D>();
		localCollider.isTrigger = true; // Set as a trigger so it doesn't collide with our environment
		localCollider.pathCount = 0; // Clear auto-generated polygons

		anim = GetComponent<Animator>();
		localShield = 700;

	}

	void OnGUI() {
		if(Application.loadedLevelName != "insideShip" && canvas.activeSelf) {
			float guiModifier = (float)Screen.height / 600f;
			screenPosition = Camera.main.WorldToScreenPoint(transform.position);
			screenPosition.y = Screen.height - screenPosition.y;
			health = Resources.Load ("overhead-gauge-bar4") as Texture;
			//GUI.DrawTexture(new Rect(screenPosition.x - (50 * guiModifier) - (100 - (localShield / 7)), screenPosition.y - (85 * guiModifier), (100) * guiModifier, 10 * guiModifier), health);
			GUI.DrawTexture(new Rect(screenPosition.x - (46.9f * guiModifier), screenPosition.y - (82.3f * guiModifier), (localShield / 7.466f) * guiModifier, 5.5f * guiModifier), health as Texture);
			healthBackground = Resources.Load ("overhead-gauge-bar1") as Texture;
			GUI.DrawTexture(new Rect(screenPosition.x - (50 * guiModifier), screenPosition.y - (85 * guiModifier), (100) * guiModifier, 11 * guiModifier), healthBackground as Texture);
		}
	}

	void Update() {

		if(Application.loadedLevelName != "insideShip") {

			if(isRespawning) {

				KeepPlayersGhosts();
				
				localShield = 700;
				InvokeRepeating("rechargeShield", 0.000001f, 0.1f);
				
				if(photonView.isMine) {
					
					foreach (Text text in texts) {
						if(text.name == "HealthCurrent") {
							text.text = "200";
						}
					}
					foreach (Image image in images) {
						if(image.name == "HealthBar0" || image.name == "HealthBar1") {
							image.sprite = healthBarFull;
						}
					}
					
				}
				
				isRespawning = false;
				
			}

			if(photonView.isMine) {

				if(Input.GetKey("h")) {

					canvas.SetActive(false);
					
				}
				if(Input.GetKey("j")) {

					canvas.SetActive(true);
					
				}

				if(GetComponent<HealthBar>().healthBarArray[1].GetComponent<Animator>().GetBool("isEmpty")) {
					
					foreach (Image image in images) {
						if(image.name == "HealthBar1") {
							image.sprite = healthBarEmpty;
						}
					}
					foreach (Text text in texts) {
						if(text.name == "HealthCurrent") {
							text.text = "100";
						}
					}
					
				}

			}

		}
		
	}

	void OnTriggerEnter2D(Collider2D col) {

		if(col.name == "glassBreakPrefab") {

			glass = col.gameObject;

			if(anim.GetBool ("midAirAttack") || anim.GetBool ("downAttack") || anim.GetBool("groundAttack")) {

				glass.GetComponent<GlassBreak>().GlassBreakMethod();
				
			}

		}
			
		if(col.tag == "Player" && anim.isActiveAndEnabled) {

			if(col.GetComponent<Rigidbody2D>() != null && col.GetComponent<HealthBar>() != null) {
				colliderGameObject = col.gameObject;
				colliderPosition = col.GetComponent<Rigidbody2D> ().position;
				healthBar0 = col.GetComponent<HealthBar>().healthBarArray[0];
				healthBar1 = col.GetComponent<HealthBar>().healthBarArray[1];
				healthAnim = healthBar1.GetComponent<Animator>();
				colliderShield = col.GetComponent<HitBoxManager>().localShield;
				colliderAnimator = col.GetComponent<Animator>();
			}

			if(anim.GetBool ("groundAttack") || anim.GetBool ("midAirAttack")) {

				if(colliderAnimator.GetBool("block")) {

					if(colliderGameObject.GetComponent<PlayerControl>().canParry) {

						Vector2 playerPos = GetComponent<Rigidbody2D>().position;

						if(GetComponent<PlayerControl>().facingRight) {

							GetComponent<Rigidbody2D>().position = playerPos + new Vector2(-8, 0);

						} else {

							GetComponent<Rigidbody2D>().position = playerPos + new Vector2(8, 0);

						}

						GetComponent<PlayerControl>().disablePlayer();

					}

				}

				if((colliderAnimator.GetBool("block") && GetComponent<PlayerControl>().facingRight == col.GetComponent<PlayerControl>().facingRight) 
				   || colliderAnimator.GetBool("block") == false) {

					if(col.GetComponent<PhotonView>().isMine) {

						GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraSmoothFollow>().shake = 0.5f;

					}
				
					if(anim.GetBool ("groundAttack")) {
						if(GetComponent<PlayerControl>().facingRight) {
							col.GetComponent<Rigidbody2D> ().position = colliderPosition + new Vector2(7, 0);
						}
						else {
							col.GetComponent<Rigidbody2D> ().position = colliderPosition - new Vector2(7, 0);
						}
					}
					
					if(col.GetComponent<HitBoxManager>().localShield < 700) {

						if(healthAnim.GetBool ("isEmpty") == false && photonView.isMine) {
							
							//healthAnim.SetBool ("isEmpty", true);
							colliderGameObject.GetComponent<HealthBar>().fireLowerHealthRPC(1);
							
						}
						else if(healthAnim.GetBool("isEmpty") && photonView.isMine) {
							
							colliderGameObject.GetComponent<HitBoxManager>().CancelInvoke();
							colliderGameObject.GetComponent<HitBoxManager>().DeadUI();
							
							healthBar0.SetActive(false);
							healthBar1.SetActive(false);
							
							SpriteSlicer2D.SliceSprite(colliderPosition + new Vector2(-1, 2), colliderPosition + new Vector2(1, -2), colliderGameObject, false, ref SlicedSpriteInfo);
							
							for (int spriteIndex = 0; spriteIndex < SlicedSpriteInfo.Count; spriteIndex++)
							{
								for (int childSprite = 0; childSprite < SlicedSpriteInfo[spriteIndex].ChildObjects.Count; childSprite++)
								{            
									SpriteSlicer2D.SliceSprite(colliderPosition + new Vector2(-1, -2), colliderPosition + new Vector2(1, 2), SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite], false, ref SlicedSpriteInfo);
									SpriteSlicer2D.SliceSprite(colliderPosition + new Vector2(-1, 2), colliderPosition + new Vector2(1, 2), SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite], false, ref SlicedSpriteInfo);
									SpriteSlicer2D.SliceSprite(colliderPosition + new Vector2(-1, -3), colliderPosition + new Vector2(1, -1), SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite], false, ref SlicedSpriteInfo);
									SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite].layer = 11;
									Physics2D.IgnoreLayerCollision(10, 11);
									SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite].GetComponent<Rigidbody2D>().AddForce(new Vector2(600, 0));
									SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite].AddComponent<FadeAndDestroy>();
									
								}
							}
							
							SlicedSpriteInfo.Clear();

							colliderGameObject.GetComponentInParent<Respawn>().FireDeadRPC("groundAttack");
							
						}

					}
					
					if(colliderShield == 700) {
						col.GetComponent<HitBoxManager>().localShield = 0;
					}
					
				}

			}

			else if(anim.GetBool("downAttack")) {

				colliderGameObject.GetComponent<HitBoxManager>().CancelInvoke();
				colliderGameObject.GetComponent<HitBoxManager>().DeadUI();
				
				healthBar0.SetActive(false);
				healthBar1.SetActive(false);
				
				SpriteSlicer2D.SliceSprite(colliderPosition + new Vector2(0, 4), colliderPosition + new Vector2(0, -4), colliderGameObject, false, ref SlicedSpriteInfo);
				
				for (int spriteIndex = 0; spriteIndex < SlicedSpriteInfo.Count; spriteIndex++)
				{
					for (int childSprite = 0; childSprite < SlicedSpriteInfo[spriteIndex].ChildObjects.Count; childSprite++)
					{            
						
						SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite].layer = 11;
						Physics2D.IgnoreLayerCollision(10, 11);
						SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite].AddComponent<FadeAndDestroy>();
						
					}
				}
				
				if(SlicedSpriteInfo[0].ChildObjects[0].GetComponent<Rigidbody2D>().transform.position.x < SlicedSpriteInfo[0].ChildObjects[1].GetComponent<Rigidbody2D>().transform.position.x) {
					SlicedSpriteInfo[0].ChildObjects[0].GetComponent<Rigidbody2D>().AddForce(new Vector2(-200, 0));
					SlicedSpriteInfo[0].ChildObjects[1].GetComponent<Rigidbody2D>().AddForce(new Vector2(200, 0));
				} else {
					SlicedSpriteInfo[0].ChildObjects[0].GetComponent<Rigidbody2D>().AddForce(new Vector2(200, 0));
					SlicedSpriteInfo[0].ChildObjects[1].GetComponent<Rigidbody2D>().AddForce(new Vector2(-200, 0));
				}
				
				SlicedSpriteInfo.Clear();

				colliderGameObject.GetComponentInParent<Respawn>().FireDeadRPC("downAttack");
				
			}

		}
		
	}

	public void ApplyUnrecievedDeath(string causeOfDeath) {

		if(causeOfDeath == "downAttack") {

			GetComponent<HitBoxManager>().CancelInvoke();
			GetComponent<HitBoxManager>().DeadUI();
			
			GetComponent<HealthBar>().healthBarArray[0].SetActive(false);
			GetComponent<HealthBar>().healthBarArray[1].SetActive(false);
			
			SpriteSlicer2D.SliceSprite(gameObject.GetComponent<Rigidbody2D>().position + new Vector2(0, 4), gameObject.GetComponent<Rigidbody2D>().position + new Vector2(0, -4), gameObject, false, ref SlicedSpriteInfo);
			
			for (int spriteIndex = 0; spriteIndex < SlicedSpriteInfo.Count; spriteIndex++)
			{
				for (int childSprite = 0; childSprite < SlicedSpriteInfo[spriteIndex].ChildObjects.Count; childSprite++)
				{            
					
					SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite].layer = 11;
					Physics2D.IgnoreLayerCollision(10, 11);
					SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite].AddComponent<FadeAndDestroy>();
					
				}
			}
			
			if(SlicedSpriteInfo[0].ChildObjects[0].GetComponent<Rigidbody2D>().transform.position.x < SlicedSpriteInfo[0].ChildObjects[1].GetComponent<Rigidbody2D>().transform.position.x) {
				SlicedSpriteInfo[0].ChildObjects[0].GetComponent<Rigidbody2D>().AddForce(new Vector2(-200, 0));
				SlicedSpriteInfo[0].ChildObjects[1].GetComponent<Rigidbody2D>().AddForce(new Vector2(200, 0));
			} else {
				SlicedSpriteInfo[0].ChildObjects[0].GetComponent<Rigidbody2D>().AddForce(new Vector2(200, 0));
				SlicedSpriteInfo[0].ChildObjects[1].GetComponent<Rigidbody2D>().AddForce(new Vector2(-200, 0));
			}
			
			SlicedSpriteInfo.Clear();

		} else if(causeOfDeath == "groundAttack") {
			
			GetComponent<HitBoxManager>().CancelInvoke();
			GetComponent<HitBoxManager>().DeadUI();
			
			GetComponent<HealthBar>().healthBarArray[0].SetActive(false);
			GetComponent<HealthBar>().healthBarArray[1].SetActive(false);

			Vector2 pos = gameObject.GetComponent<Rigidbody2D>().position;
			
			SpriteSlicer2D.SliceSprite(pos + new Vector2(-1, 2), pos + new Vector2(1, -2), gameObject, false, ref SlicedSpriteInfo);
			
			for (int spriteIndex = 0; spriteIndex < SlicedSpriteInfo.Count; spriteIndex++)
			{
				for (int childSprite = 0; childSprite < SlicedSpriteInfo[spriteIndex].ChildObjects.Count; childSprite++)
				{            
					SpriteSlicer2D.SliceSprite(pos + new Vector2(-1, -2), pos + new Vector2(1, 2), SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite], false, ref SlicedSpriteInfo);
					SpriteSlicer2D.SliceSprite(pos + new Vector2(-1, 2), pos + new Vector2(1, 2), SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite], false, ref SlicedSpriteInfo);
					SpriteSlicer2D.SliceSprite(pos + new Vector2(-1, -3), pos + new Vector2(1, -1), SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite], false, ref SlicedSpriteInfo);
					SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite].layer = 11;
					Physics2D.IgnoreLayerCollision(10, 11);
					SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite].GetComponent<Rigidbody2D>().AddForce(new Vector2(600, 0));
					SlicedSpriteInfo[spriteIndex].ChildObjects[childSprite].AddComponent<FadeAndDestroy>();
					
				}
			}
			
			SlicedSpriteInfo.Clear();
			
		}
		
	}

	public void KeepPlayersGhosts() {
		
		players = GameObject.FindGameObjectsWithTag("Player");

		for(int i = 0; i < players.Length; i++) {

			if(players[i].layer != 11) {
				Physics2D.IgnoreCollision(players[i].GetComponent<PolygonCollider2D>(), GetComponent<PolygonCollider2D>());
			}
			
		}
		
	}
	
	public void DeadUI() {

		photonView.RPC("CancelTheInvoke", PhotonTargets.All);

		if(photonView.isMine) {

			shield.value = 0;
			foreach (Text text in texts) {
				if(text.name == "HealthCurrent") {
					text.text = "0";
				}
			}
			foreach (Image image in images) {
				if(image.name == "HealthBar0" || image.name == "HealthBar1") {
					image.sprite = healthBarEmpty;
				}
			}

		}

	}

	[RPC]
	public void CancelTheInvoke() {
		CancelInvoke();
	}
	
	public void setHitBox(hitBoxes val)
	{
		if(val != hitBoxes.clear)
		{
			localCollider.SetPath(0, collidersAttack[(int)val].GetPath(0));
			return;
		}
		localCollider.pathCount = 0;
	}

}
