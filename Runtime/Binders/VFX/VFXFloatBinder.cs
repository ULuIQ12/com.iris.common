using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace com.iris.common
{

	[AddComponentMenu("IRIS/Float Binder")]
	[VFXBinder("IRIS/Float")]
	public class VFXFloatBinder : VFXBinderBase
	{

		[VFXPropertyBinding("Float"), SerializeField]
		protected ExposedProperty FloatProperty = "FloatProperty";

		public FXDataProvider.FLOAT_DATA_TYPE FloatToBind = FXDataProvider.FLOAT_DATA_TYPE.AudioBeat;

		public override bool IsValid(VisualEffect component)
		{
			return component.HasFloat(FloatProperty);
		}

		public override void UpdateBinding(VisualEffect component)
		{
			component.SetFloat(FloatProperty, FXDataProvider.GetFloat(FloatToBind));
		}			

		public override string ToString()
		{
			return "IRIS: Float Binder";
		}

	}
}
