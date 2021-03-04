using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace com.iris.common
{

	[AddComponentMenu("IRIS/Color Data Binder")]
	[VFXBinder("IRIS/Color")]
	public class VFXColorBinder: VFXBinderBase
    {
		
		[VFXPropertyBinding("Color"), SerializeField]
		protected ExposedProperty ColorProperty = "Color";

		public FXDataProvider.COLOR_TYPE ColorToBind = FXDataProvider.COLOR_TYPE.SystemColor1;

		public override bool IsValid(VisualEffect component)
		{
			return component.HasVector4(ColorProperty);
		}

		public override void UpdateBinding(VisualEffect component)
		{
			component.SetVector4(ColorProperty, FXDataProvider.GetColor(ColorToBind));
		}

		public override string ToString()
		{
			return "IRIS: Color Binder";
		}

	}
}
