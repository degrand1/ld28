using UnityEngine;
using System.Collections;

public enum HazardClass {
	Invisible,
	Solid,
	Liquid
}

public class Hazard : MonoBehaviour {

	public HazardClass hazardClass = HazardClass.Solid;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter2D (Collider2D other)
    {
    	// If the player enters the trigger zone...
    	if(other.tag == "Player")
    	{
    		BoxyControl playerController = other.GetComponent<BoxyControl>();

    		playerController.Die ();

    		// disable collision by destroying myself. Pretty hacky.
    		if ( hazardClass == HazardClass.Invisible )
    			Destroy( transform.root.gameObject );
    	}
    }
}
