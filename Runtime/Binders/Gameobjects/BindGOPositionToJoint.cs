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
		private Vector3 previousValue = new Vector3();

		public void Update()
		{
			if (CVInterface.AreDatasAvailable())
			{
				Vector3 pos = CVInterface.GetJointPos3D(Joint, userIndex);
				if( pos == Vector3.zero )
				{
					pos = previousValue;
					if (ProjectionCamera != null && ProjectToPlane && ProjectionPlane != null)
					{
						pos.z = ProjectionPlane.transform.position.z;
					}
					transform.position = Vector3.Lerp(transform.position, targetPos, .5f);
					previousValue = transform.position;

					return;
				}
				
				if(ProjectionCamera != null && ProjectToPlane && ProjectionPlane != null)
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

				transform.position = Vector3.Lerp(transform.position,  targetPos, .5f);

				previousValue = transform.position;
			}
			else
			{
				Vector3 pos = Vector3.zero;
				if (ProjectionCamera != null && ProjectToPlane && ProjectionPlane != null)
				{
					pos.z = ProjectionPlane.transform.position.z;
				}

				transform.position = Vector3.Lerp(transform.position, targetPos, .5f);
				previousValue = transform.position;
			}
		}
	}
}
