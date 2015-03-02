using UnityEngine;
using System.Collections.Generic;

public class Vertex {
	public List<Link> incomingLinks;
	public List<Link> outgoingLinks;

	public int maxIncomingEdges;
	public int maxOutgoingEdges;

	public int seed;

	public Vertex(){
		this.incomingLinks = new  List<Link>();
		this.outgoingLinks= new List<Link>();
	}

	public virtual void Initialize(){
		maxIncomingEdges = 0;
		maxOutgoingEdges = 0;
	}

	public bool AddLink(Link l){
		if(l.GetFrom() == this)
			outgoingLinks.Add (l);
		else if(l.GetTo() == this)
			incomingLinks.Add (l);
		else
			return false;
		return true;
	}

	public Link FindLink(Link l){
		if(outgoingLinks.Contains(l))
			return l;
		else
			return null;
	}

	public Link FindLink(Vertex dest){
		//Link[] linkArray = outgoingLinks.ToArray();
		foreach( Link l in outgoingLinks){
			if(l.GetTo() == dest){
				return l;
			}
		}
		return null;
	}

}

public class Clock: Vertex {

	public float t;
	public Link[] links;
	public override void Initialize ()
	{
		t = 0;
		maxIncomingEdges = 0;
		maxOutgoingEdges = 4;
	}
	void Start(){
		//links = this.outgoingLinks.ToArray();
		//Link l = this.outgoingLinks.G
	}

	void Update(){
		bool[] linkHasToggled = {false, false, false, false};

		//Does nothing if the array is empty
		if(outgoingLinks.Count > 0){
			t += Time.deltaTime;
		}

		//This approach gives a "time window" for each output to be on.
		//The clock moves fast so that the creatures themselves can move fast.
		if(t >= 0.24f && t <= 0.26f && outgoingLinks.Count >= 1){

			if(outgoingLinks.Count == 4){
				outgoingLinks[3].signal = false;
			}

			Toggle (linkHasToggled[0], outgoingLinks[0].signal);

			if(outgoingLinks.Count == 1){
				t = 0f;
			}
		}

		if(t >= 0.49f && t <= 0.51f && outgoingLinks.Count >= 2){

			Toggle (linkHasToggled[0], outgoingLinks[0].signal);
			Toggle (linkHasToggled[1], outgoingLinks[1].signal);
			if(outgoingLinks.Count == 2){
				t = 0f;
			}
		}

		if(t >= 0.74f && t<= 0.76f && links.Length >= 3){
			Toggle (linkHasToggled[1], outgoingLinks[1].signal);
			Toggle (linkHasToggled[2], outgoingLinks[2].signal);
			if(links.Length == 3){
				t = 0f;
			}
		}

		if(t >= 0.98f && outgoingLinks.Count == 4){
			Toggle (linkHasToggled[2], outgoingLinks[2].signal);
			Toggle (linkHasToggled[3], outgoingLinks[3].signal);
			t = 0f;
		}

	}


	private void Toggle(bool toggled, bool b){
		//Should only toggle once
		if(!toggled){
			if(b == false){
				b = true;
			}else{
				b = false;
			}
			toggled = true;
		}
	}
}

