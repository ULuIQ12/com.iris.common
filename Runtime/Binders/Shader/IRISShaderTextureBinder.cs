using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
	[AddComponentMenu("IRIS/Shaders/IRIS Shader Texture Binder")]	
    public class IRISShaderTextureBinder : IRISShaderBinderBase
	{
		public FXDataProvider.MAP_DATA_TYPE TextureToBind;
		public string TexturePropertyName = "TexturePropertyName";
		public bool ShowWarning = false;

		public bool BindScale = false;
		public string TextureScalePropertyName = "TextureScalePropertyName";
		public Vector2 TextureScale = Vector2.one;

		public bool BindSize = false;
		public string TextureSizePropertyName = "_textureSize";
		public Vector2 TextureSize = Vector2.zero;

		

        void Update()
        {
			if (MyRenderer == null)
				GetRenderer();

			if (MyRenderer == null)
				return;

			ShowWarning = false;

			Texture map = FXDataProvider.GetMap(TextureToBind); 
			if (map == null)
				return;

			TextureScale = FXDataProvider.GetMapScale(TextureToBind);


			TextureSize.x = map.width;
			TextureSize.y = map.height;

			if (MyRenderer.sharedMaterial.HasProperty(TexturePropertyName))
				MyRenderer.sharedMaterial.SetTexture(TexturePropertyName, map);
			else
				ShowWarning = true;

			if (BindScale)
			{
				if (MyRenderer.sharedMaterial.HasProperty(TextureScalePropertyName))
					MyRenderer.sharedMaterial.SetVector(TextureScalePropertyName, TextureScale);
				else
					ShowWarning = true;

			}

			if (BindSize)
			{
				if (MyRenderer.sharedMaterial.HasProperty(TextureSizePropertyName))
					MyRenderer.sharedMaterial.SetVector(TextureSizePropertyName, TextureSize);
				else
					ShowWarning = true;

			}
        }


	}
}
