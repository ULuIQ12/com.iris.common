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

		public bool BindSize = false;

		[VFXPropertyBinding("int"), SerializeField]
		protected ExposedProperty TextureWidthProperty = "TextureWidth";

		[VFXPropertyBinding("int"), SerializeField]
		protected ExposedProperty TextureHeightProperty = "TextureHeight";


		public FXDataProvider.MAP_DATA_TYPE TextureToBind = FXDataProvider.MAP_DATA_TYPE.ColorMap;

		public override bool IsValid(VisualEffect component)
		{
			if( BindSize )
			{
				return component.HasTexture(TextureProperty) && component.HasInt(TextureWidthProperty) && component.HasInt(TextureHeightProperty);
			}
			return component.HasTexture(TextureProperty);
		}

		public override void UpdateBinding(VisualEffect component)
		{
			component.SetTexture(TextureProperty, FXDataProvider.GetMap(TextureToBind));
			if( BindSize )
			{
				Vector2 size = FXDataProvider.GetMapSize(TextureToBind);
				component.SetInt(TextureWidthProperty, Mathf.FloorToInt(size.x));
				component.SetInt(TextureHeightProperty, Mathf.FloorToInt(size.y));
			}
		}

		public override string ToString()
		{
			return "IRIS: TextureMap Binder";
		}

	}
}