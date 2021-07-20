using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace com.iris.common
{
			
	[AddComponentMenu("IRIS/Clutter Binder")]
	[VFXBinder("IRIS/Clutter")]
	public class VFXClutterBinder : VFXBinderBase
	{

		[VFXPropertyBinding("ClutterFloat"), SerializeField]
		protected ExposedProperty FloatProperty = "FloatProperty";

		public FXDataProvider.MAP_DATA_TYPE TextureToBind = FXDataProvider.MAP_DATA_TYPE.UserMap;
		public int NbSamplesWidth = 64;
		public int NbSamplesHeight = 64;
		private Texture LastedDepthTexture;

		public float countLeft = 0;
		public float countRight = 0;
		public float percentRightLeft;

		public override bool IsValid(VisualEffect component)
		{
			return component.HasFloat(FloatProperty);
		}

		public override void UpdateBinding(VisualEffect component)
		{
			LastedDepthTexture = FXDataProvider.GetMap(TextureToBind);
			if (LastedDepthTexture == CVInterface.EmptyTexture)
				return;
			Texture2D t2d = TextureToTexture2D(LastedDepthTexture);
			Vector2 tscale = FXDataProvider.GetMapScale(TextureToBind);
			
			int i = 0;
			int j = 0;
			countLeft = countRight = 0;
			for (j = 0; j < NbSamplesHeight; j++)
			{
				for (i = 0; i < NbSamplesWidth; i++)
				{
					int index = i * NbSamplesWidth + j;
					int px;
					if( tscale.x > 0 )
						px = Mathf.FloorToInt((float)i / (float)NbSamplesWidth * (float)LastedDepthTexture.width);
					else
						px = (LastedDepthTexture.width - 1) - Mathf.FloorToInt((float)i / (float)NbSamplesWidth * (float)LastedDepthTexture.width);
					int py = 0;
					if( tscale.y > 0)
						py = Mathf.FloorToInt((float)j / (float)NbSamplesHeight * (float)LastedDepthTexture.height);
					else
						py = (LastedDepthTexture.height - 1) - Mathf.FloorToInt((float)j / (float)NbSamplesHeight * (float)LastedDepthTexture.height);

					Color col = t2d.GetPixel(px, py);
					if( col.r + col.g + col.b > 0 )
					{
						//actif
						if(i<(NbSamplesWidth*0.5))
							countLeft += 1;
						else 
							countRight += 1;
					}
					else
					{
						//inactif                    
					}
				}
			}

			if( (countLeft+countRight) == 0 )
			{
				percentRightLeft = 0;
			}
			else 
			{
				percentRightLeft = Remap(0, 1, -1, 1, countLeft / (countLeft + countRight));            
			}

			component.SetFloat(FloatProperty, percentRightLeft);
		}

		private Texture2D depthT2D;
		private Texture2D TextureToTexture2D(Texture texture)
		{
			if( depthT2D == null || depthT2D.width != texture.width || depthT2D.height != texture.height)
				depthT2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
			
			RenderTexture currentRT = RenderTexture.active;
			RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
			Graphics.Blit(texture, renderTexture);

			RenderTexture.active = renderTexture;
			depthT2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			depthT2D.Apply();

			RenderTexture.active = currentRT;
			RenderTexture.ReleaseTemporary(renderTexture);
			return depthT2D;
		}

		private float Remap(float InputLow, float InputHigh, float OutputLow, float OutputHigh, float value )
		{
			value = Mathf.InverseLerp(InputLow, InputHigh, value);
			value = Mathf.Lerp(OutputLow, OutputHigh, value);

			return value;
		}		

		public override string ToString()
		{
			return "IRIS: Float Position Binder";
		}

	}
}