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

		public bool BindScale = false;
		[VFXPropertyBinding("Vector2"), SerializeField]
		protected ExposedProperty TextureScaleProperty = "TextureScale(Vec2)";

		public bool BindSize = false;

		[VFXPropertyBinding("Vector2"), SerializeField]
		protected ExposedProperty TextureSizeProperty = "TextureSize(Vec2)";


		public FXDataProvider.MAP_DATA_TYPE TextureToBind = FXDataProvider.MAP_DATA_TYPE.ColorMap;

		private Vector2 _Size = new Vector2();
		private Vector2 _Scale = new Vector2();
		
		public override bool IsValid(VisualEffect component)
		{
			bool valid = false;
			if (!component.HasTexture(TextureProperty))
				valid = false;

			if( BindSize )
			{
				if (!component.HasVector2(TextureSizeProperty))
					valid = false;
			}

			if (BindScale)
			{
				if (!component.HasVector2(TextureScaleProperty))
					valid = false;
			}
			return valid;
		}

		public override void UpdateBinding(VisualEffect component)
		{
			component.SetTexture(TextureProperty, FXDataProvider.GetMap(TextureToBind));
			if( BindSize )
			{
				Vector2 size = FXDataProvider.GetMapSize(TextureToBind);
				component.SetVector2(TextureSizeProperty, _Size);
				component.SetVector2(TextureScaleProperty, _Scale);
			}
		}

		public override string ToString()
		{
			return "IRIS: TextureMap Binder";
		}

	}
}