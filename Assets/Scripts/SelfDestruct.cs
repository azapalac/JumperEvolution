using UnityEngine;
using System.Collections;

public class SelfDestruct : MonoBehaviour {
	private float t;
	private Evolve evolve;
	// Use this for initialization
	void Start () {
		t = 0;
		evolve = GameObject.Find("Evolver").GetComponent<Evolve>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		t += Time.deltaTime;

		if(t > evolve.timeLimit + 2){
			t = 0;
			Destroy(this.gameObject);


		}
	}
}
