using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Creature {
    //The Creature class is used to keep track of the Creature during combat and differentiate between two creatures
	public int score;
    public string name;
	public int number;
	public int HP;
	public List<GameObject> parts;

	public Creature(int number, int HP){
		this.number = number;
		this.HP = HP;
		this.score = 1;
		this.parts = new List<GameObject>();
	}

}

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
	public class Genome {
		public int HP;
        public string firstName;
        public string lastName;
		public int fitness;
		public int nSegments;
		public List<Color> colors;
		public List<int> components;
        public List<bool> eyeballs;
		public List<float> timerValues;
		public List<float> speeds;

		//Needs to remember number of joints per component,and what the joints are connected to
		public List<int> jointsPerComponent;
		public List<List<int>> connections;
        public List<List<Vector2>> jointSources;
        public List<List<Vector2>> jointDestinations;

		public Genome(){
            this.firstName = "";
            this.lastName = "";
			this.nSegments = 0;
			this.colors = new List<Color>();
			this.components = new List<int>();
			this.connections = new List<List<int>>();
			this.jointsPerComponent = new List<int>();
            this.jointSources = new List<List<Vector2>>();
            this.jointDestinations = new List<List<Vector2>>();
            this.eyeballs = new List<bool>();
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
	public GameObject weapon;
    public GameObject eyeball;

	public float timeLimit;
	public static bool creaturesInScene;
	public int nComponents;
	public int population;
	private bool creatureDead = false;
	private Creature creature1, creature2;
	private Vector3 spawnPoint;
	private Genome[] genomes;
    //private Genome testCreature;
    private const int START_SCORE = 10;
    public float segmentXDim;
    public float segmentYDim;
	private int generationCounter;
	private float timer;
	private bool isFirstGeneration;
	private int populationCounter;

    public Text generation, creature1Score, creature2Score, creatureNumbers;

	// Use this for initialization
	void Start () {
		genomes = new Genome[population];

		for(int i = 0; i < population; i++){
			genomes[i] = new Genome();
		}

		//testCreature = new Genome();
		generationCounter = 1;
		Debug.Log ("Creating Generation " + generationCounter);
        generation.text = "Generation " + generationCounter;
        
        isFirstGeneration = true;
		populationCounter = 0;

		creaturesInScene = false;
		spawnPoint = GameObject.Find ("SpawnPoint").transform.position;
		timer = 0;
	}


	void FixedUpdate () {
		
		timer += Time.fixedDeltaTime;
        creatureNumbers.text = "Creature " + (populationCounter) + " vs " + "Creature " + (populationCounter + 1);

        if (creature1 != null)
        {
            creature1Score.text = creature1.name+ ": " + (creature1.score);
        }
        else
        {
            creature1Score.text = "DEAD";
        }

        if(creature2 != null)
        {
            creature2Score.text = creature2.name +  ": " + (creature2.score);
        }
        else
        {
            creature2Score.text = "DEAD";
        }
    

		if(timer <= 0.1f && !creaturesInScene){
			creatureDead = false;
			//Create creatures individually instead of all at once.
			creature1 = CreateCreature(genomes[populationCounter], 1);
			creature2 = CreateCreature(genomes[++populationCounter], 2);
           
			creaturesInScene = true;
		}

		if((creature1.HP <= 0 || creature2.HP <= 0) && !creatureDead){
			timer = timeLimit - timeLimit/5;
			
			creatureDead = true;

			if(creature1.HP <= 0){
                Debug.Log("Creature " + creature2.number + " killed creature " + creature1.number + "!");
				creature2.score += 30;
				for(int i = 0; i < creature1.parts.Count; i++){
					Destroy(creature1.parts[i]);
				}
			}

			
			if(creature2.HP <= 0){
                Debug.Log("Creature " + creature2.number + " killed creature " + creature1.number + "!");
                creature1.score += 30;
				for(int i = 0; i < creature2.parts.Count; i++){
					Destroy(creature2.parts[i]);
				}
			}
		}

		if(timer >= timeLimit && creaturesInScene){

			//Estimate the fitness
			int fitness1 = creature1.score;
			int fitness2 = creature2.score;

			genomes[populationCounter - 1].fitness = fitness1;
			genomes[populationCounter].fitness = fitness2;
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
                generation.text = "Generation " + generationCounter;
				populationCounter = 0;
				isFirstGeneration = false;
				CreateNextGeneration();
			}

			creaturesInScene = false;

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
        //Pass the torch to the next generation and get rid of the parent genomes
		genomes = newGeneration;
	}

	private int[] Selection(){
		int a;
		int b;
		//Use roulette wheel selection to determine value of a and b
		
		//Create a weighted distribution
		List<int> distribution = new List<int>();

		for(int i = 0; i < population; i++){
			for(int j = 0; j< genomes[i].fitness + 1; j++){
				distribution.Add(i);
			}
		}

		//Choose a random value out of the weighted distribution
		a = distribution[Random.Range (0, distribution.Count)];

		//Make sure that the chosen value cannot be chosen again
		distribution.RemoveAll (item => item == a);

		//Pick a different random value out of the weighted distribution
		try{
			b = distribution[Random.Range(0, distribution.Count)];
		}catch(System.ArgumentOutOfRangeException oor ){
			Debug.Log ("Argument out of range B Distribution.Count: " + distribution.Count);
			b = 0;
		}

		return new int[] {a, b};
	}


	private Genome Crossover(Genome a, Genome b){
		//g will be a container that takes in elements of a and b
		Genome g = new Genome();

        if (FlipCoin())
        {
            g.HP = a.HP;
            g.lastName = a.lastName;
        }
        else
        {
            g.HP = b.HP;
            g.lastName = b.lastName;
        }

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
		//Copy all segment values. 
		to.colors.Add (from.colors[index]);
		to.components.Add (from.components[index]);
        to.eyeballs.Add(from.eyeballs[index]);

		to.connections.Add (from.connections[index]);
        to.jointsPerComponent.Add (from.jointsPerComponent[index]);
        to.jointDestinations.Add(from.jointDestinations[index]);
        to.jointSources.Add(from.jointSources[index]);

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
			Color mutationColor = new Color(Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range(0f, 1f), Random.Range(0.1f, 1f));

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
                g.jointDestinations.Add(new List<Vector2>());
                g.jointSources.Add(new List<Vector2>());

                if (RollD(g.nSegments) < g.nSegments - 2)
                {
                    g.eyeballs.Add(true);
                }
                else
                {
                    g.eyeballs.Add(false);
                }

				SmartRandom connectionSelector = new SmartRandom();


				for(int i = 0; i < g.jointsPerComponent[g.nSegments - 1]; i++){
					g.connections[g.nSegments - 1].Add (connectionSelector.Range (0, g.nSegments - 1));

                    g.jointDestinations[g.nSegments - 1].Add(new Vector2(RandomPointInSquare().x * segmentXDim, 
                        RandomPointInSquare().y * segmentYDim));

                    g.jointSources[g.nSegments - 1].Add(new Vector2(RandomPointInSquare().x * segmentXDim,
                       RandomPointInSquare().y * segmentYDim));
                }

				g.speeds.Add (Random.Range (1f, 4f));
				g.timerValues.Add (Random.Range (1f, 4f));
				
			}

		}
		return g;
	}

	private Creature CreateCreature(Genome g, int creatureNum){

        int HP;
        if (isFirstGeneration)
        {
            HP = Random.Range(20, 70);
            g.HP = HP;
        }
        else
        {
            HP = g.HP;
        }

		Creature creature = new Creature(creatureNum, HP);

        //Name the creature
        g.firstName = MakeRandomName(3, 6);
        string creatureName = g.firstName;

        if (isFirstGeneration)
        {
            g.lastName = MakeRandomName(3, 6);

        }

        creatureName += " " + g.lastName;
        creature.name = creatureName;

		//The creature and the genome are created together on the first generation
		List<GameObject> segments = new List<GameObject>();
		//Set the number of segments. Should be random if not set yet.
        
		if(isFirstGeneration ){
            g.nSegments = Random.Range (1, 5);
		}

		//Creatures are now a solid color. This allows us to see a creature's parents when they cross breed
		Color color = Color.white;

        //Setting minimum for alpha channel value to prevent completely invisible creatures
		if(isFirstGeneration){
			color =  new Color(Random.Range (0f, 1f),Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range(0.1f, 1f));
		}
		//Whenever there is more than one segment, add a joint between the two segments
		//A single segment can have more than one joint
		//No more than one joint should exist between two different segments
		for(int i = 0; i < g.nSegments; i++){
			//Range should be equal to the number of components
			//Adjust for off by 1 error

			int state;

			if(isFirstGeneration){

                //This way we prevent getting a weapon by itself
                if (g.components.Count == 0)
                {
                    state = Random.Range(0, nComponents -1);
                }
                else
                {
                    state = Random.Range(0, nComponents);
                }

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

			GameObject o = segment;


			//Random vector to prevent parts from sticking together due to all spawning in the same location
			Vector3 randomVect = new Vector3(Random.Range(0.01f, 0.1f), Random.Range(0.01f, 0.1f), 0);

			if(creatureNum == 1){
				randomVect.x -= 5;
			}else{
				randomVect.x += 5;
			}

			switch (state){

			case 0:
				//create both the gameObject and the joints here!
				o = Instantiate(segment, spawnPoint + randomVect, Quaternion.identity) as GameObject;
				o.GetComponent<SpriteRenderer>().color = color;
				segments.Add(o);
				o.GetComponent<Body>().creature = creature;
				o.name = "Segment" + i;
				creature.parts.Add (o);
				//Debug.Log ("State: " + state);
				//o.tag = "unconnected";
				break;

			case 1:
				o = Instantiate(wiggleSegment, spawnPoint + randomVect, Quaternion.identity) as GameObject;
				o.GetComponent<SpriteRenderer>().color = color;
				o.GetComponent<Rotator>().timeLimit = timerValue;
				o.GetComponent<Rotator>().rotationSpeed = speed;
				o.GetComponent<Body>().creature = creature;
				segments.Add(o);
				creature.parts.Add (o);
				o.name = "Segment" + i;
				//Debug.Log ("State: " + state);
				break;

			case 2:
				o = Instantiate(stretchSegment, spawnPoint + randomVect, Quaternion.identity) as GameObject;
				o.GetComponent<SpriteRenderer>().color = color;
				o.GetComponent<Stretcher>().timeLimit = timerValue;
				o.GetComponent<Stretcher>().speed = speed;
				o.GetComponent<Body>().creature = creature;
				creature.parts.Add (o);
				segments.Add(o);
				o.name = "Segment" + i;
				//Debug.Log ("State: " + state);
				break;

			case 3:
				o = Instantiate(slideSegment, spawnPoint + randomVect, Quaternion.identity) as GameObject;
				o.GetComponent<SpriteRenderer>().color = color;
				o.GetComponent<Slider>().timeLimit = timerValue;
				o.GetComponent<Slider>().speed = speed;
				o.GetComponent<Body>().creature = creature;
				creature.parts.Add (o);
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
				o.GetComponent<Body>().creature = creature;
				creature.parts.Add (o);
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
				o.GetComponent<Body>().creature = creature;
				creature.parts.Add (o);
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
				o.GetComponent<Body>().creature = creature;
				o.GetComponent<SpriteRenderer>().color = color;
				segments.Add(o);
				creature.parts.Add (o);
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
				o.GetComponent<Body>().creature = creature;
				segments.Add(o);
				creature.parts.Add (o);
				o.name = "Segment" + i;
				//Debug.Log ("State: " + state);
				break;

			case 8:
				o = Instantiate(weapon, spawnPoint + randomVect, Quaternion.identity) as GameObject;
				o.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, color.a);
				creature.parts.Add (o);
				o.GetComponent<Body>().creature = creature;
				o.GetComponent<Weapon>().C = creature;
				segments.Add(o);
				o.name = "Weapon";
				break;

			default:
				Debug.Log ("Error: nComponents negative or greater than the actual number of components");
				break;

			}

            //Add eyeballs!
            bool addEyeball = false;

            if (isFirstGeneration)
            {
                if(RollD(i) == i)
                {
                    addEyeball = true;
                }

                g.eyeballs.Add(addEyeball);
            }
            else
            {
                addEyeball = g.eyeballs[i];
            }

            if (addEyeball)
            {
                GameObject creatureEye = Instantiate(eyeball, spawnPoint + randomVect, Quaternion.identity) as GameObject;
                DistanceJoint2D j = creatureEye.AddComponent<DistanceJoint2D>();
                j.connectedBody = segments[i].GetComponent<Rigidbody2D>();

                creatureEye.GetComponent<DistanceJoint2D>().distance = 0;
                creature.parts.Add(creatureEye);
                //eye.transform.SetParent(o.transform);
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
            List<Vector2> jointSourceList = new List<Vector2>();
            List<Vector2> jointDestinationList = new List<Vector2>();

			if(isFirstGeneration){
				g.connections.Add (connectionList);
                g.jointSources.Add(jointSourceList);
                g.jointDestinations.Add(jointDestinationList);
			}else{
				
                try
                {
                    connectionList = g.connections[i];
                    jointSourceList = g.jointSources[i];
                    jointDestinationList = g.jointDestinations[i];
                }
                catch(System.ArgumentOutOfRangeException e)
                {
                    Debug.Log("Exception!");
                    Debug.Log("Joint source list count: " + jointSourceList.Count);
                    Debug.Log("I: " + i);
                }
                
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
                    Vector2 sourceVect;
                    Vector2 destinationVect;

					if(isFirstGeneration){
						index = jointSelector.Range (0, segments.Count - 1);
						g.connections[i].Add (index);

                        Vector2 s = RandomPointInSquare(), d = RandomPointInSquare();
                        sourceVect = new Vector2(s.x * segmentXDim/2, s.y * segmentYDim/2);
                        g.jointSources[i].Add(sourceVect);

                        destinationVect = new Vector2(d.x * segmentXDim/2, d.y * segmentYDim/2);
                        g.jointDestinations[i].Add(destinationVect);
						//Debug.Log ("Index: " + index + " SegmentNumber: " + i + " Segments.Count: " + segments.Count);
					}else{
						index = g.connections[i][j];
                        sourceVect = g.jointSources[i][j];
                        destinationVect = g.jointDestinations[i][j];
					}


					

                    if (i > 0)
                    {
                        segments[i].GetComponent<SpringJoint2D>().distance = 5;
                        segments[i].GetComponent<SpringJoint2D>().dampingRatio = 0.5f;
                    
                        segments[i].GetComponent<SpringJoint2D>().anchor = sourceVect;
                        segmentJoints[j].GetComponent<SpringJoint2D>().connectedAnchor = destinationVect;
                       
                    }

                    springJoint.connectedBody = segments[index].GetComponent<Rigidbody2D>();
                }
			}
		}

		return creature;

	}

	//Helper functions
	
	private int Round(float f){
		return (int)(f + 0.5f);
	}

    private string MakeRandomName(int minNameLength, int maxNameLength)
    {
        string s = "";
        int nameLength = Random.Range(minNameLength, maxNameLength + 1);
      

        for(int i = 0; i < nameLength; i++)
        {
            if (i % 2 == 0)
            {
                s += MakeRandomConsonant();
            }
            else
            {
                s += MakeRandomVowel();
            }

            if (i == 0)
            {
                string cap = "";
                cap += char.ToUpper(s[0]);
                s = cap;
            }
            
        }

        return s;
    }

    private char MakeRandomVowel()
    {
       
        string vowels = "aeiouy";
        return vowels[Random.Range(0, 6)];
    }
    private char MakeRandomConsonant()
    {
        string consonants = "bcdfghjklmnpqrstvwxz";
        return consonants[Random.Range(0, 20)];
    }


    private Vector2 RandomPointInSquare()
    {
        Vector2 v = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        return v;
    }

    private Vector2 RandomPointInCircle()
    {
        Vector2 v = RandomPointInSquare();
        v.Normalize();
        return v;
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
