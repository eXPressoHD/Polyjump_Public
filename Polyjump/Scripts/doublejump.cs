using UnityEngine;
using System.Collections;

public class doublejump : MonoBehaviour {


	public float speed;
	public float jumpSpeed;
	bool OnGround = true;
	public int jumps = 2;
	Rigidbody rb;
	bool jump = false;

	void Start() {
		rb = GetComponent<Rigidbody> ();
		jumps = 2;
	}

	void OnCollisionExit(Collision collisionInfo)
	{
		OnGround = false;
	}

	void OnCollisionStay(Collision collisionInfo)
	{
		OnGround = true;
		jumps = 2;
	}

	void Update()
	{ 
		if (Input.GetKeyDown (KeyCode.Space) && OnGround == true) {
			jump = true;
		}
	}


		
	void FixedUpdate () {
		float moveHorizontal = Input.GetAxisRaw("Horizontal");
		float moveVertical = Input.GetAxisRaw("Vertical");

		Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

		rb.AddForce(movement * speed);


		if (jump == true)
		{
			rb.AddForce(Vector3.up * 20);

			if(jumps >= 1)
			jumps = jumps - 1;
			rb.AddForce(Vector3.up * 20);
		}
	}
}