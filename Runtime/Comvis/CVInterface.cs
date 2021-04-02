using System;
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
			if(KinectManager.Instance != null && KinectManager.Instance.IsInitialized() && _Instance.CurrentSensorInterface != null)
			{
				return _Instance.CurrentSensorInterface.pointCloudColorTexture;
			}
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Texture GetVertexPointCloud()
		{
			if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized() && _Instance.CurrentSensorInterface != null)
			{
				return _Instance.CurrentSensorInterface.pointCloudVertexTexture;
			}
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Texture GetAllBonesTexture()
		{
			if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized())
			{
				_Instance.UpdateAllBonestexture();
				return _Instance.AllBonesTexture;
			}
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static int GetBoneCount()
		{
			if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized())
			{
				return _Instance.NbBones;
			}
			else return 0;
		}

		public static void CheckForManagerRefresh( bool UseDepth, bool UseSkeleton )
		{
			if (_Instance == null)
				return;

			_Instance.StartCoroutine( _Instance._CheckForManagerRefresh(UseDepth, UseSkeleton) );
		}



		private static void InitEmpty()
		{
			EmptyTexture = Texture2D.whiteTexture;
		}


		////////////////////////////////////////////////////////////////
		private const string KINECT_PREFAB = "CV/KinectManager";
		private const string K2_INTERFACE_PREFAB = "CV/K2Interface";
		private const string ARKIT_INTERFACE_PREFAB = "CV/ARkitInterface";

		public bool Initialized { get; private set; } = false;

		private GameObject ManagerGO;
		private KinectManager KManager;

		private Texture2D AllBonesTexture;
		private int NbBones = 0;

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
		private void Init(bool UseDepth = false, bool UseSkeleton = true)
		{
			ManagerGO = Instantiate(Resources.Load<GameObject>(KINECT_PREFAB), transform);
			ManagerGO.name = "KinectManager";
			KManager = ManagerGO.GetComponent<KinectManager>();
			Debug.Log("CVInterface: Init platform " + Application.platform);

			if( Application.platform == RuntimePlatform.WindowsEditor )
			{

				GameObject igo = Instantiate(Resources.Load<GameObject>(K2_INTERFACE_PREFAB), ManagerGO.transform);
				CurrentSensorInterface = igo.GetComponent<DepthSensorBase>();
			}	
			else if( Application.platform == RuntimePlatform.WindowsPlayer)
			{
				GameObject igo = Instantiate(Resources.Load<GameObject>(K2_INTERFACE_PREFAB), ManagerGO.transform);
				CurrentSensorInterface = igo.GetComponent<DepthSensorBase>();
			}
			else if( Application.platform == RuntimePlatform.IPhonePlayer)
			{
				
				GameObject igo = Instantiate(Resources.Load<GameObject>(ARKIT_INTERFACE_PREFAB), ManagerGO.transform);
				CurrentSensorInterface = igo.GetComponent<DepthSensorBase>();
				
			}
			else
			{
				Debug.LogWarning("What platform ?!? = " + Application.platform);
			}

			if (UseDepth && !UseSkeleton)
			{
				if (KManager.getDepthFrames != KinectManager.DepthTextureType.DepthTexture)
				{
					KManager.getDepthFrames = KinectManager.DepthTextureType.DepthTexture;
					KManager.getBodyFrames = KinectManager.BodyTextureType.None;
				}
			}
			else if (!UseDepth && UseSkeleton)
			{
				if (KManager.getBodyFrames != KinectManager.BodyTextureType.UserTexture)
				{
					KManager.getDepthFrames = KinectManager.DepthTextureType.None;
					KManager.getBodyFrames = KinectManager.BodyTextureType.UserTexture;
				}
			}
			else
			{
				KManager.getDepthFrames = KinectManager.DepthTextureType.DepthTexture;
				KManager.getBodyFrames = KinectManager.BodyTextureType.UserTexture;
			}

			KManager.StartDepthSensors();
			
			Initialized = true;
		}

		private IEnumerator _CheckForManagerRefresh(bool UseDepth, bool UseSkeleton)
		{
			bool shouldRefresh = false;
			if( UseDepth && !UseSkeleton)
			{
				if( KManager.getDepthFrames != KinectManager.DepthTextureType.DepthTexture)
				{
					shouldRefresh = true;
				}
			}
			else if( !UseDepth && UseSkeleton)
			{
				if( KManager.getBodyFrames != KinectManager.BodyTextureType.UserTexture)
				{
					shouldRefresh = true;
				}
			}
			if (shouldRefresh)
			{

				Destroy(ManagerGO);
				ManagerGO = null;
				KManager = null;
				yield return null;
				Init(UseDepth, UseSkeleton);
			}
			yield return null;
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

		private void UpdateAllBonestexture()
		{
			if( AllBonesTexture == null)
			{
				NbBones = Enum.GetNames(typeof(IRISJoints.Joints)).Length;
				AllBonesTexture = new Texture2D(NbBones, 1, TextureFormat.RGBAFloat, false);
			}

			
			//var joints = Enum.GetValues(typeof(IRISJoints.Joints));
			var joints = Enum.GetValues(typeof(KinectInterop.JointType));
			
			ulong uid = KManager.GetUserIdByIndex(0);
			
			var data = new byte[NbBones * 4 * 4];


			int i = 0;
			foreach (KinectInterop.JointType j in joints)
			{
				if (j == KinectInterop.JointType.Count)
					continue;

				Vector3 p = KManager.GetJointPosition(uid, j);
				
				float val;
				byte[] biteval;

				val = p.x;
				biteval = BitConverter.GetBytes(val);
				data[i+0] = biteval[0];
				data[i+1] = biteval[1];
				data[i+2] = biteval[2];
				data[i+3] = biteval[3];

				val = p.y;
				biteval = BitConverter.GetBytes(val);
				data[i + 4] = biteval[0];
				data[i + 5] = biteval[1];
				data[i + 6] = biteval[2];
				data[i + 7] = biteval[3];

				val = p.z;
				biteval = BitConverter.GetBytes(val);
				data[i + 8] = biteval[0];
				data[i + 9] = biteval[1];
				data[i + 10] = biteval[2];
				data[i + 11] = biteval[3];

				val = (p!=Vector3.zero)?1f:0f;
				biteval = BitConverter.GetBytes(val);
				data[i + 12] = biteval[0];
				data[i + 13] = biteval[1];
				data[i + 14] = biteval[2];
				data[i + 15] = biteval[3];

				i += 16 ;
			}

			AllBonesTexture.SetPixelData(data, 0);
			AllBonesTexture.Apply();
		}
	}
}
