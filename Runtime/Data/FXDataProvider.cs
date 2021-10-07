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
			HandsAboveElbows, 
			HandsAboveElbows2D, 
		}

		public enum FLOAT_DATA_TYPE
		{
			AudioLevel, 
			AudioBeat, 

			HandsHorizontalSeparation, 
			HandsVerticalSeparation,
			PelvisToLeftHand,
			PelvisToRightHand,
			HandsToPelvisFactor,
			UserHorizontalPosition,			

			AmplitudeSetting,

			HandsHorizontalSeparation2D,
			HandsVerticalSeparation2D,
			PelvisToLeftHand2D,
			PelvisToRightHand2D,
			HandsToPelvisFactor2D,
			UserHorizontalPosition2D,
		}

		public enum INT_DATA_TYPE
		{
			BoneCount,
			UserCount,
			ScreenWidth, 
			ScreenHeight
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
				case FLOAT_DATA_TYPE.UserHorizontalPosition:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.UserHorizontalPosition);
				case FLOAT_DATA_TYPE.PelvisToLeftHand:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.PelvisToLeftHand, userIndex);
				case FLOAT_DATA_TYPE.PelvisToRightHand:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.PelvisToRightHand, userIndex);

				case FLOAT_DATA_TYPE.HandsHorizontalSeparation2D:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.HandsHorizontalSeparation2D, userIndex);
				case FLOAT_DATA_TYPE.HandsVerticalSeparation2D:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.HandsVerticalSeparation2D);
				case FLOAT_DATA_TYPE.HandsToPelvisFactor2D:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.HandsToPelvisFactor2D);
				case FLOAT_DATA_TYPE.UserHorizontalPosition2D:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.UserHorizontalPosition2D);
				case FLOAT_DATA_TYPE.PelvisToLeftHand2D:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.PelvisToLeftHand2D, userIndex);
				case FLOAT_DATA_TYPE.PelvisToRightHand2D:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.PelvisToRightHand2D, userIndex);

				case FLOAT_DATA_TYPE.AmplitudeSetting:
					return CVInterface.GetFloat(FLOAT_DATA_TYPE.AmplitudeSetting);
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
				case INT_DATA_TYPE.UserCount:
					return CVInterface.GetUserCount();
				case INT_DATA_TYPE.ScreenWidth:
					return Screen.width;
				case INT_DATA_TYPE.ScreenHeight:
					return Screen.height;
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
					/*if( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer )
						return CVInterface.GetUsersMap();
					else
					*/
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

		public static Vector2 GetMapScale(MAP_DATA_TYPE type)
		{
			Vector2 scale = CVInterface.GetTextureScale(type);
			/*
			if (Application.platform == RuntimePlatform.IPhonePlayer)
				scale.x *= -1f;
			*/
			return scale;
		}

		public static Vector2 GetMapSize( MAP_DATA_TYPE type)
		{
			Vector2 output = new Vector2();
			Texture map = null;
			switch (type)
			{
				case MAP_DATA_TYPE.ColorMap:
					map = CVInterface.GetColorMap();
					break;
				case MAP_DATA_TYPE.DepthMap:
					if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
						//map = CVInterface.GetUsersMap();
						map = CVInterface.GetDepthMap();
					else
						map = CVInterface.GetDepthMap();
					break;
				case MAP_DATA_TYPE.UserMap:
					map = CVInterface.GetUsersMap();
					break;
				case MAP_DATA_TYPE.ColorPointCloud:
					map = CVInterface.GetColorPointCloud();
					break;
				case MAP_DATA_TYPE.VertexPointCloud:
					map = CVInterface.GetVertexPointCloud();
					break;
				default:
					break;

			}
			if( map == null)
			{
				return output;
			}
			output.x = map.width;
			output.y = map.height;

			return output;
		}

		public static Texture GetAllBonesTexture()
		{
			return CVInterface.GetAllBonesTexture();
		}

		public static Texture GetAllBones2DTexture()
		{
			return CVInterface.GetAllBones2DTexture();
		}

		public static Vector3 GetJointPosition(IRISJoints.Joints joint, int userIndex = 0)
		{
			return CVInterface.GetJointPos3D(joint, userIndex);
		}

		public static Vector2 GetJoint2DPosition(IRISJoints.Joints2D joint, int userIndex = 0)
		{
			return CVInterface.GetJointPos2D(joint, userIndex);
		}

		public static Vector3 GetJointRotation(IRISJoints.Joints joint, int userIndex = 0)
		{
			return CVInterface.GetJointRot3D(joint, userIndex);
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
