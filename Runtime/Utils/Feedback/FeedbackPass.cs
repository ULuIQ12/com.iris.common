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
	internal class FeedbackPass : ScriptableRenderPass
	{
		public FilterMode filterMode { get; set; }
		public FeedbackFeature.Settings settings;

		RenderTargetIdentifier source;
		RenderTargetIdentifier destination;
		int temporaryRTId = Shader.PropertyToID("_TempRT");

		int sourceId;
		int destinationId;
		bool isSourceAndDestinationSameTarget;

		private RTHandle _buffer1;
		private RTHandle _buffer2;
		private FeedbackEffectController _controller;

		string m_ProfilerTag;

		public FeedbackPass(string tag, RTHandle buff1, RTHandle buff2, FeedbackEffectController controller)
		{
			m_ProfilerTag = tag;

			_buffer1 = buff1;
			_buffer2 = buff2;

			_controller = controller;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			
			RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			blitTargetDescriptor.depthBufferBits = 0;

			isSourceAndDestinationSameTarget = settings.sourceType == settings.destinationType &&
				(settings.sourceType == BufferType.CameraColor || settings.sourceTextureId == settings.destinationTextureId);

			var renderer = renderingData.cameraData.renderer;

			if (settings.sourceType == BufferType.CameraColor)
			{
				sourceId = -1;
				source = renderer.cameraColorTarget;
			}
			else
			{
				sourceId = Shader.PropertyToID(settings.sourceTextureId);
				cmd.GetTemporaryRT(sourceId, blitTargetDescriptor, filterMode);
				source = new RenderTargetIdentifier(sourceId);
			}

			if (isSourceAndDestinationSameTarget)
			{
				destinationId = temporaryRTId;
				cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
				destination = new RenderTargetIdentifier(destinationId);
			}
			else if (settings.destinationType == BufferType.CameraColor)
			{
				destinationId = -1;
				destination = renderer.cameraColorTarget;
			}
			else
			{
				destinationId = Shader.PropertyToID(settings.destinationTextureId);
				cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
				destination = new RenderTargetIdentifier(destinationId);
			}
		}

		/// <inheritdoc/>
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

			settings.blitMaterial.SetColor("_Tint", _controller.GetTintColor() );
			settings.blitMaterial.SetVector("_Xform", _controller.GetTransformVector());

			Blit(cmd, _buffer1, _buffer2, settings.blitMaterial, settings.blitMaterialPassIndex);
			Blit(cmd, _buffer2, destination);

			(_buffer1, _buffer2) = (_buffer2, _buffer1);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		/// <inheritdoc/>
		public override void FrameCleanup(CommandBuffer cmd)
		{
			if (destinationId != -1)
				cmd.ReleaseTemporaryRT(destinationId);

			if (source == destination && sourceId != -1)
				cmd.ReleaseTemporaryRT(sourceId);
		}
	}
}
