using UnityEngine;
using System.Collections;

public class TriggerPowerUpAnimation : MonoBehaviour {

	private Animator anim;					// Reference to the player's animator component.

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
	}

	public void StartSpeedAnimation()
	{
		anim.SetTrigger("SpeedPower");
	}

	// Update is called once per frame
	void Update () {

	}
}
