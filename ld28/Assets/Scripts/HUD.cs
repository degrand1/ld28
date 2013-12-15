using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private static string colorToTag( Coin.CoinColor color ) {
        switch (color)
        {
            case Coin.CoinColor.Blue:
                    return "Blue Coin";
            case Coin.CoinColor.Green:
                    return "Green Coin";
            case Coin.CoinColor.Orange:
                    return "Orange Coin";
            case Coin.CoinColor.Red:
                    return "Red Coin";
            case Coin.CoinColor.None:
                    return "White Coin";
            default: return "White Coin";
        }
    }

    public static int GetCoinsInLevel( Coin.CoinColor color ) {
        return GameObject.FindGameObjectsWithTag( HUD.colorToTag( color ) ).Length;
    }

    void OnGUI() {
        int counter = 0; //used to offset each GUI label
        foreach ( Coin.CoinColor color in ( Coin.CoinColor[] ) Coin.CoinColor.GetValues( typeof( Coin.CoinColor ) ) ) { //go kill yourself, C#
            if ( GetCoinsInLevel( color ) > 0 ) {
                GUI.Label( new Rect( Screen.width - 80, 15 * counter, 60, 20 ), "0/0" );
                counter++;
            }
        }
    }
}
