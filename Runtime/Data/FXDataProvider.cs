using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
    public class FXDataProvider : MonoBehaviour
    {
		public static FXDataProvider Instance;

		public enum BOOL_DATA_TYPE
		{
			HandsAboveElbows
		}

		public enum FLOAT_DATA_TYPE
		{
			AudioLevel, 
			AudioBeat, 

			HandsHorizontalSeparation, 
			HandsVerticalSeparation,
			HandsToPelvisFactor,

		}

		public enum INT_DATA_TYPE
		{
			BoneCount
		}

		public enum MAP_DATA_TYPE
		{
			ColorMap,
			DepthMap,
			UserMap, 
			ColorPointCloud, 
			VertexPointCloud
		}

		public enum V2_DATA_TYPE
		{

		}

		public enum V3_DATA_TYPE
		{

		}

		public enum COLOR_TYPE
		{
			SystemColor1,
			SystemColor2
		}

		void Awake()
        {
			if (Instance == null)
				Instance = this;
			else
				Destroy(gameObject);
        }

		public static bool GetBool(BOOL_DATA_TYPE type)
		{
			switch(type)
			{
				case BOOL_DATA_TYPE.HandsAboveElbows:
					//return FXDataProvider.GetBool(BOOL_DATA_TYPE.HandsAboveElbows);					
					return false;
				default:
					return false;
			}
		}

		public static float GetFloat(FLOAT_DATA_TYPE type , int userIndex = 0)
		{
			switch(type)
			{
				case FLOAT_DATA_TYPE.AudioBeat:
					return AudioProcessor.GetBeat();
				case FLOAT_DATA_TYPE.AudioLevel:
					return AudioProcessor.GetLevel();
				case FLOAT_DATA_TYPE.HandsHorizontalSeparation:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.HandsHorizontalSeparation, userIndex);
				case FLOAT_DATA_TYPE.HandsVerticalSeparation:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.HandsVerticalSeparation);
				case FLOAT_DATA_TYPE.HandsToPelvisFactor:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.HandsToPelvisFactor);
				default:
					return 0.0f;
			}
		}
		public static int GetInt(INT_DATA_TYPE type, int userIndex =0)
		{
			switch(type)
			{
				case INT_DATA_TYPE.BoneCount:
					return CVInterface.GetBoneCount();
				default:
					return 0;
			}
		}

		public static Vector2 GetV2( V2_DATA_TYPE type )
		{
			return Vector2.zero;
		}

		public static Texture GetMap( MAP_DATA_TYPE type )
		{
			switch(type)
			{
				case MAP_DATA_TYPE.ColorMap:
					return CVInterface.GetColorMap();
				case MAP_DATA_TYPE.DepthMap:
					return CVInterface.GetDepthMap();
				case MAP_DATA_TYPE.UserMap:
					return CVInterface.GetUsersMap();
				case MAP_DATA_TYPE.ColorPointCloud:
					return CVInterface.GetColorPointCloud();
				case MAP_DATA_TYPE.VertexPointCloud:
					return CVInterface.GetVertexPointCloud();
				default:
					return null;
				
			}
		}

		public static Texture GetAllBonesTexture()
		{
			return CVInterface.GetAllBonesTexture();
		}

		public static Color GetColor(COLOR_TYPE type)
		{
#if CREATION_MODE
			Color colRef;
			switch(type)
			{
				case COLOR_TYPE.SystemColor1:
					colRef = Color.red;
					break;
				case COLOR_TYPE.SystemColor2:
					colRef = Color.red;
					break;
				default:
					colRef = Color.red;
					break;
			}
			return colRef;
#else
			Color colRef;
			switch (type)
			{
				case COLOR_TYPE.SystemColor1:
					colRef = Color.blue;
					break;
				case COLOR_TYPE.SystemColor2:
					colRef = Color.blue;
					break;
				default:
					colRef = Color.blue;
					break;
			}
			return colRef;
#endif

		}

	}
}
