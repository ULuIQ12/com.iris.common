using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.iris.common
{
	public class JointToPlane2D : MonoBehaviour
	{
		public Camera projCam;
		public IRISJoints.Joints2D joint;
		public float HandProjectionPlaneDist = 5.0f;

		// Start is called before the first frame update
		void Start()
		{
			
		}

		// Update is called once per frame
		void Update()
		{
			if (CVInterface.AreDatasAvailable())
			{
				Vector3 pos = FXDataProvider.GetJoint2DPosition(joint);
				pos.z = HandProjectionPlaneDist;
				transform.position = projCam.ViewportToWorldPoint(pos);

			}
		}
	}
}
