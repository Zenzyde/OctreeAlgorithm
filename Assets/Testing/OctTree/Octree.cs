using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octree
{
	public Octree(Vector3 center, float radius, int maxNodesInPartition, int maxDepth = 4)
	{
		octreeRoot.nodePosition = center;
		octreeRoot.nodeRadius = radius;
		octreeRoot.maxNodesInPartition = maxNodesInPartition;
		octreeRoot.currentDepth = maxDepth;
		octreeRoot.maxDepth = maxDepth;
	}

	private OctreeNode octreeRoot = new OctreeNode();

	public GameObject GetObjectAtPartition(Vector3 objPos)
	{
		return octreeRoot.GetObjectAtPartition(objPos);
	}

	public bool InsertPosition(Vector3 position, GameObject obj = null)
	{
		return octreeRoot.InsertPosition(position, obj);
	}

	public bool RemovePosition(Vector3 position)
	{
		return octreeRoot.RemovePosition(position);
	}

	public void DrawGizmos()
	{
		octreeRoot.DrawGizmos();
	}

	public class OctreeNode
	{
		public Vector3 nodePosition;
		public float nodeRadius;
		public OctreeNode[] children = new OctreeNode[8];
		public int maxNodesInPartition;
		public int currentNodesInPartition = 0;
		public int currentDepth;
		public int maxDepth;
		public List<Vector3> positions = new List<Vector3>();
		public List<GameObject> objects = new List<GameObject>();

		private bool hasBeenDivided = false;

		public bool InsertPosition(Vector3 position, GameObject obj = null)
		{
			if (currentDepth == 0)
				return false;

			if (positions.Contains(position))
				return true;

			if (currentNodesInPartition < maxNodesInPartition && !hasBeenDivided)
			{
				currentNodesInPartition++;
				positions.Add(position);
				if (obj != null)
					objects.Add(obj);
				return true;
			}

			if (!hasBeenDivided)
			{
				SubDivide();
				PassDownObjects();
				currentNodesInPartition = 0;
				hasBeenDivided = true;
				return true;
			}
			else
			{
				for (int i = 0; i < children.Length; i++)
				{
					if (children[i].IsInsideBoundingBox(position))
					{
						if (children[i].InsertPosition(position))
							return true;
					}
				}
			}

			return false;
		}

		public bool RemovePosition(Vector3 position)
		{
			if (!hasBeenDivided)
			{
				if (positions.Contains(position))
				{
					currentNodesInPartition--;
					positions.Remove(position);
					return true;
				}
			}
			else
			{
				int activeChildCount = 8;
				for (int i = 0; i < children.Length; i++)
				{
					if (children[i].GetNodesInPartition() == 0)
						activeChildCount--;
					if (children[i].IsInsideBoundingBox(position))
					{
						if (children[i].RemovePosition(position))
						{
							return true;
						}
					}
				}
				if (activeChildCount == 0)
				{
					for (int i = 0; i < children.Length; i++)
					{
						children[i] = null;
					}
					hasBeenDivided = false;
				}
			}

			return false;
		}

		public void DrawGizmos()
		{
			if (currentDepth == maxDepth)
			{
				Gizmos.color = Color.white;
				for (int i = 0; i < positions.Count; i++)
				{
					Gizmos.DrawWireSphere(positions[i], .8f);
				}
				Gizmos.DrawWireCube(nodePosition, Vector3.one * nodeRadius * 2);
			}
			else
			{
				switch (currentDepth)
				{
					case 3:
						Gizmos.color = Color.magenta;
						break;
					case 2:
						Gizmos.color = Color.cyan;
						break;
					case 1:
						Gizmos.color = Color.green;
						break;
					case 0:
						Gizmos.color = Color.red;
						break;
				}
				for (int i = 0; i < positions.Count; i++)
				{
					Gizmos.DrawWireSphere(positions[i], .8f);
				}
				Gizmos.DrawWireCube(nodePosition, Vector3.one * nodeRadius * 1.9f);
			}
			for (int i = 0; i < children.Length; i++)
			{
				if (children[i] != null)
					children[i].DrawGizmos();
			}
		}

		private void SubDivide(int subDivideIndex = -1)
		{
			float newRadius = nodeRadius / 2f;

			children[0] = new OctreeNode();
			children[0].nodePosition = nodePosition + Vector3.up * newRadius + Vector3.right * newRadius + Vector3.forward * newRadius;

			children[1] = new OctreeNode();
			children[1].nodePosition = nodePosition + Vector3.up * newRadius + Vector3.right * newRadius - Vector3.forward * newRadius;

			children[2] = new OctreeNode();
			children[2].nodePosition = nodePosition + Vector3.up * newRadius - Vector3.right * newRadius + Vector3.forward * newRadius;

			children[3] = new OctreeNode();
			children[3].nodePosition = nodePosition + Vector3.up * newRadius - Vector3.right * newRadius - Vector3.forward * newRadius;

			children[4] = new OctreeNode();
			children[4].nodePosition = nodePosition - Vector3.up * newRadius + Vector3.right * newRadius + Vector3.forward * newRadius;

			children[5] = new OctreeNode();
			children[5].nodePosition = nodePosition - Vector3.up * newRadius + Vector3.right * newRadius - Vector3.forward * newRadius;

			children[6] = new OctreeNode();
			children[6].nodePosition = nodePosition - Vector3.up * newRadius - Vector3.right * newRadius + Vector3.forward * newRadius;

			children[7] = new OctreeNode();
			children[7].nodePosition = nodePosition - Vector3.up * newRadius - Vector3.right * newRadius - Vector3.forward * newRadius;

			for (int i = 0; i < 8; i++)
			{
				children[i].nodeRadius = nodeRadius / 2;
				children[i].maxNodesInPartition = maxNodesInPartition;
				children[i].currentDepth = currentDepth - 1;
				children[i].maxDepth = maxDepth;
			}
		}

		private bool IsInsideBoundingBox(Vector3 position)
		{
			return (position.y <= (nodePosition + Vector3.up * nodeRadius).y) && (position.y >= (nodePosition - Vector3.up * nodeRadius).y) &&
			(position.x <= (nodePosition + Vector3.right * nodeRadius).x) && (position.x >= (nodePosition - Vector3.right * nodeRadius).x) &&
			(position.z <= (nodePosition + Vector3.forward * nodeRadius).z) && (position.z >= (nodePosition - Vector3.forward * nodeRadius).z);
		}

		private void PassDownObjects()
		{
			for (int i = 0; i < children.Length; i++)
			{
				for (int j = positions.Count - 1; j >= 0; j--)
				{
					if (children[i].IsInsideBoundingBox(positions[j]))
					{
						if (children[i].InsertPosition(positions[j]))
							positions.RemoveAt(j);
					}
				}
			}
		}

		private int GetNodesInPartition() => positions.Count;

		public GameObject GetObjectAtPartition(Vector3 objPos)
		{
			if (children.Length == 0)
			{
				if (IsInsideBoundingBox(objPos))
				{
					for (int i = 0; i < objects.Count; i++)
					{
						if (objects[i].transform.position == objPos)
							return objects[i];
					}
				}
			}
			else
			{
				for (int i = 0; i < children.Length; i++)
				{
					if (children[i].IsInsideBoundingBox(objPos))
						return children[i].GetObjectAtPartition(objPos);
				}
			}
			return null;
		}

		public List<GameObject> GetObjectsAtPartition(Vector3 partPos)
		{
			if (children.Length == 0)
			{
				if (IsInsideBoundingBox(partPos))
				{
					return objects;
				}
			}
			else
			{
				for (int i = 0; i < children.Length; i++)
				{
					if (children[i].IsInsideBoundingBox(partPos))
						return children[i].GetObjectsAtPartition(partPos);
				}
			}
			return null;
		}
	}
}
