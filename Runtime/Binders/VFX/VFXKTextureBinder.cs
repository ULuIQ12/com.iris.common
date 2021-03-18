using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace com.iris.common
{

	[AddComponentMenu("IRIS/TextureMap Binder")]
	[VFXBinder("IRIS/TextureMap")]
	public class VFXTextureBinder : VFXBinderBase
	{

		[VFXPropertyBinding("Texture"), SerializeField]
		protected ExposedProperty TextureProperty = "Texture";

		public FXDataProvider.MAP_DATA_TYPE TextureToBind = FXDataProvider.MAP_DATA_TYPE.ColorMap;

		public override bool IsValid(VisualEffect component)
		{
			return component.HasTexture(TextureProperty);
		}

		public override void UpdateBinding(VisualEffect component)
		{
			component.SetTexture(TextureProperty, FXDataProvider.GetMap(TextureToBind));
		}

		public override string ToString()
		{
			return "IRIS: TextureMap Binder";
		}

	}
}