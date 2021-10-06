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

		[Range(0.0f, 1.0f)]
		public float Smoothing = 0f;
		private float PreviousValue = 0f;

		public override bool IsValid(VisualEffect component)
		{
			return component.HasFloat(FloatProperty);
		}

		public override void UpdateBinding(VisualEffect component)
		{
			float Val = FXDataProvider.GetFloat(FloatToBind);
			Val = Mathf.Lerp(PreviousValue, Val, 1f-Smoothing);
			component.SetFloat(FloatProperty, Val );

			PreviousValue = Val;
		}			

		public override string ToString()
		{
			return "IRIS: Float Binder";
		}

	}
}
