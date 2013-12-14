using UnityEngine;

public class BoxyControl : MonoBehaviour
{
	public enum PlayerState {
		Walking,
		StuckOnSide,
		Jumping,
	};

	public PlayerState PreviousState = PlayerState.Walking;
	public PlayerState State = PlayerState.Walking;
	public float PlayerDecel = 0.2f; 		//The amount the player slows down
	public float PlayerAccel = 0.08f; 		// The amount the player accelerates by
    public float maxSpeed = 5f;
	public float jumpSpeed = 1f;
	public float maxJumpSpeed = 10f;

	public float[] RequiredTorque;

	public float JumpingTime = 0.0f;
	public float MaxJumpingTime = 2.0f;

	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	public float jumpForce = 1000f;			// Amount of force added when the player jumps.
	public bool grounded = false;

	public Coin.CoinColor firstColor = Coin.CoinColor.None;
	private int numCoins = 0;

	public int coinsNeeded = -1;
	public string nextLevel = "";

    void Awake()
    {
		// Setting up references.
		groundCheck = transform.Find("groundCheck");
    }

	private bool IsRotated( float RotationZ )
	{
		return RotationZ <= 92  && RotationZ >= 88 
			|| RotationZ <= 182 && RotationZ >= 178
			|| RotationZ <= 272 && RotationZ >= 268;
	}

	private float GetAmountToFlip( float RotationZ )
	{
		if( RotationZ <= 92  && RotationZ >= 88  )
			return RequiredTorque[0];
		else if( RotationZ <= 182 && RotationZ >= 178 )
			return RequiredTorque[1];
		else if( RotationZ <= 272 && RotationZ >= 268 )
			return RequiredTorque[2];
		else{
			print ( "We shouldn't get here!" );
			return 0;
		}
	}

    void Update()
    {
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  

		//Player is stuck on his side
		if( State == PlayerState.StuckOnSide )
		{
			//Check to see if a miracle happened and the player got up.
			if ( !IsRotated( transform.rotation.eulerAngles.z ) && grounded )
			{
				PreviousState = State;
				State = PlayerState.Walking;
			}
		}
		else if( IsRotated( transform.rotation.eulerAngles.z ) && rigidbody2D.velocity.y == 0 )
		{
			PreviousState = State;
			State = PlayerState.StuckOnSide;
			rigidbody2D.velocity = new Vector2( 0, rigidbody2D.velocity.y );
		}

		// If the jump button is pressed and the player is grounded then the player should jump.
		if(Input.GetButtonDown("Jump"))
		{

			if( State == PlayerState.StuckOnSide )
			{
				rigidbody2D.AddTorque( GetAmountToFlip( transform.rotation.eulerAngles.z ) );
				PreviousState = State;
				State = PlayerState.Walking;
			}
			else if( grounded )
			{
				PreviousState = State;
				State = PlayerState.Jumping;
			}
		}
    }
	
	private float TendToZero(float val, float amount)
	{
		if (val > 0f && (val -= amount) < 0f) return 0f;
		if (val < 0f && (val += amount) > 0f) return 0f;
		return val;
	}

	public float SpeedX;

    void FixedUpdate()
    {
        float h = Input.GetAxis( "Horizontal" );
		SpeedX = rigidbody2D.velocity.x;
		float SpeedY = rigidbody2D.velocity.y;

		if( State != PlayerState.StuckOnSide )
		{
			if( h > 0f )
			{
				SpeedX = rigidbody2D.velocity.x + PlayerAccel + PlayerDecel;
				if( SpeedX > maxSpeed )
				{
					SpeedX = maxSpeed;
				}
			}
			else if( h < 0f )
			{
				SpeedX = rigidbody2D.velocity.x - (PlayerAccel + PlayerDecel);
				if( SpeedX < -maxSpeed )
					SpeedX = -maxSpeed;
			}

			SpeedX = TendToZero( SpeedX, PlayerDecel );
		}
		if( State == PlayerState.Jumping ){
			if( PreviousState != PlayerState.Jumping )
			{
				rigidbody2D.AddForce(new Vector2(0f, jumpForce));
				PreviousState = State;
				return;
			}
			else if( Input.GetButton("Jump") )
			{
				SpeedY += jumpSpeed;
				JumpingTime += Time.fixedDeltaTime;
				if( JumpingTime >= MaxJumpingTime )
				{
					State = PlayerState.Walking;
					JumpingTime = 0f;
				}
			}
			else //Not jumping anymore, so begin descent
			{
				State = PlayerState.Walking;
				JumpingTime = 0f;
			}
		}

		if( rigidbody2D.velocity.x != SpeedX || rigidbody2D.velocity.y != SpeedY )
		{
			rigidbody2D.velocity = new Vector2( SpeedX, SpeedY );
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
			// do something: game over
		}
	}
}
