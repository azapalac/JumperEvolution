using UnityEngine;
using System.Collections;

public class Slider : MonoBehaviour {

	public float speed;
	public float timeLimit;
	private int moveDir;
	private bool canMove;
	private float t;
	// Use this for initialization
	void Start () {
		t = 0;

		//Remember to change this 
		//timeLimit = 2;
		canMove = false;
		moveDir = 1;
		//speed  = 10;
	}
	
	// Update is called once per frame
	void Update () {

		if(canMove){
			if(Mathf.Abs (rigidbody2D.velocity.x) <= speed){
				rigidbody2D.AddForce (new Vector2(moveDir * 100, 0), ForceMode2D.Force);
			}

			if(t >= timeLimit){
				//Switch movement direction and reset timer
				rigidbody2D.isKinematic = true;
				moveDir *= -1;
				t = 0;
				rigidbody2D.isKinematic = false;
			}
			t += Time.deltaTime;
		}
	}

	void OnCollisionEnter2D(Collision2D other){
		if(other.gameObject.tag == "Cliff"){
			canMove = true;
		}
	}

	void OnCollisionExit2D(Collision2D other){
		if(other.gameObject.tag == "Cliff"){
			canMove = false;
		}
	}

}
