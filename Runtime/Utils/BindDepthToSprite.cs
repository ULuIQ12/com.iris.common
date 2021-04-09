using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.iris.common;

public class BindDepthToSprite : MonoBehaviour
{
	private SpriteRenderer depthImage;
	private Texture2D texDepth2D = null;
	public Camera foregroundCamera;

	void Start()
	{
		depthImage = GetComponent<SpriteRenderer>();

	}

	// Update is called once per frame
	void Update()
	{
		if (depthImage != null)
		{
			if (FXDataProvider.GetMap(FXDataProvider.MAP_DATA_TYPE.DepthMap) != CVInterface.EmptyTexture)
			{

				//if (kinectManager && kinectManager.IsInitialized() && depthImage /**&& depthImage.sprite == null*/)
				//{
				//Texture texDepth = kinectManager.GetUsersImageTex();
				Texture texDepth = FXDataProvider.GetMap(FXDataProvider.MAP_DATA_TYPE.DepthMap);

				if (texDepth != null)
				{
					depthImage.enabled = true;
					Rect rectDepth = new Rect(0, 0, texDepth.width, texDepth.height);
					Vector2 pivotSprite = new Vector2(0.5f, 0.5f);

					if (texDepth2D == null && texDepth != null)
					//if (texDepth2D == null && texDepth != null && sensorData != null)
					{
						texDepth2D = new Texture2D(texDepth.width, texDepth.height, TextureFormat.ARGB32, false);

						depthImage.sprite = Sprite.Create(texDepth2D, rectDepth, pivotSprite);
						Vector2 depthImageScale = FXDataProvider.GetMapScale(FXDataProvider.MAP_DATA_TYPE.DepthMap);
						depthImage.flipX = depthImageScale.x < 0;
						depthImage.flipY = depthImageScale.y < 0;
						//depthImage.flipX = sensorData.depthImageScale.x < 0;
						//depthImage.flipY = sensorData.depthImageScale.y < 0;
					}

					if (texDepth2D != null)
					{
						Graphics.CopyTexture(texDepth, texDepth2D);
					}

					float worldScreenHeight = foregroundCamera.orthographicSize * 2f;
					float spriteHeight = depthImage.sprite.bounds.size.y;

					float scale = worldScreenHeight / spriteHeight;
					depthImage.transform.localScale = new Vector3(scale, scale, 1f);
				}
				else
				{
					depthImage.enabled = false;
				}
				//}
			}
			else
			{
				depthImage.enabled = false;
			}
		}
	}
}
