using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
	[RequireComponent(typeof(Rigidbody))]
    public class RandomVelocityOnStart : MonoBehaviour
    {
		public Vector3 VelocityLowRandomRange = new Vector3();
		public Vector3 VelocityHighRandomRange = new Vector3();
		public Vector3 AngularLowRandomRange = new Vector3();
		public Vector3 AngularHighRandomRange = new Vector3();

		private Rigidbody MyRigidBody;

		public void Start()
		{
			MyRigidBody = GetComponent<Rigidbody>();
			if (MyRigidBody == null)
				Debug.LogWarning("This component requires a Rigidody");

			MyRigidBody.velocity = new Vector3(Random.Range(VelocityLowRandomRange.x, VelocityHighRandomRange.x), Random.Range(VelocityLowRandomRange.y, VelocityHighRandomRange.y), Random.Range(VelocityLowRandomRange.z, VelocityHighRandomRange.z));
			MyRigidBody.angularVelocity = new Vector3(Random.Range(AngularLowRandomRange.x, AngularHighRandomRange.x), Random.Range(AngularLowRandomRange.y, AngularHighRandomRange.y), Random.Range(AngularLowRandomRange.z, AngularHighRandomRange.z));
		}
	}
}
