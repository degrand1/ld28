using UnityEngine;

public enum BoxyState {
	Dead,
	Normal,
	TooCool,
	Horrified
}

public class BoxyControl : MonoBehaviour
{
    public float moveForce = 365f;
    public float antiMoveForce = 200f;
    public float maxSpeed = 5f;

	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	public float jumpForce = 1000f;			// Amount of force added when the player jumps.
	private bool jump = false;
	public bool grounded = false;

	public Coin.CoinColor firstColor = Coin.CoinColor.None;
	private int numCoins = 0;

	public int coinsNeeded = -1;
	public string nextLevel = "";

	public BoxyState state = BoxyState.Normal;

	private Sprite normalSprite;
	private Sprite deadSprite;
	private Sprite coolSprite;
	private Sprite horrifiedSprite;

    void Awake()
    {
		// Setting up references.
		groundCheck = transform.Find("groundCheck");

        // load sprites
        normalSprite     = Resources.Load<Sprite>( "boxy" );
        deadSprite       = Resources.Load<Sprite>( "boxy_dead" );
        coolSprite       = Resources.Load<Sprite>( "boxy_deal_with_it" );
        horrifiedSprite  = Resources.Load<Sprite>( "boxy_shock" );

        print( normalSprite );
    }

    void Update()
    {
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  
		
		// If the jump button is pressed and the player is grounded then the player should jump.
		if(Input.GetButtonDown("Jump") && grounded)
			jump = true;

		SpriteRenderer renderer = GetComponent<SpriteRenderer> ();

		// update sprite to reflect state
		switch( state ) {
		case BoxyState.Dead:        renderer.sprite = deadSprite;       break;
		case BoxyState.Normal:      renderer.sprite = normalSprite;     break;
		case BoxyState.TooCool:     renderer.sprite = coolSprite;       break;
		case BoxyState.Horrified:   renderer.sprite = horrifiedSprite;  break;
		default:                    renderer.sprite = normalSprite;     break;
		}
    }
	
	// If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
	//if(h * rigidbody2D.velocity.x < maxSpeed)

    void FixedUpdate()
    {
        float h = Input.GetAxis( "Horizontal" );

        rigidbody2D.AddForce( Vector2.right * h * moveForce );

        if ( Mathf.Abs( rigidbody2D.velocity.x ) > maxSpeed ) {
            rigidbody2D.velocity = new Vector2( Mathf.Sign( rigidbody2D.velocity.x ) * maxSpeed, rigidbody2D.velocity.y );
        }
		
        // do not let player slide. Apply resistance.
        if ( Mathf.Abs( h ) < Mathf.Epsilon && Mathf.Abs( rigidbody2D.velocity.x ) > maxSpeed / 3 ) {
            rigidbody2D.AddForce( Vector2.right * -Mathf.Sign( rigidbody2D.velocity.x ) * antiMoveForce );
        }

		if( jump ){
			// Add a vertical force to the player.
			rigidbody2D.AddForce(new Vector2(0f, jumpForce));
			jump = false;
		}
    }

	private string colorToTag( Coin.CoinColor color ) {
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

	public void HandleGetCoin( Coin.CoinColor color ) {
		print (color);

		if (firstColor == Coin.CoinColor.None) {
			firstColor = color;
			coinsNeeded = GameObject.FindGameObjectsWithTag( colorToTag( color ) ).Length;
		}
			
		if (firstColor == color) {
			numCoins++;
			if ( numCoins == coinsNeeded ) {
				Application.LoadLevel( nextLevel );
			}
		} else {
            state = BoxyState.Dead;
		}
	}
}
