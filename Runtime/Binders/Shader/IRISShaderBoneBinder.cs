using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
	[AddComponentMenu("IRIS/Shaders/IRIS Shader Bone Binder")]
	public class IRISShaderBoneBinder : IRISShaderBinderBase
	{

		public enum BONENAMES
		{
			CoudeGauche, 
			CoudeDroit, 
			GenouGauche, 
			GenouDroit,
		}

		[System.Serializable]
		public class BoneBinding
		{
			public BONENAMES Bone = BONENAMES.CoudeDroit;
			public uint userId = 0;
			public string PropertyName = "BonePropertyName";
		}

		public List<BoneBinding> Bones = new List<BoneBinding>();


		void Update()
		{

		}

	}
}
