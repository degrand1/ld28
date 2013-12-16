using UnityEngine;

public enum BoxyFeeling {
    Dead,
    Normal,
    TooCool,
    Horrified
}

public enum CauseOfDeath {
    HadTwo,
    Fell,
}

public enum PlayerState {
    Walking,
    StuckOnSide,
    Jumping,
};

public class BoxyControl : MonoBehaviour
{

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
    public float slopeNormalForce = 100f;   // Amount of fudge force applied to keep player on slope
	public bool grounded = false;
    private bool jumpedOffSlope = false;

	public Coin.CoinColor firstColor = Coin.CoinColor.None;
	private int numCoins = 0;

	public int coinsNeeded = -1;
	public string nextLevel = "";

	public BoxyFeeling feeling = BoxyFeeling.Normal;
	public float tooCoolLength = 1.0f;

	private Sprite normalSprite;
	private Sprite deadSprite;
	private Sprite coolSprite;
	private Sprite horrifiedSprite;

    private bool restartedSinceLastDeath = false;

    public float maxTextAnimTime = 2.0f;
    public float textAnimTimeAcc = 0.0f;

    private GUIStyle animTextStyle = new GUIStyle();

    public AudioClip coinSfx;
    public AudioClip lastCoinSfx;
    public AudioClip deathSfx;
    public AudioClip loJumpSfx;
    public AudioClip warningSfx;

    private bool hasPlayedWarningSfx = false;

    public void Die( CauseOfDeath cause = CauseOfDeath.Fell ) {
        if ( feeling == BoxyFeeling.Dead )
            return;

        feeling = BoxyFeeling.Dead;
        PlayerPrefs.SetInt( "CauseOfDeath", (int) cause );
        RagDollMe();
        restartedSinceLastDeath = false;

        if ( PlayerPrefs.GetInt( "HasDiedByHavingTwo" ) == 1 ) {
            Invoke( "RestartLevel", 1f );
        } else if ( cause == CauseOfDeath.HadTwo ){
            // allow text animation to play before resetting
            Invoke( "SetDiedFlag", maxTextAnimTime );
        } else { // cause == CauseOfDeath.Fell
            Invoke( "RestartLevel", 1f ); // simple restart, like the first condition
        }

        PlayClip( deathSfx );
    }

    private void SetDiedFlag() {
        PlayerPrefs.SetInt( "HasDiedByHavingTwo", 1 );
        RestartLevel();
    }

    // overlay code
    void OnGUI() {
        if ( feeling == BoxyFeeling.Dead && PlayerPrefs.GetInt( "HasDiedByHavingTwo" ) == 0 && PlayerPrefs.GetInt( "CauseOfDeath" ) == (int) CauseOfDeath.HadTwo ) { // wow, this is definitely one of the worst conditionals I've ever written
            if ( !hasPlayedWarningSfx ) {
                AudioSource.PlayClipAtPoint( warningSfx, new Vector2( transform.position.x, transform.position.y ) );
                hasPlayedWarningSfx = true;
            }
            Texture2D warning = Resources.Load<Texture2D>( "warning" );
            Texture2D onlyone = Resources.Load<Texture2D>( "you_only_get_one" );
            GUI.Label( new Rect( ( Screen.width - warning.width ) / 2, ( Screen.height - onlyone.height) / 2 - onlyone.height + 20, warning.width, warning.height ), // 20 is fudge
                       warning );
            GUI.Label( new Rect( ( Screen.width - onlyone.width ) / 2, ( Screen.height - onlyone.height ) / 2, onlyone.width, onlyone.height ),
                       onlyone );
        } else {
        
        }
    }

    void Awake()
    {
		// Setting up references.
		groundCheck = transform.Find("groundCheck");

        // load sprites
        normalSprite     = Resources.Load<Sprite>( "boxy" );
        deadSprite       = Resources.Load<Sprite>( "boxy_dead" );
        coolSprite       = Resources.Load<Sprite>( "boxy_deal_with_it" );
        horrifiedSprite  = Resources.Load<Sprite>( "boxy_shock" );

        // set up GUI style
        animTextStyle.fontSize = 100;
        animTextStyle.normal.textColor = Color.black;
        animTextStyle.alignment = TextAnchor.MiddleCenter;
        animTextStyle.normal.background = Resources.Load<Texture2D>( "white" );
    }

	public bool IsRotated( float RotationZ )
	{
		return RotationZ <= 62  && RotationZ >= 58
			|| RotationZ <= 92  && RotationZ >= 88
			|| RotationZ <= 152 && RotationZ >= 148
			|| RotationZ <= 182 && RotationZ >= 178
			|| RotationZ <= 242 && RotationZ >= 238
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
		else if( RotationZ <= 62 && RotationZ >= 58 )
			return RequiredTorque[3];
		else if( RotationZ <= 152 && RotationZ >= 148 )
			return RequiredTorque[4];
		else if( RotationZ <= 242 && RotationZ >= 238 )
			return RequiredTorque[5];
		else{
			print ( "We shouldn't get here!" );
			return 0;
		}
	}

    void Update()
    {
        if ( feeling == BoxyFeeling.Dead && PlayerPrefs.GetInt( "HasDiedByHavingTwo" ) == 0 )
            textAnimTimeAcc += Time.deltaTime;
        
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        jumpedOffSlope = grounded ? false : jumpedOffSlope; // clear jumpedOffSlope everytime we get grounded

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

        // restart level if dead
        if ( Input.GetButtonDown( "Jump" ) && feeling == BoxyFeeling.Dead )
            RestartLevel();

		SpriteRenderer renderer = GetComponent<SpriteRenderer> ();

		// update sprite to reflect state
		switch( feeling ) {
		case BoxyFeeling.Dead:        renderer.sprite = deadSprite;       break;
		case BoxyFeeling.Normal:      renderer.sprite = normalSprite;     break;
		case BoxyFeeling.TooCool:     renderer.sprite = coolSprite;       break;
		case BoxyFeeling.Horrified:   renderer.sprite = horrifiedSprite;  break;
		default:                    renderer.sprite = normalSprite;     break;
		}

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
                float rotation = transform.rotation.eulerAngles.z;
                if ( rotation > 15 && rotation < 345 ) // if on slope, set flag so we can use it while moving in midair
                    jumpedOffSlope = true;
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
        float rotation = transform.rotation.eulerAngles.z;
        float rotRad = rotation * Mathf.Deg2Rad; // for convenience

		if( State != PlayerState.StuckOnSide )
		{
			if( h > 0f )
			{
				SpeedX = Mathf.Cos( rotRad ) * ( rigidbody2D.velocity.x + PlayerAccel + PlayerDecel ); // goes a bit slower on slopes

				if( SpeedX > maxSpeed )
					SpeedX = maxSpeed;

                if ( rotation > 15 && rotation < 345 && grounded ) // if on slope, apply normal force toward the slope
                    rigidbody2D.AddForce( -1 * new Vector2( Mathf.Sin( rotRad ), Mathf.Cos( rotRad ) ) * slopeNormalForce );

			}
			else if( h < 0f )
			{
                // we want to apply an equivalent force upwards
				SpeedX = Mathf.Cos( rotRad ) * ( rigidbody2D.velocity.x - (PlayerAccel + PlayerDecel) ); // goes a bit slower on slopes

				if( SpeedX < -maxSpeed )
					SpeedX = -maxSpeed;

                if ( rotation > 15 && rotation < 345 && grounded ) // if on slope, apply normal force toward the slope
                    rigidbody2D.AddForce( -1 * new Vector2( Mathf.Sin( rotRad ), Mathf.Cos( rotRad ) ) * slopeNormalForce );
			}

            if ( !( ( rotation > 15 && rotation < 345 && grounded ) || jumpedOffSlope ) ) { // do not use this code path if we are on a slope or jumped off of one
                SpeedX = TendToZero( SpeedX, PlayerDecel );
            }
		}
		if( State == PlayerState.Jumping ) {
			if( PreviousState != PlayerState.Jumping )
			{
                if ( rotation > 15 && rotation < 345 && grounded ) // this is still too buggy to use for all jumps
                    rigidbody2D.AddForce( new Vector2( -Mathf.Sin( rotRad ), Mathf.Cos( rotRad ) ) * jumpForce );
                else
                    rigidbody2D.AddForce( new Vector2( 0f, jumpForce ) );

                AudioSource.PlayClipAtPoint( loJumpSfx, new Vector2( transform.position.x, transform.position.y ) );
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


    private void LoadNextLevel() {
        Application.LoadLevel( nextLevel );
    }

    private void RestartLevel() {
        if ( !restartedSinceLastDeath ) {
            Application.LoadLevel(Application.loadedLevel);
            restartedSinceLastDeath = true;
        }
    }

    // 2D ragdoll FTW
    private void RagDollMe() {
        if ( !grounded ) {
            // choose random torque direction, then choose a random torque value between 25 and 200
            rigidbody2D.AddTorque( Mathf.Sign( Random.value - 0.5f ) * ( 25f + Random.value * 175f ) );
        } else {
            // fall down in the direction we're going
            rigidbody2D.AddTorque( Mathf.Sign( rigidbody2D.velocity.x ) * -50f );
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

    private int getCoinsInLevel( Coin.CoinColor color ) {
        return GameObject.FindGameObjectsWithTag( colorToTag( color ) ).Length;
    }

    private void PlayClip( AudioClip clip ) {
        AudioSource player = GetComponent<AudioSource>();
        player.Stop();
        player.clip = clip;
        player.Play();
    }

	public bool HandleGetCoin( Coin.CoinColor color ) {

        if ( feeling == BoxyFeeling.TooCool || feeling == BoxyFeeling.Dead ) {
            return false; //don't get no mo coins
        }

		if (firstColor == Coin.CoinColor.None) {
			firstColor = color;
            coinsNeeded = getCoinsInLevel( color );
		}
			
		if (firstColor == color) {
			numCoins++;
			if ( numCoins == coinsNeeded ) {
                PlayClip( lastCoinSfx );
                feeling = BoxyFeeling.TooCool;
                Invoke( "LoadNextLevel", tooCoolLength );
			} else {
                PlayClip( coinSfx );
            }
		} else {
            Die( CauseOfDeath.HadTwo );
		}

        return true;
	}
}
