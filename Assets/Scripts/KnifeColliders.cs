using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class KnifeColliders : Photon.MonoBehaviour {

	public PolygonCollider2D knife1;
	public PolygonCollider2D knife2;
	public PolygonCollider2D knife3;
	public PolygonCollider2D knife4;
	public PolygonCollider2D knife5;
	public PolygonCollider2D knife6;
	public PolygonCollider2D knife7;
	public PolygonCollider2D knife8;

	// Collider on this game object
	private PolygonCollider2D knifeCollider;
	
	// Used for organization
	private PolygonCollider2D[] knifeColliders;

	public enum hitBoxes {
		knife1,
		knife2,
		knife3,
		knife4,
		knife5,
		knife6,
		knife7,
		knife8,
	}

	void Start()
	{
		// Set up an array so our script can more easily set up the hit boxes
		knifeColliders = new PolygonCollider2D[]{knife1, knife2, knife3, knife4, knife5, knife6, knife7, knife8};
		
		// Create a polygon collider
		knifeCollider = gameObject.AddComponent<PolygonCollider2D>();
		//knifeCollider.isTrigger = true; // Set as a trigger so it doesn't collide with our environment
		knifeCollider.pathCount = 0; // Clear auto-generated polygons
		
	}
	
	public void setKnifeHitBox(hitBoxes val)
	{
		knifeCollider.SetPath(0, knifeColliders[(int)val].GetPath(0));
		return;
	}
}
