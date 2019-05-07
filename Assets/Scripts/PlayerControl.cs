using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerControl : Photon.MonoBehaviour {

	[HideInInspector] public bool canDash;
	[HideInInspector] public bool canDodge;
	[HideInInspector] public bool canParry;
	[HideInInspector] public bool hasKnife;
	[HideInInspector] public bool hasSmokeBomb;
	[HideInInspector] public bool facingRight;	

	public float jumpForce = 8500f;				// Amount of force added when the player jumps.
	public float maxSpeed = 500f;				// The fastest the player can travel in the x axis.
	
	private float moveForce = 365f;				// Amount of force added to move the player left and right.

	private bool isLeftAndRightEnabled;			// Condition for when to disable the left & right arrow keys. Only for wall jumps right now.
	private bool isDisabled;
	private bool grounded;						// Whether or not the player is grounded.
	private bool stop;

	private int midAirAttackCounter;
	private int jumpCounter;
	private int tauntIndex;						// The index of the taunts array indicating the most recent taunt.

	private float dodgeForce;
	private float h;
	
	private Transform groundCheck;				// A position marking where to check if the player is grounded.
	
	private Animator anim;						// Reference to the player's animator component.

	private GameObject thrownObject;

	private Image[] images;

	private Text[] texts;

	/*private AudioClip[] taunts;				// Array of clips for when the player taunts.
	private float tauntProbability = 50f;	// Chance of a taunt happening.
	private float tauntDelay = 1f;			// Delay for when the taunt should happen.*/

	void Awake() {

		jumpCounter = 0;
		midAirAttackCounter = 2;
		isLeftAndRightEnabled = true;
		canDash = true;
		canDodge = true;
		canParry = false;
		facingRight = true;
		grounded = false;
		hasKnife = false; // false
		hasSmokeBomb = false; //false

		// Setting up references.
		groundCheck = transform.Find("groundCheck");
		anim = GetComponent<Animator>();
		DontDestroyOnLoad(gameObject);

		// How much data we are sending
		PhotonNetwork.sendRate = 100;
		PhotonNetwork.sendRateOnSerialize = 90;
		
	}
	
	void Update() {
		
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  

		if(photonView.isMine) {

			if(isDisabled == false) {

				if(Application.loadedLevelName != "insideShip") {

					InputManager();

				}

			}

		} else {

			/*float pingInSeconds = (float)PhotonNetwork.GetPing() * 0.001f;
			float timeSinceLastUpdate = (float)(PhotonNetwork.time - time);
			float totalTimePassed = pingInSeconds + timeSinceLastUpdate;*/

			transform.position = Vector3.Lerp(transform.position, realPos, Time.deltaTime * 50f);
			if(facingRight != isFacingRight) {
				Flip();
			}
			if(GetComponent<HealthBar>().healthBarArray[1].activeSelf) {
				GetComponent<HealthBar>().healthBarArray[1].GetComponent<Animator>().SetBool ("isEmpty", healthBar);
			}
			GetComponent<PlayerInfo>().playerNumber = playerNumber;

		}

	}
	
	void InputManager() {

		if(grounded) {

			// If the ground attack button is pressed then the player should do a ground attack.
			if(Input.GetButtonDown("GroundAttack")) {
				photonView.RPC ("GroundAttack", PhotonTargets.All);
			}
			
			// This code allows for changing directions while holding down block
			else if(Input.GetButton("Block")) {
				photonView.RPC ("Block", PhotonTargets.All);
				
				if(Input.GetKeyDown ("left") && facingRight)
					photonView.RPC ("Flip", PhotonTargets.All);
				else if(Input.GetKeyDown ("right") && !facingRight)
					photonView.RPC ("Flip", PhotonTargets.All);
				
			} else if (anim.GetBool("block") && !Input.GetButton("Block")) {
				photonView.RPC ("BlockFalse", PhotonTargets.All);
			}

			// If the player presses left or right while blocking then player will execute a dodge
			if(Input.GetKeyDown ("left") && anim.GetBool ("block") && canDodge) {
				dodgeForce = -25000;
				if(!facingRight) {
					photonView.RPC ("Flip", PhotonTargets.All);
				}
				photonView.RPC ("DodgeBackward", PhotonTargets.All);
			}
			else if(Input.GetKeyDown ("right") && anim.GetBool ("block") && canDodge) {
				dodgeForce = 25000;
				if(facingRight) {
					photonView.RPC ("Flip", PhotonTargets.All);
				}
				photonView.RPC ("DodgeBackward", PhotonTargets.All);
			}

			// If the player is not pressing a button to move, stop them completely
			if (Input.GetButton ("Horizontal") == false) {
				stopMovement();
			}

		} else {

			// If the air attack button is pressed then the player should do an air attack
			if(Input.GetButtonDown ("AirSlash") && !grounded && midAirAttackCounter > 0 && anim.GetBool ("downAttack") == false && Input.GetButtonDown("Dash") == false) {
				photonView.RPC ("AirSlash", PhotonTargets.All);
			}
			// If the down attack button is pressed then the player should do a down attack
			else if(Input.GetButtonDown("DownAttack") && !grounded && anim.GetBool("midAirAttack") == false && anim.GetBool ("dash") == false) {
				photonView.RPC ("DownAttack", PhotonTargets.All);
			}

		}

		if(Input.GetKeyDown(KeyCode.Escape)) {

			Application.Quit();

		}

		
		if(Input.GetButtonDown("ActivateItem")) {	

			if(hasKnife) {

				if(facingRight) {
					thrownObject = PhotonNetwork.Instantiate("Knife", transform.position + new Vector3(2, 3, 0), Quaternion.identity, 0);
					thrownObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(3000, 50));
				} else {
					thrownObject = PhotonNetwork.Instantiate("Knife", transform.position - new Vector3(2, -3, 0), Quaternion.identity, 0);
					thrownObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(-3000, 50));
				}

				hasKnife = false; //false

				foreach (Image image in images) {
					if(image.name == "Equip") {
						image.enabled = false;
					}
				}

				foreach (Text text in texts) {
					if(text.name == "EquipCurrent") {
						text.text = "0";
					}
				}

			}

			else if (hasSmokeBomb) {

				if(facingRight) {
					thrownObject = PhotonNetwork.Instantiate("SmokeBomb", transform.position + new Vector3(2, 3, 0), Quaternion.identity, 0);
					thrownObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(3000, 50));
				} else {
					thrownObject = PhotonNetwork.Instantiate("SmokeBomb", transform.position - new Vector3(2, -3, 0), Quaternion.identity, 0);
					thrownObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(-3000, 50));
				}

				hasSmokeBomb = false;

				foreach (Image image in images) {
					if(image.name == "Equip") {
						image.enabled = false;
					}
				}
				
				foreach (Text text in texts) {
					if(text.name == "EquipCurrent") {
						text.text = "0";
					}
				}

			}

		}

		if(anim.GetBool ("block") == false && anim.GetBool("dash") == false && anim.GetBool ("midAirAttack") == false) {
			h = Input.GetAxis("Horizontal");
		}
		else if(anim.GetBool("block")) {
			h = 0;
			if(anim.GetBool("falling")) {
				photonView.RPC("BlockFalse", PhotonTargets.All);
			}
		}
		
		// If the jump button is pressed then the player should jump.
		// NOTE: the counter resets when grounded so the first jump will not be counted, code adjusted for it.
		if(Input.GetButtonDown("Jump") && anim.GetBool ("groundAttack") == false && anim.GetBool("downAttack") == false) {
			if(jumpCounter < 2) { //Max of 2 jumps
				if(jumpCounter == 1) {
					GetComponent<Rigidbody2D>().velocity = new Vector3(GetComponent<Rigidbody2D>().velocity.x, 0);
				}
				jumpCounter++;
				photonView.RPC("jumpAnim", PhotonTargets.All);
			}
		}

		// If the player presses the dash button then a dash will be executed
		if(Input.GetButtonDown("Dash") && canDash && anim.GetBool ("midAirAttack") == false && anim.GetBool ("groundAttack") == false && anim.GetBool ("downAttack") == false) {
			photonView.RPC ("Dash", PhotonTargets.All);
		}

		if(anim.GetBool("wallSlide")) {

			// If the jump button is pressed while on the wall then do a wall jump.
			if(Input.GetButtonDown("Jump") && anim.GetBool ("wallSlide")) {
				photonView.RPC ("WallJump", PhotonTargets.All);
			}

			// If the player stops pressing left or right then they will no longer wall slide
			if(Input.GetButton("Horizontal") == false || GetComponent<Rigidbody2D>().velocity.x > 0.1 || GetComponent<Rigidbody2D>().velocity.x < -0.1) {
				photonView.RPC ("wallSlideFalse", PhotonTargets.All);
			}
		}

	}
	
	void FixedUpdate () {
	
		// Cache the horizontal input.
		if(grounded) {
			
			// If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
			if(h * GetComponent<Rigidbody2D>().velocity.x < maxSpeed) {
				// ... add a force to the player.
				GetComponent<Rigidbody2D>().AddForce(Vector2.right * h * moveForce * 3);
			}
			
			// If the player's horizontal velocity is greater than the maxSpeed...
			if(Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) > maxSpeed) {
				// ... set the player's velocity to the maxSpeed in the x axis.
				GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sign(GetComponent<Rigidbody2D>().velocity.x) * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);
			}

			anim.SetBool ("midAir", false);
			anim.SetBool ("jump", false);
			anim.SetBool ("wallSlide", false);
			anim.SetBool ("falling", false);
			if(GetComponent<Rigidbody2D>().velocity.y > -99 && anim.GetBool("downAttack")) {
				anim.SetBool ("downAttack", false);
			}
			midAirAttackCounter = 2;
			if(isDisabled == false) {
				moveForce = 365f;
			}
			if(Input.GetKey ("up")) {
				jumpCounter = 1;
			} else {
				jumpCounter = 0;
			}
			// Stops all movement and lets the player do a ground attack.
			if(anim.GetBool ("groundAttack")){
				stopMovement();
			}
			// If the players velocity is not 0, they are running
			if (GetComponent<Rigidbody2D>().velocity.x < -0.1 || GetComponent<Rigidbody2D>().velocity.x > 0.1) {
				anim.SetBool ("run", true);
			}
			else {
				anim.SetBool ("run", false);
			}

			if(anim.GetBool ("dodgeBackward")) {
				// Let's the player move through another player
				//StartCoroutine ("tempCharGhosting");
				GetComponent<Rigidbody2D>().AddForce (new Vector2 (dodgeForce, 0));
			}
			
		} else {

			if(GetComponent<Rigidbody2D>().velocity.x > 0.1 || GetComponent<Rigidbody2D>().velocity.x < -0.1 ) {

				//photonView.RPC ("wallSlideFalse", PhotonTargets.All);
				if(anim.GetBool("downAttack")) {
					photonView.RPC ("downAttackFalse", PhotonTargets.All);
				}

			}

			if(anim.GetBool("block")) {

				photonView.RPC ("BlockFalse", PhotonTargets.All);

			}
			// If the player attains positive y velocity, start jump animation
			if(GetComponent<Rigidbody2D>().velocity.y > 0) {
				anim.SetBool ("jump", true);
				anim.SetBool ("midAir", false);
				anim.SetBool ("run", false);
			}
			
			// If the player loses y velocity, start the mid-air and falling animation.
			else if(GetComponent<Rigidbody2D>().velocity.y <= 0) {
				
				if(anim.GetBool ("jump")) {
					anim.SetBool ("midAir", true);
				}
				anim.SetBool ("falling", true);
				anim.SetBool ("jump", false);
				anim.SetBool ("run", false);
			}
			
			if(h * GetComponent<Rigidbody2D>().velocity.x < maxSpeed ) {
				// ... add a force to the player.
				GetComponent<Rigidbody2D>().AddForce(Vector2.right * h * moveForce);
			}
			
			// If the player's horizontal velocity is greater than the maxSpeed...
			if(Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) > maxSpeed ) {
				// ... set the player's velocity to the maxSpeed in the x axis.
				GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sign(GetComponent<Rigidbody2D>().velocity.x) * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);
			}

			// If the player does a down attack, they can not move anywhere until they hit something
			if(anim.GetBool ("downAttack")) {
				stopMovement ();
				moveForce = 0f;	
			}

			// If a mid air attack is being executed, push the player with 5k force left or right
			else if(anim.GetBool ("midAirAttack")) {
				GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0);
				if(facingRight) {
					GetComponent<Rigidbody2D>().AddForce (new Vector2 (10000, 0));
				}
				else {
					GetComponent<Rigidbody2D>().AddForce (new Vector2 (-10000, 0));
				}
			}

		}

		// If dash was pressed, add 25000 force either left or right
		if(anim.GetBool ("dash")) {
			GetComponent<Rigidbody2D>().velocity = new Vector3(GetComponent<Rigidbody2D>().velocity.x, 0);
			if(facingRight)
				GetComponent<Rigidbody2D>().AddForce (new Vector2 (25000, 0));
			else
				GetComponent<Rigidbody2D>().AddForce (new Vector2 (-25000, 0));
		}

		// If the input is moving the player right and the player is facing left...
		if (h > 0.1 && !facingRight && anim.GetBool ("groundAttack") == false) {
			// ... flip the player.
			photonView.RPC ("Flip", PhotonTargets.All);

		}
		// Otherwise if the input is moving the player left and the player is facing right...
		else if(h < -0.1 && facingRight && anim.GetBool ("groundAttack") == false) {
			// ... flip the player.
			photonView.RPC ("Flip", PhotonTargets.All);

		}
		
	}
	// This is the code that allows a player to move through another player
	/*IEnumerator tempCharGhosting() {
		Physics2D.IgnoreLayerCollision (8, 8, true);
		yield return new WaitForSeconds(0.18f);
		Physics2D.IgnoreLayerCollision (8, 8, false);
	}*/
	// This RPC Block method stops all motion and removes all possiblity of moving
	[RPC] void Block() {
		anim.SetBool ("block", true);
		StartCoroutine("Parry");
		stopMovement();
	}
	
	[RPC] void BlockFalse() {
		anim.SetBool ("block", false);
		StopCoroutine("Parry");
		canParry = false;
	}

	[RPC] void wallSlideFalse() {

		anim.SetBool ("wallSlide", false);

	}

	[RPC] void downAttackFalse() {
		
		anim.SetBool ("downAttack", false);
		
	}

	//This RPC is used to completely halt a player
	void stopMovement() {
		GetComponent<Rigidbody2D>().velocity = new Vector3(0, GetComponent<Rigidbody2D>().velocity.y);
		GetComponent<Rigidbody2D>().angularVelocity = 0;
		moveForce = 0f;
		h = 0;
	}
	
	[RPC] void jumpAnim() {
		
		// Play a random jump audio clip.
		//int i = Random.Range(0, jumpClips.Length);
		//AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);
		
		// Add a vertical force to the player.
		moveForce = 365f;
		GetComponent<Rigidbody2D>().AddForce (new Vector2 (0, jumpForce));
		
	}
	
	 [RPC] void DownAttack() {
		
		anim.SetBool ("downAttack", true);
		stopMovement ();
		GetComponent<Rigidbody2D>().velocity = new Vector3(0, -80);
		
	}
	
	 [RPC] void Dash() {
		
		StartCoroutine("dash");
		StartCoroutine("dashTimer");
		
	}

	IEnumerator Parry() {
		canParry = true;
		yield return new WaitForSeconds(2f);
		canParry = false;
	}
	
	IEnumerator dash() {
		anim.SetBool ("dash", true);
		yield return new WaitForSeconds(0.18f);
		anim.SetBool ("dash", false);
	}
	
	IEnumerator dashTimer() {
		canDash = false;
		yield return new WaitForSeconds(5f);
		canDash = true;
	}
	
	 [RPC] void DodgeBackward() {
		
		StartCoroutine("dodgeBackward");
		StartCoroutine("dodgeTimer");
		
	}
	
	IEnumerator dodgeBackward() {
		anim.SetBool ("dodgeBackward", true);
		yield return new WaitForSeconds(0.18f);
		anim.SetBool ("dodgeBackward", false);
	}
	
	IEnumerator dodgeTimer() {
		canDodge = false;
		yield return new WaitForSeconds(2f);
		canDodge = true;
	}
	
	 [RPC] void GroundAttack() {
		
		StartCoroutine("groundAttack");
		
	}
	
	IEnumerator groundAttack() {
		
		anim.SetBool ("groundAttack", true);
		// Ground attack will last .26 seconds
		yield return new WaitForSeconds(0.26f);
		anim.SetBool ("groundAttack", false);
		
	}
	
	 [RPC] void AirSlash() {
		
		StartCoroutine("airSlash");
		
	}
	
	IEnumerator airSlash() {
		
		anim.SetBool ("midAirAttack", true);
		midAirAttackCounter--;
		yield return new WaitForSeconds(0.5f);
		anim.SetBool ("midAirAttack", false);
		
	}
	
	 [RPC] void WallJump() {
		
		StartCoroutine ("wallJump");
		
	}
	
	IEnumerator wallJump() {
		
		anim.SetBool ("wallSlide", false);
		anim.SetBool ("jump", true);
		// Disable the player's ability to hold onto the wall temporarily
		isLeftAndRightEnabled = false;
		// Return wall hold after .2 seconds
		yield return new WaitForSeconds(0.20f);
		// Return the wall hold ability
		isLeftAndRightEnabled = true;
		anim.SetBool ("jump", false);
		
	}
	
	 [RPC] void WallSlide() {
		
		StartCoroutine ("wallSlide");
		
	}
	
	IEnumerator wallSlide() {

		midAirAttackCounter = 2;
		
		//Must be -1 so you can jump twice off of a wall
		jumpCounter = 0;
		anim.SetBool ("wallSlide", true);
		anim.SetBool ("jump", false);
		anim.SetBool ("midAir", false);
		
		// Stop the players motion completely so they are grabbing the wall.
		GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		Physics2D.gravity = new Vector3(0, 0, 0);
		// Player can hold on for max of 1.5 seconds, then falls. Can also be interrupted by letting go.
		float i = 0f;
		stop = false;
		do
		{
			if(anim.GetBool ("wallSlide") == false) {
				photonView.RPC ("stopped", PhotonTargets.All);
			}

			yield return new WaitForSeconds(.01f);
			i += .01f;
			
		} while(!stop && i < 1.5f);
		// Return gravity back to normal.
		Physics2D.gravity = new Vector3(0, -80, 0);
		
	}
	
	 [RPC] void stopped() {
		
		stop = true;
		
	}

	void OnCollisionEnter2D (Collision2D col) {

		Collider2D collider = col.collider;

		if(collider.name == "FallKillZone" && photonView.isMine) {
			Physics2D.IgnoreCollision (gameObject.GetComponent<PolygonCollider2D>(), collider);
			gameObject.GetComponent<HealthBar>().healthBarArray[0].SetActive(false);
			gameObject.GetComponent<HealthBar>().healthBarArray[1].SetActive(false);
			gameObject.GetComponent<HitBoxManager>().DeadUI();
			gameObject.SetActive(false);
		}

		if(collider.name == "glassBreakPrefab") {

			if(anim.GetBool ("dash")) {

				collider.GetComponent<Animator>().SetBool("isBroken", true);
				collider.GetComponent<Collider2D>().enabled = false;

			}

		}

	}
	
	void OnCollisionStay2D (Collision2D col) {

		Collider2D collider = col.collider;

		if(photonView.isMine && !grounded) {

			// If the player comes into contact with a tower...
			if(collider.tag == "desertCityBuilding" || collider.tag == "Glass") { 

				if(Input.GetKey("right") && isLeftAndRightEnabled && facingRight && anim.GetBool ("wallSlide") == false && GetComponent<Rigidbody2D>().velocity.x == 0) {
					photonView.RPC ("WallSlide", PhotonTargets.All);
				}

				else if(Input.GetKey("left") && isLeftAndRightEnabled && !facingRight && anim.GetBool ("wallSlide") == false && GetComponent<Rigidbody2D>().velocity.x == 0) {
					photonView.RPC ("WallSlide", PhotonTargets.All);
				}

			}
			else {
				photonView.RPC ("wallSlideFalse", PhotonTargets.All);
			}
			
		}

	}

	void OnCollisionExit2D (Collision2D col) {

		Collider2D collider = col.collider;

		if(photonView.isMine && !grounded) {

			if(collider.name == "env_TowerFull" || collider.tag == "desertCityBuilding")  {
				photonView.RPC ("wallSlideFalse", PhotonTargets.All);
			}

		}

	}
	
	 [RPC] void Flip () {
		
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
		
	}

	public void disablePlayer() {

		StartCoroutine("disable");

	}

	IEnumerator disable() {

		isDisabled = true;

		yield return new WaitForSeconds(1);

		isDisabled = false;

	}

	private Vector3 realPos = Vector2.zero;
	private bool isFacingRight;
	private bool healthBar;
	private float playerNumber;

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		realPos = Vector3.zero;

		if (stream.isWriting) {

			stream.SendNext(transform.position);
			stream.SendNext(GetComponent<Rigidbody2D>().velocity);
			stream.SendNext(facingRight);
			if(GetComponent<HealthBar>().healthBarArray.Count == 2) {
				stream.SendNext(GetComponent<HealthBar>().healthBarArray[1].GetComponent<Animator>().GetBool("isEmpty"));
			}
			if(Application.loadedLevelName == "insideShip") {
				stream.SendNext(GetComponent<PlayerInfo>().playerNumber);
			}

		}
		else {

			realPos = (Vector3)stream.ReceiveNext();
			GetComponent<Rigidbody2D>().velocity = (Vector2)stream.ReceiveNext();
			isFacingRight = (bool)stream.ReceiveNext();
			if(GetComponent<HealthBar>().healthBarArray.Count == 2) {
				healthBar = (bool)stream.ReceiveNext();
			}
			if(Application.loadedLevelName == "insideShip") {
				playerNumber = (float)stream.ReceiveNext();
			}

		}
	}

	void OnLevelWasLoaded() {

		images = FindObjectOfType<Canvas>().GetComponentsInChildren<Image>();
		texts = FindObjectOfType<Canvas>().GetComponentsInChildren<Text>();

		for(int i = 0; i < GameObject.FindGameObjectsWithTag("Player").Length; i++) {
			if(GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<Rigidbody2D>().position == new Vector2 (156, 24)) {
				GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<Rigidbody2D>().position = new Vector2 (160, 24);
			} else {
				GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<Rigidbody2D>().position = new Vector2 (156, 24);
			}
		}

	}

	/*public IEnumerator Taunt() {
		// Check the random chance of taunting.
		float tauntChance = Random.Range(0f, 100f);
		if(tauntChance > tauntProbability) {
			// Wait for tauntDelay number of seconds.
			yield return new WaitForSeconds(tauntDelay);
			
			// If there is no clip currently playing.
			if(!GetComponent<AudioSource>().isPlaying) {
				// Choose a random, but different taunt.
				tauntIndex = TauntRandom();
				
				// Play the new taunt.
				GetComponent<AudioSource>().clip = taunts[tauntIndex];
				GetComponent<AudioSource>().Play();
			}
		}
	}
	
	
	int TauntRandom() {
		// Choose a random index of the taunts array.
		int i = Random.Range(0, taunts.Length);
		
		// If it's the same as the previous taunt...
		if(i == tauntIndex)
			// ... try another random taunt.
			return TauntRandom();
		else
			// Otherwise return this index.
			return i;
	}*/
	
}

