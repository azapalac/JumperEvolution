using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
	public Creature C;
	public int damage = 10;
    bool isMoving = true;
    private float timeLimit = 0.3f;
    private float t;
	// Use this for initialization
	void Start () {
        StartCoroutine(IsMoving());
	}
	
	// Update is called once per frame
	void Update () {
        t += Time.deltaTime;

        if (t >= timeLimit)
        {
            StartCoroutine(IsMoving());
            t = 0;
        }
    }

	void OnCollisionEnter2D(Collision2D other){
		if(other.gameObject.tag == "Segment" && isMoving){
			if(other.gameObject.GetComponent<Body>().creature.number != C.number){
				other.gameObject.GetComponent<Body>().creature.HP -= damage;
                C.score += 10;
				if(other.gameObject.GetComponent<Body>().creature.score < 1)
					other.gameObject.GetComponent<Body>().creature.score = 1;
			}
            
		}

	}

    private IEnumerator IsMoving()
    {
       
        Vector3 prevPos = this.gameObject.transform.position;
        yield return new WaitForSeconds(0.3f);
        Vector3 currentPos = this.gameObject.transform.position;

        if(Vector3.Magnitude(prevPos - currentPos) >= 0.1f)
        {
            isMoving = true;
            
        }
        else
        {
            isMoving = false;
        }
        
    }
}
