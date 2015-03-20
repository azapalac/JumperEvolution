using UnityEngine;
using System.Collections;

public class Stretcher : MonoBehaviour {

	private float t;
	private bool canMove;
	public float timeLimit;
	public float speed;

	// Use this for initialization
	void Start () {
		t = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if(canMove){
			transform.localScale += new Vector3(0, speed*Time.deltaTime, 0);

			if(t >= timeLimit){
				speed *=  -1;
				t = 0;
			}

			t += Time.fixedDeltaTime;
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
