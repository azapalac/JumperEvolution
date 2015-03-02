using UnityEngine;
using System.Collections.Generic;

public class Evolver : MonoBehaviour {

	//private GameObject[] creatures;
	public int initialPopulation;
	public int generations;
	private int minVertices;
	private int maxVertices;

	public struct Creature {
		//public Genome genome;
		public List<int> seeds;
		public float fitness;
		public int nVertices;
		public int nLinks;

	};
	//Use a state machine to create random node typs
	//Have a state for each type of node, and a random number that chooses between them
	public Creature[] creatures;

	//This stores all possible types of vertices. 
	//During mutation and initial generation, new vertices will be randomly chosen from this array.
	private Vertex[] vertexPool;


	// Use this for initialization
	void Start () {
		creatures = new Creature[initialPopulation];
		CreatePopulation(initialPopulation);
		generations = 0;
	}

	private void AddSeed(int i, int seed){
		creatures[i].seeds.Add(seed);
	}
	// Update is called once per frame
	void Update () {

		//Use timers with this
		RunPopulation();
	}

	public void CreatePopulation(int population){
		//Creates a random population
		//Make a random directed graph for each 
		for(int i = 0; i < population; i++){
			CreateGenome ();
		}
		
	}

	private Genome CreateGenome(){
		int graphSize = (int)(Random.Range(minVertices, maxVertices));

		//Grab the entire range of all the objects
		int state = (int)(Random.Range(0, 1) + 0.5f);
		Genome G = new Genome();

		//I need a state for each child of Vertex I create
		for(int i = 0; i < graphSize; i++){
			switch(state){
			case 0:
				Clock v = new Clock();
				G.AddVertex (v);
				v.Initialize();
				break;

			case 1:

				break;
			}
		}
		return G;
	}

	public void CreatePopulation(int population, List<int>[] seeds){
		//Creates a population based on predetermined seeds
	}

	public Genome CreateCreature(){
		//Creates a random creature
		Genome G = new Genome();

		return G;
	}

	public Genome CreateCreature(List<int> seedValues){
		//Creates a creature based on a seed input
		Genome G = new Genome();

		return G;

	}

	public void RunPopulation(){

		int a = -1;
		int b = -1;
		DeterminePairs(a, b);
		int[] seedA = creatures[a].seeds.ToArray();
		int[] seedB = creatures[b].seeds.ToArray();
		Crossover (seedA, seedB);
		generations++;

	}

	private void DeterminePairs(int a, int b){

		//Uses roulette wheel selection to determine which creatures to breed
		//Creatures with a higher fitness should have a higher chance of being picked
		List<float> weights = new List<float>();
		int maxWeight = 0;
		for(int i = 0;  i < creatures.Length; i++){
			weights.Add (creatures[i].fitness);
			maxWeight += (int)(creatures[i].fitness + 0.5f);
		}
		//Sorting the weights makes it easier to find in between values
		weights.Sort();
		float[] sortedWeights = weights.ToArray();

		//Pick first creature
		//Choose a random number and see where it lies in the distribution
		int r = (int)Random.Range (0, maxWeight);
		for(int i = 0; i < sortedWeights.Length; i++){
			if( i < sortedWeights.Length - 2){

				if(r >= weights[i] && r <= weights[i + 1]){
					a = i;
					break;
				}

			}else{
				a = i;
			}
		}

		//Pick second creature
		//Choose a different random number
		r = (int)Random.Range(0, maxWeight);
		for(int i = 0; i < sortedWeights.Length; i++){
			if( i < sortedWeights.Length - 2){
			
				//Make sure there are no repeats
				if(r >= weights[i] && r <= weights[i + 1] && i != a){
					b = i;
					break;
				}
				
			}else if(i != a){
				b = i;

			}else{
				//This is the case where it gets the same int twuce
			}
		}
	
	}

	private void Crossover(int[] seedA, int[] seedB){
		//Randomly crossover seedA and seedB, with a small chance of mutation
		//Keep in mind these two arrays may not be the same size
	}

	public void GetFitness(int i){
		float f = 0f;

		creatures[i].fitness = f;
		//return f;
	}



}
