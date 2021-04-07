using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
	[AddComponentMenu("IRIS/Shaders/IRIS Shader float Binder")]
	public class IRISShaderBinderFloat : IRISShaderBinderBase
	{
		public FXDataProvider.FLOAT_DATA_TYPE FloatToBind;
		public string FloatPropertyName = "floatparameter";
		public bool ShowWarning = false;
		void Update()
		{
			if (MyRenderer == null)
				GetRenderer();

			if (MyRenderer == null)
				return;

			float f = FXDataProvider.GetFloat(FloatToBind);

			ShowWarning = false;
			if( !MyRenderer.sharedMaterial.HasProperty(FloatPropertyName))
			{
				ShowWarning = true;
			}
			else
			{
				MyRenderer.sharedMaterial.SetFloat(FloatPropertyName, f);
			}

		}
	}
}
