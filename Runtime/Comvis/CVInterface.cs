using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;

namespace com.iris.common
{
    public class CVInterface : MonoBehaviour
    {
		public static CVInterface _Instance;
		private static Texture2D EmptyTexture;

		public static Vector2 GetJointPositionTex(IRISJoints.Joints joints, ulong userId = 0)
		{
			return Vector2.zero;
		}

		public static Vector3 GetJointPos3D(IRISJoints.Joints joint, ulong userId = 0)
		{
			if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized() )
			{
				return KinectManager.Instance.GetJointPosition(0, IRISJoints.GetKinectJoint(joint));
			}
			return Vector3.zero;
		}

		public static float GetFloat( FXDataProvider.FLOAT_DATA_TYPE type, int userIndex = 0)
		{
			if (_Instance == null)
				return 0.0f;

			ulong userID = _Instance.KManager.GetUserIdByIndex(userIndex);

			_Instance.UpdateUserMetaBoneData(userID);

			switch (type)
			{
				case FXDataProvider.FLOAT_DATA_TYPE.HandsHorizontalSeparation:
					return _Instance.UsersMetaDatas[userID].HandsHorizontalSeparation;
				case FXDataProvider.FLOAT_DATA_TYPE.HandsToPelvisFactor:
					return _Instance.UsersMetaDatas[userID].HandsToPelvisFactor;
				case FXDataProvider.FLOAT_DATA_TYPE.HandsVerticalSeparation:
					return _Instance.UsersMetaDatas[userID].HandsVerticalSeparation;
				case FXDataProvider.FLOAT_DATA_TYPE.AudioBeat:
					return AudioProcessor.GetBeat();
				case FXDataProvider.FLOAT_DATA_TYPE.AudioLevel:
					return AudioProcessor.GetLevel();
				default:
					return 0.0f;

			}
		}

		public static bool GetMetaBool( FXDataProvider.BOOL_DATA_TYPE type, ulong userID = 0 )
		{
			if (_Instance == null)
				return false;

			_Instance.UpdateUserMetaBoneData(userID);
			switch( type)
			{
				case FXDataProvider.BOOL_DATA_TYPE.HandsAboveElbows:
					return _Instance.UsersMetaDatas[userID].HandsAboveElbows;
				default:
					return false;

			}
			
		}

		public static Texture GetDepthMap()
		{
			if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized())
			{
				return KinectManager.Instance.GetDepthImageTex(0);
			}

			if (EmptyTexture == null)
				InitEmpty();

			return EmptyTexture;
		}

		public static Texture GetUsersMap()
		{
			if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized())
			{
				return KinectManager.Instance.GetUsersImageTex(0);
			}
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Texture GetColorMap()
		{
			if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized())
			{
				return KinectManager.Instance.GetColorImageTex(0);
			}
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Texture GetColorPointCloud()
		{
			if(KinectManager.Instance != null && KinectManager.Instance.IsInitialized())
			{
				return _Instance.CurrentSensorInterface.pointCloudColorTexture;
			}
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Texture GetVertexPointCloud()
		{
			if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized())
			{
				return _Instance.CurrentSensorInterface.pointCloudVertexTexture;
			}
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		

		private static void InitEmpty()
		{
			EmptyTexture = Texture2D.whiteTexture;
		}


		////////////////////////////////////////////////////////////////
		private const string KINECT_PREFAB = "CV/KinectManager";
		private GameObject ManagerGO;
		private KinectManager KManager;

		private class UserBonesMetaData
		{
			public float HandsHorizontalSeparation = 0.0f;
			public float HandsVerticalSeparation = 0.0f;
			public float HandsToPelvisFactor = 0.0f;
			public bool HandsAboveElbows = false;
			public float lastUpdateTime = 0.0f;
		}

		private Dictionary<ulong, UserBonesMetaData> UsersMetaDatas = new Dictionary<ulong, UserBonesMetaData>();

		public void Awake()
		{
			if (_Instance != null)
			{
				Destroy(gameObject);
				return;
			}
			else
				_Instance = this;

			Init();
		}

		private DepthSensorBase CurrentSensorInterface;
		private void Init()
		{
			ManagerGO = Instantiate(Resources.Load<GameObject>(KINECT_PREFAB), transform);
			ManagerGO.name = "KinectManager";
			KManager = ManagerGO.GetComponent<KinectManager>();

			Kinect2Interface k2 = ManagerGO.GetComponentInChildren<Kinect2Interface>();
			ARKitInterface ark = ManagerGO.GetComponentInChildren<ARKitInterface>();

			if( Application.platform == RuntimePlatform.WindowsEditor )
			{
				k2.gameObject.SetActive(true);
				ark.gameObject.SetActive(false);

				CurrentSensorInterface = k2;
			}	
			else if( Application.platform == RuntimePlatform.WindowsPlayer)
			{
				k2.gameObject.SetActive(true);
				ark.gameObject.SetActive(false);

				CurrentSensorInterface = k2;
			}
			else if( Application.platform == RuntimePlatform.IPhonePlayer)
			{
				k2.gameObject.SetActive(false);
				ark.gameObject.SetActive(true);

				CurrentSensorInterface = ark;
			}
			else
			{
				Debug.Log("Platform2 = " + Application.platform);

			}
		}

		private void UpdateUserMetaBoneData( ulong user )
		{
			if( !UsersMetaDatas.ContainsKey(user))
			{
				UsersMetaDatas[user] = new UserBonesMetaData();
			}

			if(KManager != null && KManager.IsInitialized() && KManager.GetUsersCount() > 0 )
			{
				
				if ((Time.time - UsersMetaDatas[user].lastUpdateTime) <= (1f / (float)Application.targetFrameRate))
					return;


				Vector3 HandRightPosition = KManager.GetJointPosition(user, KinectInterop.JointType.HandRight);

				Debug.Log("HandRightPosition= " + HandRightPosition);

				Vector3 HandLeftPosition = KManager.GetJointPosition(user, KinectInterop.JointType.HandLeft);

				Vector3 ElbowRightPosition = KManager.GetJointPosition(user, KinectInterop.JointType.ElbowRight);
				Vector3 ElbowLeftPosition = KManager.GetJointPosition(user, KinectInterop.JointType.ElbowLeft);

				Vector3 PelvisPosition = KManager.GetJointPosition(user, KinectInterop.JointType.Pelvis);

				//posBodyX = KManager.GetJointPosition(user, KinectInterop.JointType.SpineNaval).x;

				UsersMetaDatas[user].HandsHorizontalSeparation = HandRightPosition.x - HandLeftPosition.x;
				UsersMetaDatas[user].HandsVerticalSeparation = HandRightPosition.y - HandLeftPosition.y;

				float distHandTopRight = HandRightPosition.y - PelvisPosition.y;
				float distHandTopLeft = HandLeftPosition.y - PelvisPosition.y;

				UsersMetaDatas[user].HandsToPelvisFactor = distHandTopRight + distHandTopLeft;
				
				if (ElbowLeftPosition.y < HandLeftPosition.y && ElbowRightPosition.y < HandRightPosition.y)
				{
					UsersMetaDatas[user].HandsAboveElbows = true;
				}
				else
				{
					UsersMetaDatas[user].HandsAboveElbows = false;
				}

				UsersMetaDatas[user].lastUpdateTime = Time.time;
			}
		}
	}
}
