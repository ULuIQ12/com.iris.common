#undef UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
#if MODULE_URP_ENABLED
using UnityEngine.Rendering.Universal;
#elif MODULE_LWRP_ENABLED
using UnityEngine.Rendering.LWRP;
#else
using ScriptableRendererFeature = UnityEngine.ScriptableObject;
#endif
using UnityEngine.Experimental.Rendering;

namespace com.iris.common
{ 

	public enum BufferType
	{
		CameraColor,
		Custom
	}

	public class FeedbackFeature : ScriptableRendererFeature
	{
		[System.Serializable]
		public class Settings
		{
			public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

			public Material blitMaterial = null;
			public int blitMaterialPassIndex = -1;
			public BufferType sourceType = BufferType.CameraColor;
			public BufferType destinationType = BufferType.CameraColor;
			public string sourceTextureId = "_SourceTexture";
			public string destinationTextureId = "_DestinationTexture";
		}

		public Settings settings = new Settings();
		FeedbackPass blitPass;

		
		const GraphicsFormat BufferFormat = GraphicsFormat.R8G8B8A8_SRGB;
		
		Material FeedbackMaterial;
		RTHandle _buffer1;
		RTHandle _buffer2;
		FeedbackEffectController _controller = null;

		public override void Create()
		{
			Debug.Log("Create Feedback Feature");
			RTHandles.SetReferenceSize(Screen.width, Screen.height, MSAASamples.MSAA2x);
			_buffer1 = RTHandles.Alloc(Vector2.one, colorFormat: BufferFormat, name: "Feedback Buffer 1");
			_buffer2 = RTHandles.Alloc(Vector2.one, colorFormat: BufferFormat, name: "Feedback Buffer 2");
			_controller = FindObjectOfType<FeedbackEffectController>();
			if (_controller == null)
				return;

			blitPass = new FeedbackPass(name, _buffer1, _buffer2, _controller);
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (_controller == null)
				return;

			if (settings.blitMaterial == null)
			{
				Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
				return;
			}

			blitPass.renderPassEvent = settings.renderPassEvent;
			blitPass.settings = settings;
			renderer.EnqueuePass(blitPass);
		}
	}
}

