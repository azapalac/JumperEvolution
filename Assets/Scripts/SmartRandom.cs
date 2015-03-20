using UnityEngine;
using System.Collections.Generic;

public class SmartRandom {
	public List<int> pool;
	private List<int> privatePool;


	public SmartRandom(){
		pool =  new List<int>();
		privatePool = new List<int>();

	}

	private void Setup(int min, int max){
		//Debug.Log ("Setting up SmartRandom pools");
		pool.Clear();
		privatePool.Clear();

		for(int i = min; i < max; i++){
			pool.Add (i);
			privatePool.Add (i);
		}

	}

	public void Reset(){
		Setup (pool[0], pool[pool.Count - 1]);
	}

	//Computes between a range of ints
	//Never computes the same value twice
	//This DOES NOT work with floats! 
	//Also doesn't work with an uneven distribution
	public int Range(int a, int b){ 
		//Check for errors and if I need to set up

		if( b >= a){
			if(b - a > pool.Count){
				Setup (a, b);
			}

			int i = Random.Range (0, privatePool.Count);
			int r = privatePool[i];
			privatePool.Remove(i);

			return r;
		}else{
			Debug.Log ("Error: B < A");
			return -1;
		}
	}
}
