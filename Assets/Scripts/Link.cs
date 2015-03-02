using UnityEngine;
using System.Collections;

public class Link {
	public Vertex from;
	public Vertex to;
	public bool signal;

	public Link(Vertex from, Vertex to){
		this.from = from;
		this.to = to;
	}

	public Vertex GetFrom(){
		return this.from;
	}
	public Vertex GetTo(){
		return this.to;
	}
}

