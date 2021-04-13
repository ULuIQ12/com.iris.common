using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
    public class ColliderFromDepth : MonoBehaviour
    {
		public Camera ExperienceCamera;
		public int NbSamplesWidth = 512;
		public int NbSamplesHeight = 512;
		public float ColliderRadius = 0.1f;
		private Rect SpawnZone;
		private Texture LastedDepthTexture;
		private Coroutine WaitRoutine;
		private GameObject[] ColliderGOs;

		public bool visualDebugOn = true;

		private bool IsStarted = false;

		public IEnumerator Start()
		{
			yield return WaitRoutine = StartCoroutine(WaitForDepthImage());
			InitColliders();
		}

		public void InitColliders()
		{
			if (!IsStarted)
				return;

			

			//Debug.Log("TEX SIZE = " + LastedDepthTexture.width + "/" + LastedDepthTexture.height);

			float worldScreenHeight;
			if( ExperienceCamera == null )
				worldScreenHeight = 10f;
			else 
				worldScreenHeight = ExperienceCamera.orthographicSize * 2f;

			float textHeight = LastedDepthTexture.height;
			float scale = worldScreenHeight / textHeight;
			//Debug.Log("Scale = " + scale);

			SpawnZone = new Rect(-LastedDepthTexture.width / 2 * scale, -LastedDepthTexture.height / 2 * scale , LastedDepthTexture.width * scale , LastedDepthTexture.height * scale);

			int totalSamples = NbSamplesWidth * NbSamplesHeight;
			ColliderGOs = new GameObject[totalSamples];

			int i = 0;
			Vector3 topleft = new Vector3(0f, 0f, ExperienceCamera.transform.position.z);
			Vector3 lowerRight = new Vector3(Screen.width, Screen.height, ExperienceCamera.transform.position.z);

			int j = 0;
			for ( j=0;j<NbSamplesHeight;j++)
			{
				for( i=0;i<NbSamplesWidth;i++)
				{
					int index = i * NbSamplesWidth + j;
					GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
					go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
					go.transform.localScale = new Vector3(1f, 10f, 1f);

					if( !visualDebugOn)
					{
						go.GetComponent<MeshRenderer>().enabled = false;
					}
					go.SetActive(false);

					
					go.transform.parent = transform;
					float posx = SpawnZone.x + (float)i / (float)NbSamplesWidth * (float)SpawnZone.width;
					float posy = SpawnZone.y + (float)j / (float)NbSamplesHeight * (float)SpawnZone.height;

					go.transform.localPosition = new Vector3(posx, posy, 0f);
					ColliderGOs[index] = go;
					
				}
			}
			
		}

		public void Update()
		{
			UpdateColliderPositions();
		}

		private void UpdateColliderPositions()
		{
			if (ColliderGOs != null && ColliderGOs.Length > 0)
			{
				LastedDepthTexture = FXDataProvider.GetMap(FXDataProvider.MAP_DATA_TYPE.UserMap);
				if (LastedDepthTexture == CVInterface.EmptyTexture)
					return;
				Texture2D t2d = TextureToTexture2D(LastedDepthTexture);
				Vector2 tscale = FXDataProvider.GetMapScale(FXDataProvider.MAP_DATA_TYPE.UserMap);
				int i = 0;
				int j = 0;
				for (j = 0; j < NbSamplesHeight; j++)
				{
					for (i = 0; i < NbSamplesWidth; i++)
					{
						int index = i * NbSamplesWidth + j;
						int px;
						if( tscale.x < 0 ) // inferieur pour mirroir
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
							ColliderGOs[index].SetActive(true);
						}
						else
						{
							ColliderGOs[index].SetActive(false);
						}
					}
				}
			}
			else
			{
				InitColliders();
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

		private IEnumerator WaitForDepthImage()
		{
			yield return new WaitForSeconds(.5f);
			bool isReady = false;
			while (!isReady)
			{
				LastedDepthTexture = FXDataProvider.GetMap(FXDataProvider.MAP_DATA_TYPE.UserMap);
				if (LastedDepthTexture == CVInterface.EmptyTexture)
					yield return null;
				else
					isReady = true;
			}
			yield return new WaitForSeconds(.5f);

			IsStarted = true;
		}

		public void OnDestroy()
		{
			if (WaitRoutine != null)
				StopCoroutine(WaitRoutine);
		}

	}
}
