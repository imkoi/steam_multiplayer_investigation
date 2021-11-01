using UnityEngine;

namespace Test.Data
{
	public struct Vector3FixedData
	{
		public Vector3 UnityVector3 => new Vector3(x.Value, y.Value, z.Value);
		
		public FloatFixedData x;
		public FloatFixedData y;
		public FloatFixedData z;
	}
}