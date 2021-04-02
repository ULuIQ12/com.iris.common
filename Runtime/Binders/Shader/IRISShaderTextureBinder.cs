using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
	[AddComponentMenu("IRIS/Shaders/IRIS Shader Texture Binder")]	
    public class IRISShaderTextureBinder : IRISShaderBinderBase
	{
		public FXDataProvider.MAP_DATA_TYPE TextureToBind;
		public string TexturePropertyName = "_texture";
		public bool ShowWarning = false;

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
			TextureSize.x = map.width;
			TextureSize.y = map.height;

			if (MyRenderer.sharedMaterial.HasProperty(TexturePropertyName))
				MyRenderer.sharedMaterial.SetTexture(TexturePropertyName, map);
			else
				ShowWarning = true;

			if(BindSize)
			{
				if (MyRenderer.sharedMaterial.HasProperty(TextureSizePropertyName))
					MyRenderer.sharedMaterial.SetVector(TextureSizePropertyName, TextureSize);
				else
					ShowWarning = true;

			}
        }


	}
}
