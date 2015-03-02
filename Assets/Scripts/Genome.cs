using UnityEngine;
using System.Collections.Generic;

public class Genome{
	//A directed graph of all Genes and gene
	private List<Vertex> vertices;
	private List<Link> edges;
	private Vertex rootVertex;

	//public int[] seeds;
	//public List<int> seedValues;

	public Genome(){

		vertices = new List<Vertex>();
		edges= new List<Link>();
	}

	public bool isEmpty(){
		return vertices.Count == 0;
	}

	public bool AddVertex(Vertex v){
		bool added = false;
		if(vertices.Contains(v) == false){
			added = false;
			vertices.Add(v);
		}else{
			added = true;
		}

		return added;
	}

	public bool AddLink(Vertex from, Vertex to){
		if(!vertices.Contains(from)){
			Debug.LogError("vertex from is not in graph");
		}
		if(!vertices.Contains(to)){
			Debug.LogError ("vertex to is not in graph");
		}

		Link l = new Link(from, to);

		if(from.FindLink(to) != null){
			return false;
		}else{
			from.AddLink(l);
			to.AddLink(l);
			edges.Add(l);
			return true;
		}


	}

	public int GetSize(){
		return vertices.Count;
	}
}
