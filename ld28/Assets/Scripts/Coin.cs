using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour {

	public enum CoinColor {
		Red,
		Blue,
		Green,
		Orange,
		None
	}

	public CoinColor color;

	void Awake ()
	{

	}
	
	void OnTriggerEnter2D (Collider2D other)
	{
		// If the player enters the trigger zone...
		if(other.tag == "Player")
		{
			BoxyControl playerController = other.GetComponent<BoxyControl>();

			playerController.HandleGetCoin( color );

			// Destroy me
			Destroy(transform.root.gameObject);
		}
	}
}
