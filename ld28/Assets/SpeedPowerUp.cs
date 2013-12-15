using UnityEngine;
using System.Collections;

public class SpeedPowerUp : MonoBehaviour {

	public TriggerPowerUpAnimation animationController;

	// Use this for initialization
	void Start () {
		// Setting up the reference.
		animationController = GameObject.FindGameObjectWithTag("PowerUpSprite").GetComponent<TriggerPowerUpAnimation>();
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		// If the player enters the trigger zone...
		if(other.tag == "Player")
		{
			animationController.StartSpeedAnimation();
			// Destroy me
			Destroy( transform.root.gameObject );
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
