using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Renderer))]
	public class IRISShaderBinderBase : MonoBehaviour
    {
		protected Renderer MyRenderer;

		void Awake()
		{
			GetRenderer();
		}

		protected void GetRenderer()
		{
			MyRenderer = GetComponent<Renderer>();
		}
	}
}
