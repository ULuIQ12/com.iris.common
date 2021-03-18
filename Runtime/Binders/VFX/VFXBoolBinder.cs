using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace com.iris.common
{

	[AddComponentMenu("IRIS/Bool Binder")]
	[VFXBinder("IRIS/Bool")]
	public class VFXBoolBinder : VFXBinderBase
	{

		[VFXPropertyBinding("Bool"), SerializeField]
		protected ExposedProperty BoolProperty = "BoolProperty";

		public FXDataProvider.BOOL_DATA_TYPE BoolToBind = FXDataProvider.BOOL_DATA_TYPE.HandsAboveElbows;

		public override bool IsValid(VisualEffect component)
		{
			return component.HasBool(BoolProperty);
		}

		public override void UpdateBinding(VisualEffect component)
		{
			component.SetBool(BoolProperty, FXDataProvider.GetBool(BoolToBind));
		}

		public override string ToString()
		{
			return "IRIS: Bool Binder";
		}

	}
}
