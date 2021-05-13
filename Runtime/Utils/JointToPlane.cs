using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.iris.common
{
	public class JointToPlane : MonoBehaviour
	{
		public Camera projCam;
		public IRISJoints.Joints joint;
		public Transform planeToProjectOnto;

		// Start is called before the first frame update
		void Start()
		{
			
		}

		// Update is called once per frame
		void Update()
		{
			if (CVInterface.AreDatasAvailable())
			{
				Vector3 pos = CVInterface.GetJointPos3D(joint);
				
				pos = projCam.WorldToViewportPoint(pos);
				pos.z = planeToProjectOnto.position.z;
				pos = projCam.ViewportToWorldPoint(pos);
				transform.position = pos;
			}
		}
	}
}
