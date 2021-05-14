using UnityEngine;

namespace com.iris.common
{
	public class BindGOPositionToJoint : MonoBehaviour
	{
		public bool bindX = true;
		public bool bindY = true;
		public bool bindZ = true;

		public bool ProjectToPlane = false;
		public Camera ProjectionCamera;
		public Transform ProjectionPlane;


		public IRISJoints.Joints Joint;
		public int userIndex = 0;

		private Vector3 targetPos = new Vector3();

		public void Update()
		{
			if (CVInterface.AreDatasAvailable())
			{
				Vector3 pos = CVInterface.GetJointPos3D(Joint, userIndex);

				if(ProjectionCamera != null && ProjectToPlane)
				{
					pos = ProjectionCamera.WorldToViewportPoint(pos);
					pos.z = ProjectionPlane.position.z;
					pos = ProjectionCamera.ViewportToWorldPoint(pos);
				}

				if (bindX)
					targetPos.x = pos.x;
				else
					targetPos.x = transform.position.x;

				if (bindY)
					targetPos.y = pos.y;
				else
					targetPos.y = transform.position.y;

				if (bindZ)
					targetPos.z = pos.z;
				else
					targetPos.z = transform.position.z;

				transform.position = targetPos;
			}
		}
	}
}
