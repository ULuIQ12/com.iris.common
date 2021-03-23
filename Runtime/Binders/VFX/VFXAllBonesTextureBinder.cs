using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace com.iris.common
{
	
	[AddComponentMenu("IRIS/AllBonesTexture Binder")]
	[VFXBinder("IRIS/AllBonesTexture")]
	public class VFXAllBonesTextureBinder : VFXBinderBase
	{

		[VFXPropertyBinding("Texture"), SerializeField]
		protected ExposedProperty TextureProperty = "Texture";

		[VFXPropertyBinding("System.Int32"), SerializeField]
		protected ExposedProperty BoneCountProperty = "BoneCount";

		//public FXDataProvider.MAP_DATA_TYPE TextureToBind = FXDataProvider.MAP_DATA_TYPE.ColorMap;

		public override bool IsValid(VisualEffect component)
		{
		
			return component.HasTexture(TextureProperty) && component.HasInt(BoneCountProperty);
		}

		public override void UpdateBinding(VisualEffect component)
		{
			component.SetTexture(TextureProperty, FXDataProvider.GetAllBonesTexture());
			component.SetInt(BoneCountProperty, FXDataProvider.GetInt(FXDataProvider.INT_DATA_TYPE.BoneCount));
		}

		public override string ToString()
		{
			return "IRIS: AllBonesTexture Binder";
		}

	}
}