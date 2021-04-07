using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;


namespace com.iris.common
{
	[AddComponentMenu("IRIS/Int Binder")]
	[VFXBinder("IRIS/Int")]
	public class VFXIntBinder: VFXBinderBase
	{
		[VFXPropertyBinding("Int"), SerializeField]
		protected ExposedProperty IntProperty = "IntProperty";

		public FXDataProvider.INT_DATA_TYPE IntToBind = FXDataProvider.INT_DATA_TYPE.UserCount;

		public override bool IsValid(VisualEffect component)
		{
			return component.HasFloat(IntProperty);
		}

		public override void UpdateBinding(VisualEffect component)
		{
			component.SetInt(IntProperty, FXDataProvider.GetInt(IntToBind));
		}

		public override string ToString()
		{
			return "IRIS: Int Binder";
		}

	}
}
