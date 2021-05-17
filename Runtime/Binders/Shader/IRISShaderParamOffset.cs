using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
	[AddComponentMenu("IRIS/Shaders/IRIS Shader Param Offset Binder")]
	public class IRISShaderParamOffset : IRISShaderBinderBase
	{
		public FXDataProvider.FLOAT_DATA_TYPE FloatToBind;
		public string FloatPropertyName = "floatparameter";

		public Vector4 RemapParams = new Vector4(0f, 1f, 0f, 1f);

		public bool ShowWarning = false;
		private float localValue = 0f;
		void Update()
		{
			if (MyRenderer == null)
				GetRenderer();

			if (MyRenderer == null)
				return;

			float f = FXDataProvider.GetFloat(FloatToBind);

			float remapped = Remap(RemapParams.x, RemapParams.y, RemapParams.z, RemapParams.w, f);
			localValue += remapped;

			ShowWarning = false;
			if (!MyRenderer.sharedMaterial.HasProperty(FloatPropertyName))
			{
				ShowWarning = true;
			}
			else
			{
				MyRenderer.sharedMaterial.SetFloat(FloatPropertyName, localValue);
			}

		}

		private float Remap(float InputLow, float InputHigh, float OutputLow, float OutputHigh, float value)
		{
			value = Mathf.InverseLerp(InputLow, InputHigh, value);
			value = Mathf.Lerp(OutputLow, OutputHigh, value);

			return value;
		}
	}
}
