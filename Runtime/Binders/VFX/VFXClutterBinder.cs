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

		public bool BindClutterHorizontal = false;
		[VFXPropertyBinding("Float"), SerializeField]
		protected ExposedProperty HorizontalProperty = "PercentHorizontalProperty";

		public bool BindClutterVertical = false;
		[VFXPropertyBinding("Float"), SerializeField]
		protected ExposedProperty VerticalProperty = "PercentVerticalProperty";

		public bool BindClutterTotal = false;
		[VFXPropertyBinding("Float"), SerializeField]
		protected ExposedProperty TotalProperty = "PercentTotalProperty";

		public bool BindClutterDeltaHorizontal = false;
		[VFXPropertyBinding("Float"), SerializeField]
		protected ExposedProperty DeltaHorizontalProperty = "DeltaHorizontalProperty";

		public bool BindClutterActivity = false;
		[VFXPropertyBinding("Float"), SerializeField]
		protected ExposedProperty ActivityProperty = "PercentActivityProperty";

		private FXDataProvider.MAP_DATA_TYPE TextureToBind = FXDataProvider.MAP_DATA_TYPE.UserMap;
		public int NbSamplesWidth = 64;
		public int NbSamplesHeight = 64;
		public int totalSamples;
		public bool[] boolActive;
		public bool[] oldBoolActive;
		private Texture LastedDepthTexture;

		public float countLeft = 0;
		public float countRight = 0;
		public float countTop = 0;
		public float countDown = 0;
		public float minLeft = 0;
		public float maxRight = 0;
		public float percent;
		public float countActivity = 0;


		public override bool IsValid(VisualEffect component)
		{
			bool valid = true;
			if( BindClutterHorizontal )
			{
				if (!component.HasFloat(HorizontalProperty))
					valid = false;
			}

			if( BindClutterVertical )
			{
				if (!component.HasFloat(VerticalProperty))
					valid = false;
			}

			if (BindClutterTotal)
			{
				if (!component.HasFloat(TotalProperty))
					valid = false;
			}
			
			if (BindClutterDeltaHorizontal)
			{
				if (!component.HasFloat(DeltaHorizontalProperty))
					valid = false;
			}

			if( BindClutterActivity )
			{
				if (!component.HasFloat(ActivityProperty))
					valid = false;
			}
			
			return valid;
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
			countTop = countDown = countLeft = countRight = countActivity = 0;
			minLeft = NbSamplesWidth;
			maxRight = 0;
			totalSamples = NbSamplesWidth * NbSamplesHeight;
			boolActive = new bool[totalSamples];

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
						boolActive[index] = true;
						if(i<(NbSamplesWidth*0.5))
							countLeft += 1;
						else 
							countRight += 1;

						if(j<(NbSamplesHeight*0.5))
							countTop += 1;
						else 
							countDown += 1;

						if(j > 2 && j < NbSamplesHeight-3) 
						{
							if( i < minLeft) minLeft = i;
							if( i > maxRight) maxRight = i;
						}	
						
					} else {
						boolActive[index] = false;
					}

					if(oldBoolActive != null && oldBoolActive.Length >0 )
						if(oldBoolActive[index] != boolActive[index] )
							countActivity += 1;
						

				}
			}
			oldBoolActive = new bool[totalSamples];
			System.Array.Copy(boolActive, oldBoolActive, totalSamples);
			//oldBoolActive = (bool[]) boolActive.Clone();
			/*if(oldBoolActive != null)
			{
				int rand = Random.Range(0, totalSamples-1);
				Debug.Log( "coucou " + rand + "   " + boolActive[rand] + "   " + oldBoolActive[rand]);
			}*/
			
			float countTotalActive;
			percent = 0.0f;

			if(BindClutterHorizontal)
			{
				countTotalActive = countLeft + countRight;
				if( countTotalActive == 0 )
				{
					percent = 0.0f;
				}
				else 
				{
					percent = countLeft / countTotalActive;            
				}

				component.SetFloat(HorizontalProperty, percent);
			}

			if(BindClutterVertical)
			{
				countTotalActive = countTop + countDown;
				if( countTotalActive == 0 )
				{
					percent = 0.0f;
				}
				else
				{
					percent = countDown / countTotalActive;         
				}

				component.SetFloat(VerticalProperty, percent);
			}

			if(BindClutterTotal)
			{
				countTotalActive = countTop + countDown;
				if(countTotalActive == 0)
				{
					percent = 0.0f;
				}
				else
				{
					percent = countTotalActive / totalSamples;
				}

				component.SetFloat(TotalProperty, percent);
			}

			if(BindClutterDeltaHorizontal)
			{
				countTotalActive = countTop + countDown;
				if(countTotalActive == 0)
				{
					percent = 0.0f;
				}
				else
				{
					percent = (maxRight-minLeft)/NbSamplesWidth;
				}
				
				component.SetFloat( DeltaHorizontalProperty, percent );
			}

			if(BindClutterActivity)
			{
				percent = countActivity/totalSamples;								
				component.SetFloat( ActivityProperty, percent );
			}

			
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
			return "IRIS: Clutter Binder";
		}

	}
}