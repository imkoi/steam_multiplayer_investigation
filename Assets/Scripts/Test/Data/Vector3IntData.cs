using System.Numerics;
using UnityEngine;

namespace Test.Data
{
	public struct Vector3IntData
	{
		public Vector3Int UnityVector => new Vector3Int();
		
		public int serialized;
	}
}