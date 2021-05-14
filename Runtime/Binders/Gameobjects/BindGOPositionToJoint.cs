using UnityEngine;

namespace com.iris.common
{
	public class BindGOPositionToJoint : MonoBehaviour
	{
		public bool bindX = true;
		public bool bindY = true;
		public bool bindZ = true;

		public IRISJoints.Joints Joint;
		public int userIndex = 0;

		private Vector3 targetPos = new Vector3();

		public void Update()
		{
			if (CVInterface.AreDatasAvailable())
			{
				Vector3 pos = CVInterface.GetJointPos3D(Joint, userIndex);
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
