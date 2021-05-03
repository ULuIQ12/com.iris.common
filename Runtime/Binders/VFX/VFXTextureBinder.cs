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
			bool valid = true;
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
			if (component != null)
			{
				Texture t = FXDataProvider.GetMap(TextureToBind);
				if (t == null)
					return;

				component.SetTexture(TextureProperty, t);

				if (BindSize)
				{
					_Size = FXDataProvider.GetMapSize(TextureToBind);
					if (_Size != null)
						component.SetVector2(TextureSizeProperty, _Size);
					else
						component.SetVector2(TextureSizeProperty, Vector2.one);

				}

				if (BindScale)
				{
					_Scale = FXDataProvider.GetMapScale(TextureToBind);
					if (_Scale != null)
						component.SetVector2(TextureScaleProperty, _Scale);
					else
						component.SetVector2(TextureScaleProperty, Vector2.one);
				}
			}
		}

		public override string ToString()
		{
			return "IRIS: TextureMap Binder";
		}

	}
}