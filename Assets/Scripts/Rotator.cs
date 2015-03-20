using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {
	Rigidbody2D rigidbody;
	private float theta;
	public float rotationSpeed;
	public float timeLimit;
	private bool rotateClockwise;
	private float startTime;
	private float timer;

	// Use this for initialization
	void Start () {
		rotateClockwise = true;
		//startTime = Time.time;
		//rotationSpeed = 5f;
		timer = 0f;
		//theta  = 90f;
		rigidbody = GetComponent<Rigidbody2D>();
		//Set theta equal to a random angle
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	 //rotate to a random angle, at a constantSpeed
	//Instead of making the angle random, make the speed random
			if(Mathf.Abs(rigidbody.angularVelocity) <= rotationSpeed){
				if(rotateClockwise){
					rigidbody.AddTorque(-100f, ForceMode2D.Force);
				}else{
					rigidbody.AddTorque(100f, ForceMode2D.Force);
				}

			}

			if(timer >= timeLimit){
				//Toggle rotate clockwise every 2 seconds
				rotateClockwise = !rotateClockwise;
				timer = 0;
			}
			timer += Time.deltaTime;

	 //rigidbody.angularVelocity
	 //Rotate back to original angle
	}

}
