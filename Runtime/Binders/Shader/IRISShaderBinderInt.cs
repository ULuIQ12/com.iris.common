using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
	[AddComponentMenu("IRIS/Shaders/IRIS Shader int Binder")]
	public class IRISShaderBinderInt : IRISShaderBinderBase
	{
		public FXDataProvider.INT_DATA_TYPE IntToBind;
		public string IntPropertyName = "intparameter";
		public bool ShowWarning = false;
		void Update()
		{
			if (MyRenderer == null)
				GetRenderer();

			if (MyRenderer == null)
				return;

			float f = (float)FXDataProvider.GetInt(IntToBind);

			ShowWarning = false;
			if (!MyRenderer.sharedMaterial.HasProperty(IntPropertyName))
			{
				ShowWarning = true;
			}
			else
			{
				MyRenderer.sharedMaterial.SetFloat(IntPropertyName, f);
			}

		}
	}
}
