using UnityEngine;
using System.Collections;

public class CanvasFollow : MonoBehaviour {
	public int maxHealth = 700;
	public int curHealth = 700;
	
	public float healthBarLength;

	private float pos;
	
	// Use this for initialization
	void Start () {
		healthBarLength = Screen.width / 6;
	}
	
	void Update ()
	{
		AddjustCurrentHealth(0);
	}
	
	void OnGUI() {
		GUI.Box(new Rect(transform.position.x + 400, transform.position.y + 100, healthBarLength, 20), curHealth + "/" + maxHealth);
	}
	
	public void AddjustCurrentHealth(int adj) {
		curHealth += adj;
		
		if (curHealth < 0)
			curHealth = 0;
		
		if (curHealth > maxHealth)
			curHealth = maxHealth;
		
		if(maxHealth < 1)
			maxHealth = 1;
		
		healthBarLength = (Screen.width / 6) * (curHealth /(float)maxHealth);
	}
}