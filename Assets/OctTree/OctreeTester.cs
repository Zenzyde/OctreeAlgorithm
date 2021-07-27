using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeTester : MonoBehaviour
{
	[SerializeField] float insertionRadius;
	[SerializeField] Vector3 center;
	[SerializeField] int maxNodesInPartition;

	Octree tree;

	List<Vector3> positions = new List<Vector3>();

	void Start()
	{
		tree = new Octree(center, insertionRadius, maxNodesInPartition);
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector3 position = center;
			position += Random.value > .5f ? Vector3.up * Random.Range(0, insertionRadius) : Vector3.down * Random.Range(0, insertionRadius);
			position += Random.value > .5f ? Vector3.right * Random.Range(0, insertionRadius) : Vector3.left * Random.Range(0, insertionRadius);
			position += Random.value > .5f ? Vector3.forward * Random.Range(0, insertionRadius) : Vector3.back * Random.Range(0, insertionRadius);
			tree.InsertPosition(position);
			positions.Add(position);
		}
		if (Input.GetMouseButtonDown(1) && positions.Count > 0)
		{
			Vector3 position = positions[Random.Range(0, positions.Count)];
			if (tree.RemovePosition(position))
			{
				positions.Remove(position);
			}
		}
	}

	void OnDrawGizmos()
	{
		//Gizmos.DrawWireCube(center, Vector3.one * insertionRadius * 2);
		if (tree != null)
			tree.DrawGizmos();
	}
}
