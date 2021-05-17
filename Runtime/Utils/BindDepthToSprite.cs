using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.iris.common;

public class BindDepthToSprite : MonoBehaviour
{
	private SpriteRenderer depthImage;
	private Texture2D texDepth2D = null;
	public Camera foregroundCamera;

	public bool Stretch = false;

	void Start()
	{
		depthImage = GetComponent<SpriteRenderer>();

	}
	
	void Update()
	{
		if (depthImage != null)
		{
			if (FXDataProvider.GetMap(FXDataProvider.MAP_DATA_TYPE.UserMap) != CVInterface.EmptyTexture)
			{
				Texture texDepth = FXDataProvider.GetMap(FXDataProvider.MAP_DATA_TYPE.UserMap);

				if (texDepth != null)
				{
					depthImage.enabled = true;
					Rect rectDepth = new Rect(0, 0, texDepth.width, texDepth.height);
					Vector2 pivotSprite = new Vector2(0.5f, 0.5f);

					if (texDepth2D == null && texDepth != null)
					{
						texDepth2D = new Texture2D(texDepth.width, texDepth.height, TextureFormat.ARGB32, false);

						depthImage.sprite = Sprite.Create(texDepth2D, rectDepth, pivotSprite);
						Vector2 depthImageScale = FXDataProvider.GetMapScale(FXDataProvider.MAP_DATA_TYPE.DepthMap);
						
						depthImage.flipX = depthImageScale.x < 0;
						depthImage.flipY = depthImageScale.y < 0;
					}

					if (texDepth2D != null)
					{
						Graphics.CopyTexture(texDepth, texDepth2D);
					}

					float worldScreenHeight = foregroundCamera.orthographicSize * 2f;
					float spriteHeight = depthImage.sprite.bounds.size.y;

					float scaleX = 1.0f;
					float scaleY = 1.0f;
					if(!Stretch)
					{
						scaleX = scaleY = worldScreenHeight / spriteHeight;
					}
					else
					{
						
						scaleY = worldScreenHeight / spriteHeight;
						scaleX = scaleY * (depthImage.sprite.bounds.size.x / depthImage.sprite.bounds.size.y);
					}

					//float scale = worldScreenHeight / spriteHeight;
					//depthImage.transform.localScale = new Vector3(scale, scale, 1f);
					depthImage.transform.localScale = new Vector3(scaleX, scaleY, 1f);
				}
				else
				{
					depthImage.enabled = false;
				}
			}
			else
			{
				depthImage.enabled = false;
			}
		}
	}
}
