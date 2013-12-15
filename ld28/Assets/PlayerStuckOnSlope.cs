using UnityEngine;
using System.Collections;

public class PlayerStuckOnSlope : MonoBehaviour {

	private Transform topStuckCheck;			
	private Transform leftStuckCheck;			
	private Transform rightStuckCheck;
	private BoxyControl playerController;

	// Use this for initialization
	void Start () {
		topStuckCheck = transform.Find("TopStuckCheck");
		leftStuckCheck = transform.Find("LeftStuckCheck");
		rightStuckCheck = transform.Find("RightStuckCheck");
		playerController = GetComponent<BoxyControl>();
	}
	
	// Update is called once per frame
	void Update () {
		bool Stuck = Physics2D.Linecast(transform.position, topStuckCheck.position, 1 << LayerMask.NameToLayer("Ground"))
		|| Physics2D.Linecast(transform.position, leftStuckCheck.position, 1 << LayerMask.NameToLayer("Ground"))
		|| Physics2D.Linecast(transform.position, rightStuckCheck.position, 1 << LayerMask.NameToLayer("Ground"));

		if( Stuck && playerController.IsRotated( playerController.transform.rotation.eulerAngles.z ) ) {
			playerController.State = PlayerState.StuckOnSide;
		}
	}
}
