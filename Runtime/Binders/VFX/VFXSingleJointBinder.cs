using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace com.iris.common
{

	[AddComponentMenu("IRIS/Single Joint Binder")]
	[VFXBinder("IRIS/SingleJointBinder")]
	public class VFXSingleJointBinder : VFXBinderBase
	{
		public IRISJoints.Joints Joint = IRISJoints.Joints.HandLeft;
		public int userIndex = 0;
		[VFXPropertyBinding("System.Vector3"), SerializeField]
		protected ExposedProperty JointPositionProperty = "JointPositionProperty";

		public bool BindRotation = false;
		[VFXPropertyBinding("System.Vector3"), SerializeField]
		protected ExposedProperty JointRotationProperty = "JointRotationProperty";

		public override bool IsValid(VisualEffect component)
		{
			if (!BindRotation)
				return component.HasVector3(JointPositionProperty);
			else
				return component.HasVector3(JointRotationProperty) && component.HasVector3(JointRotationProperty);
		}

		public override void UpdateBinding(VisualEffect component)
		{
			component.SetVector3(JointPositionProperty, FXDataProvider.GetJointPosition(Joint, userIndex));
			if( BindRotation)
				component.SetVector3(JointPositionProperty, FXDataProvider.GetJointRotation(Joint, userIndex));
		}

		public override string ToString()
		{
			return "IRIS: SingleJoint Binder";
		}

	}
}