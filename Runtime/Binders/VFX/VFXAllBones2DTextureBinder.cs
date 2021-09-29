using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace com.iris.common
{

	[AddComponentMenu("IRIS/AllBonesTexture2D Binder")]
	[VFXBinder("IRIS/AllBonesTexture2D")]
	public class VFXAllBones2DTextureBinder : VFXBinderBase
	{

		[VFXPropertyBinding("Texture"), SerializeField]
		protected ExposedProperty TextureProperty = "Texture";


		public override bool IsValid(VisualEffect component)
		{

			return component.HasTexture(TextureProperty) ;
		}

		public override void UpdateBinding(VisualEffect component)
		{
			component.SetTexture(TextureProperty, FXDataProvider.GetAllBones2DTexture());
		}

		public override string ToString()
		{
			return "IRIS: AllBones2DTexture Binder";
		}

	}
}