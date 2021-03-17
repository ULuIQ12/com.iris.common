using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
    public class FXDataProvider : MonoBehaviour
    {
		public static FXDataProvider Instance;

		public enum FLOAT_DATA_TYPE
		{
			AudioLevel, 
			AudioBeat
		}

		public enum MAP_DATA_TYPE
		{
			ColorMap,
			DepthMap,
			UserMap, 
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

		public static float GetFloat(FLOAT_DATA_TYPE type )
		{
			switch(type)
			{
				case FLOAT_DATA_TYPE.AudioBeat:
					return AudioProcessor.GetBeat();
				case FLOAT_DATA_TYPE.AudioLevel:
					return AudioProcessor.GetLevel();
				default:
					return 0.0f;
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
				default:
					return null;
				
			}
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
