﻿using UnityEngine;
using System.Collections.Generic;

public class Evolve : MonoBehaviour {
	/*
	 * Creation of the creatures will work like a factory
	 * Each prefab is a different part, so I need to clone the parts
	 * using a state machine, and save which state I chose.
	 * I use the genome to store information about the creature, so 
	 * that I can do crossover and mutation.
	 * 
	 * Fitness function will use a timer and subtract vectors
	 * 
	 * Selection will still use roulette wheel algorithm, based on fitness
	 * 
	 * Mutation will have a small chance to replace a pre-selected part with
	 * a randomly chosen part.
	 * 
	 * Crossover will scramble data between two different genomes
	 * 
	 */
	public class Genome{
		public int fitness;
		public int nSegments;
		public List<Color> colors;
		public List<int> components;
		public List<float> timerValues;
		public List<float> speeds;

		//Needs to remember number of joints per component,and what the joints are connected to
		public List<int> jointsPerComponent;
		public List<List<int>> connections;

		public Genome(){
			this.nSegments = 0;
			this.colors = new List<Color>();
			this.components = new List<int>();
			this.connections = new List<List<int>>();
			this.jointsPerComponent = new List<int>();
			this.timerValues = new List<float>();
			this.speeds = new List<float>();
			this.fitness = 0;
		}

	};

	//The individual parts needed to create the creature
	public GameObject segment;
	public GameObject wiggleSegment;
	public GameObject stretchSegment;
	public GameObject slideSegment;
	public float timeLimit;
	public static bool creatureInScene;
	public int nComponents;
	public int population;


	private Vector3 spawnPoint;
	private Genome[] genomes;
	//private Genome testCreature;


	private int generationCounter;
	private float timer;
	private bool isFirstGeneration;
	private int populationCounter;


	// Use this for initialization
	void Start () {
		genomes = new Genome[population];

		for(int i = 0; i < population; i++){
			genomes[i] = new Genome();
		}

		//testCreature = new Genome();
		generationCounter = 1;
		Debug.Log ("Creating Generation " + generationCounter);
		isFirstGeneration = true;
		populationCounter = 0;

		creatureInScene = false;
		spawnPoint = GameObject.Find ("SpawnPoint").transform.position;
		timer = 0;
	}


	void FixedUpdate () {
		
		timer += Time.fixedDeltaTime;

		if(timer <= 0.1f && !creatureInScene){
			//Create creatures individually instead of all at once.
			CreateCreature(genomes[populationCounter]);
			creatureInScene = true;
		}

		if(timer >= timeLimit && creatureInScene){

			//Estimate the fitness
			int fitness = GetFitness(GameObject.FindGameObjectWithTag("Segment").transform.position);
			genomes[populationCounter].fitness = fitness;
			//Debug.Log ("Fitness of creature " + populationCounter + ": " + fitness);
			GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag("Segment");

			foreach (GameObject g in objectsToDestroy){
				Destroy(g);
			}
			timer = 0;
			populationCounter++;


			//Once we have run the entire population of creatures, create the new generation
			if(populationCounter == population){

				generationCounter++;
				Debug.Log ("Creating Generation " + generationCounter);
				populationCounter = 0;
				isFirstGeneration = false;
				CreateNextGeneration();
			}

			creatureInScene = false;

		}

	}

	private void CreateNextGeneration(){
		//Creates the new generation of creatures out of the old generation
		Genome[] newGeneration = new Genome[population];
		for(int i = 0; i < population; i++){
			//For each creature in the population, select a pair to breed
			//These pairs are independent of one another
			int[] pair = Selection();
			Genome child = Crossover(genomes[pair[1]], genomes[pair[0]]);
			child = Mutate(child);
			newGeneration[i] = child;


		}
		//CreateCreature (newGeneration[0]);
		genomes = newGeneration;
	}

	private int[] Selection(){
		int a;
		int b;
		//Use roulette wheel selection to determine value of a and b
		
		//Create a weighted distribution
		List<int> distribution = new List<int>();

		for(int i = 0; i < population; i++){
			for(int j = 0; j< genomes[i].fitness; j++){
				distribution.Add(i);
			}
		}

		//Choose a random value out of the weighted distribution
		a = distribution[Random.Range (distribution.Count - 1, distribution.Count)];

		Debug.Log ("Distribution.Count: " + distribution.Count);
		//Make sure that the chosen value cannot be chosen again
		distribution.RemoveAll (item => item == a);


		//Pick a different random value out of the weighted distribution
		try{
			b = distribution[Random.Range(distribution.Count - 1, distribution.Count)];
		}catch(System.ArgumentOutOfRangeException oor ){
			Debug.Log ("Distribution.Count: " + distribution.Count);
			b = 0;
		}



		return new int[] {a, b};
	}


	private Genome Crossover(Genome a, Genome b){
		//G will be a container that takes in elements of a and b
		Genome g = new Genome();
		Genome biggerGenome;
		int maxSegments = Mathf.Max(a.nSegments, b.nSegments);
		int minSegments = Mathf.Min(a.nSegments, b.nSegments);

		if(a.nSegments == maxSegments){
			biggerGenome = a;
		}else{
			biggerGenome = b;
		}


		for(int i = 0; i < maxSegments; i++){
			if(i < minSegments){
				g.nSegments++;

				if(FlipCoin()){
					//Get elements from a
					CopySegment(g, a, i);
				}else{
					//Get elements from b
					CopySegment(g, b, i);
				}

			}else{

				if(FlipCoin()){
					g.nSegments++;
					//Get elements from bigger genome
					CopySegment (g, biggerGenome, i);
				}else{
					//Have to break here, since I might otherwise get an error with joint setup
					break;
				}

			}
		}

		return g;
	}

	private void CopySegment(Genome to, Genome from, int index){
		//Copy all segment values. I can add to this later on.
		to.colors.Add (from.colors[index]);
		to.components.Add (from.components[index]);
		to.connections.Add (from.connections[index]);
		to.jointsPerComponent.Add (from.jointsPerComponent[index]);
		to.speeds.Add (from.speeds[index]);
		to.timerValues.Add (from.timerValues[index]);
	}

	private Genome Mutate(Genome g){
		//Takes a genome and replaces one of its stored segments with a random segment
		//Also can change the properties of a segment and its color
		//Most of the time, Mutate will do nothing
		if(RollD (10) == 10){

			int mutatedSegment = Random.Range(0, g.nSegments);
			//Change the color so we know the segment has been mutated
			Color mutationColor = new Color(Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range(0f, 1f));

			if(RollD(3) == 1){
				//Replace component with a new, random component
				g.colors[mutatedSegment] = mutationColor;
				g.components[mutatedSegment] = Random.Range (0, nComponents);

			}else if (RollD(3) == 2) {
				//Edit existing properties of a component
				g.colors[mutatedSegment] = mutationColor;
				g.speeds[mutatedSegment] = Random.Range (1f, 4f);
				g.timerValues[mutatedSegment] = Random.Range (1f, 4f);

			}else if(RollD (3) == 3){
				//Add an entirely new component
				g.nSegments++;
				g.colors.Add (mutationColor);
				g.components.Add (Random.Range(0, nComponents));

				g.jointsPerComponent.Add (Random.Range (1, g.nSegments));
				g.connections.Add (new List<int>());
				SmartRandom smartRandom = new SmartRandom();
				for(int i = 0; i < g.jointsPerComponent[g.nSegments - 1]; i++){
					g.connections[g.nSegments - 1].Add (smartRandom.Range (0, g.nSegments - 1));
				}

				g.speeds.Add (Random.Range (1f, 4f));
				g.timerValues.Add (Random.Range (1f, 4f));
				
			}

		}
		return g;
	}

	private void CreateCreature(Genome g){
		//The creature and the genome are created together
		List<GameObject> segments = new List<GameObject>();
		//Set the number of segments. Should be random if not set yet.

		if(isFirstGeneration){
			//Switched to 2 for debug purposes
			g.nSegments = Random.Range (1, 5);
		}

		//Creatures are now a solid color. This allows us to see a creature's parents when they cross breed
		Color color = Color.white;

		if(isFirstGeneration){
			color =  new Color(Random.Range (0f, 1f),Random.Range (0f, 1f), Random.Range (0f, 1f));
			//g.colors.Add (color);
		}
		//Whenever there is more than one segment, add a joint between the two segments
		//A single segment can have more than one joint
		//No more than one joint should exist between two different segments
		for(int i = 0; i < g.nSegments; i++){
			//Range should be equal to the number of components
			//Adjust for off by 1 error

			int state;

			if(isFirstGeneration){
				state = Random.Range (0, nComponents);
				g.components.Add(state);
				g.colors.Add (color);
			}else{
				state = g.components[i];
				color = g.colors[i];
			}

			float timerValue;
			float speed;

			if(isFirstGeneration){
				timerValue = Random.Range (1f, 4f);
				speed = Random.Range (1f, 4f);
				g.speeds.Add (speed);
				g.timerValues.Add (timerValue);
			}else{
				timerValue = g.timerValues[i];
				speed = g.speeds[i];
			}

			GameObject o;


			//Random vector to prevent parts from sticking together due to all spawning in the same location
			Vector3 randomVect = new Vector3(Random.Range(0.01f, 0.1f), Random.Range(0.01f, 0.1f), 0);

			switch (state){

			case 0:
				//create both the gameObject and the joints here!
				o = Instantiate(segment, spawnPoint + randomVect, Quaternion.identity) as GameObject;
				o.GetComponent<SpriteRenderer>().color = color;
				segments.Add(o);
				o.name = "Segment" + i;
				//Debug.Log ("State: " + state);
				//o.tag = "unconnected";
				break;

			case 1:
				o = Instantiate(wiggleSegment, spawnPoint + randomVect, Quaternion.identity) as GameObject;
				o.GetComponent<SpriteRenderer>().color = color;
				o.GetComponent<Rotator>().timeLimit = timerValue;
				o.GetComponent<Rotator>().rotationSpeed = speed;
				segments.Add(o);
				o.name = "Segment" + i;
				//Debug.Log ("State: " + state);
				break;

			case 2:
				o = Instantiate(stretchSegment, spawnPoint + randomVect, Quaternion.identity) as GameObject;
				o.GetComponent<SpriteRenderer>().color = color;
				o.GetComponent<Stretcher>().timeLimit = timerValue;
				o.GetComponent<Stretcher>().speed = speed;
				segments.Add(o);
				o.name = "Segment" + i;
				//Debug.Log ("State: " + state);
				break;

			case 3:
				o = Instantiate(slideSegment, spawnPoint + randomVect, Quaternion.identity) as GameObject;
				o.GetComponent<SpriteRenderer>().color = color;
				o.GetComponent<Slider>().timeLimit = timerValue;
				o.GetComponent<Slider>().speed = speed;
				segments.Add(o);
				o.name = "Segment" + i;
				//Debug.Log ("State: " + state);
				break;

			case 4:
				o = Instantiate(slideSegment, spawnPoint + randomVect, Quaternion.identity) as GameObject;
				o.AddComponent<Rotator>();
				o.GetComponent<Rotator>().timeLimit = timerValue;
				o.GetComponent<Rotator>().rotationSpeed = speed;
				o.GetComponent<Slider>().timeLimit = timerValue;
				o.GetComponent<Slider>().speed = speed;
				o.GetComponent<SpriteRenderer>().color = color;
				segments.Add(o);
				o.name = "Segment" + i;
				//Debug.Log ("State: " + state);
				break;

			case 5:
				o = Instantiate(stretchSegment, spawnPoint + randomVect, Quaternion.identity) as GameObject;
				o.AddComponent<Rotator>();
				o.GetComponent<Stretcher>().timeLimit = timerValue;
				o.GetComponent<Stretcher>().speed = speed;
				o.GetComponent<Rotator>().timeLimit = timerValue;
				o.GetComponent<Rotator>().rotationSpeed = speed;
				o.GetComponent<SpriteRenderer>().color = color;
				segments.Add(o);
				o.name = "Segment" + i;
				//Debug.Log ("State: " + state);
				break;

			case 6:
				o = Instantiate(slideSegment, spawnPoint + randomVect, Quaternion.identity) as GameObject;

				o.AddComponent<Stretcher>();
				o.GetComponent<Stretcher>().timeLimit = timerValue;
				o.GetComponent<Stretcher>().speed = speed;

				o.GetComponent<Slider>().timeLimit = timerValue;
				o.GetComponent<Slider>().speed = speed;

				o.GetComponent<SpriteRenderer>().color = color;
				segments.Add(o);
				o.name = "Segment" + i;
				//Debug.Log ("State: " + state);
				break;

			case 7:
				o = Instantiate(segment, spawnPoint + randomVect, Quaternion.identity) as GameObject;

				o.AddComponent<Rotator>();
				o.GetComponent<Rotator>().timeLimit = timerValue;
				o.GetComponent<Rotator>().rotationSpeed = speed;

				o.AddComponent<Stretcher>();
				o.GetComponent<Stretcher>().timeLimit = timerValue;
				o.GetComponent<Stretcher>().speed = speed;

				o.AddComponent<Slider>();
				o.GetComponent<Stretcher>().timeLimit = timerValue;
				o.GetComponent<Stretcher>().speed = speed;

				o.GetComponent<SpriteRenderer>().color = color;
				o.GetComponent<Slider>().timeLimit = timerValue;
				o.GetComponent<Slider>().speed = speed;

				segments.Add(o);
				o.name = "Segment" + i;
				//Debug.Log ("State: " + state);
				break;

			default:
				Debug.Log ("Error: nComponents negative or greater than the actual number of components");
				break;

			}
			//Adds joints to each segment. Can only connect with segments that were generated previously
			//This can be used to guarantee a segment will not be connected to itself
			//Use Random.Range(0, segments.Count -2) if segments.Count >= 2;
			//Segments[count - 1] refers to the latest generated game object

			//nJoints is initialized here so that segments without joints have a place on the list
			//This is essentially a place holder
			int nJoints = 0;
			if(segments.Count == 1 && isFirstGeneration){
				g.jointsPerComponent.Add (nJoints);
				//g.connections.Add (new List<int>());
			}

			//Keeps track of what the joints are connected to
			//Also needs a place holder
			List<int> connectionList = new List<int>();

			if(isFirstGeneration){
				g.connections.Add (connectionList);
			}else{
				connectionList = g.connections[i];
			}

			if(segments.Count >= 2){
				//Maximum number of joints per segment should be at most the number of segments in the scene
				//Minimum number should be at least 1, since I want every segment beyond the first to be connected to the creature
				if(isFirstGeneration){
					nJoints = Random.Range (1, segments.Count);
					g.jointsPerComponent.Add (nJoints);
				}else{
					nJoints = g.jointsPerComponent[i];
					
				}

				//This can be used for adding more joint types later. For now, just spring joints will be fine.
				//int jointType = 0;
				SmartRandom jointSelector = new SmartRandom();
				List<SpringJoint2D> segmentJoints = new List<SpringJoint2D>();
				for(int j = 0; j < nJoints; j++){

					segmentJoints.Add (segments[i].AddComponent<SpringJoint2D>());
					SpringJoint2D springJoint = segmentJoints[j];
					//Randomly selects which other object to attach itself to
					//Use SmartRandom class to prevent double jointing
					int index;

					if(isFirstGeneration){
						index = jointSelector.Range (0, segments.Count - 1);
						g.connections[i].Add (index);
						//Debug.Log ("Index: " + index + " SegmentNumber: " + i + " Segments.Count: " + segments.Count);
					}else{
						try{
							index = g.connections[i][j];
						}catch(System.ArgumentOutOfRangeException oor){
							index = 0;
							Debug.Log ("Argument out of range exception. i: " + i + " j: " + j + 
							           " g.connections.Count: " + g.connections.Count + " g.connections[i].Count " + g.connections[i].Count);
						}
					}

					springJoint.connectedBody = segments[index].rigidbody2D;
					springJoint.distance = 5;
					springJoint.dampingRatio = 10;
	
				}
			}
		}

	}

	//Helper functions
	
	private int Round(float f){
		return (int)(f + 0.5f);
	}
		
	private int GetFitness(Vector3 v){
		return Round(Mathf.Abs(v.x - spawnPoint.x));
	}

	private bool FlipCoin(){
		bool b = false;
		if(Round (Random.Range(0f, 1f)) == 1){
			b = true;
		}
		return b;
	}

	private int RollD(int sides){
		return Random.Range (0, sides) + 1;
	}


}
