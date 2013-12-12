using UnityEngine;

public class BoxyControl : MonoBehaviour
{
    public float moveForce = 365f;
    public float antiMoveForce = 200f;
    public float maxSpeed = 5f;

    void Awake()
    {
    }

    void Update()
    {
    }

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

    }
}
