using UnityEngine;
using System.Collections;

public class GamePrefs : MonoBehaviour {

	// Use this for initialization
	void Start () {
        PlayerPrefs.SetInt( "HasDiedByHavingTwo", 0 );	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
