using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace com.iris.common
{

	[AddComponentMenu("IRIS/Single Joint2D Binder")]
	[VFXBinder("IRIS/SingleJoint2DBinder")]
	public class VFXSingleJoint2DBinder : VFXBinderBase
	{
		public IRISJoints.Joints2D Joint = IRISJoints.Joints2D.Head;
		public int userIndex = 0;
		[VFXPropertyBinding("System.Vector2"), SerializeField]
		protected ExposedProperty JointPositionProperty = "JointPositionProperty";

		public override bool IsValid(VisualEffect component)
		{
			return component.HasVector2(JointPositionProperty);
		}

		public override void UpdateBinding(VisualEffect component)
		{
			Vector2 pos = FXDataProvider.GetJoint2DPosition(Joint, userIndex);
			component.SetVector2(JointPositionProperty, pos);

		}

		public override string ToString()
		{
			return "IRIS: SingleJoint2D Binder";
		}

	}
}