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
		public static Texture2D EmptyTexture;

		private static float AmplitudeLevel = 1.0f;
		public static void SetAmplitudeLevel( float level )
		{
			AmplitudeLevel = level;
		}

		public static Vector2 GetJointPositionTex(IRISJoints.Joints joints, ulong userId = 0)
		{
			return Vector2.zero;
		}

		public static Vector3 GetJointPos3D(IRISJoints.Joints joint, int userIndex = 0)
		{
			if (AreDatasAvailable())
			{
				ulong uid = KinectManager.Instance.GetUserIdByIndex(userIndex);

				Vector3 pos;
				if (Application.platform == RuntimePlatform.IPhonePlayer)
				{
					pos = KinectManager.Instance.GetJointPosition(uid, IRISJoints.GetInvertedKinectJoint(joint));
					pos.x *= -1f;
				}
				else
				{
					pos = KinectManager.Instance.GetJointPosition(uid, IRISJoints.GetKinectJoint(joint));
				}
				//Debug.Log(joint + "/" + userIndex);
				return pos;
			}
			return Vector3.zero;
		}

		public static Vector3 GetJointRot3D(IRISJoints.Joints joint, int userIndex = 0)
		{
			if (AreDatasAvailable())
			{
				ulong uid = KinectManager.Instance.GetUserIdByIndex(userIndex);

				Vector3 rot;
				if (Application.platform == RuntimePlatform.IPhonePlayer)
				{
					rot = KinectManager.Instance.GetJointOrientation(uid, IRISJoints.GetInvertedKinectJoint(joint),true).eulerAngles;
				
				}
				else
				{
					rot = KinectManager.Instance.GetJointOrientation(uid, IRISJoints.GetInvertedKinectJoint(joint), true).eulerAngles;
				}
				
				return rot;
			}
			return Vector3.zero;
		}

		public static bool IsJointTracked(IRISJoints.Joints joint, int userIndex = 0)
		{
			if (!AreDatasAvailable())
			{
				return false;
			}
			ulong uid = KinectManager.Instance.GetUserIdByIndex(userIndex);
			return KinectManager.Instance.IsJointTracked(uid, IRISJoints.GetKinectJoint(joint));
		}

		public static float GetFloat( FXDataProvider.FLOAT_DATA_TYPE type, int userIndex = 0)
		{
			if (!AreDatasAvailable())
			{
				return 0.0f;
			}
			

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
				case FXDataProvider.FLOAT_DATA_TYPE.UserHorizontalPosition:
					return _Instance.UsersMetaDatas[userID].UserHorizontalPosition;
				case FXDataProvider.FLOAT_DATA_TYPE.PelvisToLeftHand:
					return _Instance.UsersMetaDatas[userID].PelvisToLeftHand;
				case FXDataProvider.FLOAT_DATA_TYPE.PelvisToRightHand:
					return _Instance.UsersMetaDatas[userID].PelvisToRightHand;
				case FXDataProvider.FLOAT_DATA_TYPE.AudioBeat:
					return AudioProcessor.GetBeat();
				case FXDataProvider.FLOAT_DATA_TYPE.AudioLevel:
					return AudioProcessor.GetLevel();
				case FXDataProvider.FLOAT_DATA_TYPE.AmplitudeSetting:
					return AmplitudeLevel;
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
			if (AreDatasAvailable())
			{
				return KinectManager.Instance.GetDepthImageTex(0);
			}

			if (EmptyTexture == null)
				InitEmpty();

			return EmptyTexture;
		}
		public static Vector2 LastUsersMapDimensions = new Vector2();
		public static Texture GetUsersMap()
		{
			if (AreDatasAvailable())
			{
				Texture t = KinectManager.Instance.GetUsersImageTex(0);
				if( t == null)
				{
					if (EmptyTexture == null)
						InitEmpty();
					return EmptyTexture;
				}
				if( t.height > 100)
					LastUsersMapDimensions.Set(t.width, t.height);
				return t;
			}

			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Texture GetColorMap()
		{
			if (AreDatasAvailable())
			{
				return KinectManager.Instance.GetColorImageTex(0);
			}
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Texture GetColorPointCloud()
		{
			if (AreDatasAvailable())
			{
				return _Instance.CurrentSensorInterface.pointCloudColorTexture;
			}
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Texture GetVertexPointCloud()
		{
			if (AreDatasAvailable())
			{
				return _Instance.CurrentSensorInterface.pointCloudVertexTexture;
			}
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Vector2 GetTextureScale(FXDataProvider.MAP_DATA_TYPE type)
		{

			if (!AreDatasAvailable())
			{
				return Vector2.one; 
			}

			switch (type)
			{
				case FXDataProvider.MAP_DATA_TYPE.ColorMap:
					return KinectManager.Instance.GetColorImageScale(0);
				case FXDataProvider.MAP_DATA_TYPE.DepthMap:
					return KinectManager.Instance.GetDepthImageScale(0);
				case FXDataProvider.MAP_DATA_TYPE.UserMap:
					return KinectManager.Instance.GetDepthImageScale(0);
				case FXDataProvider.MAP_DATA_TYPE.ColorPointCloud:
				case FXDataProvider.MAP_DATA_TYPE.VertexPointCloud:
				default:
					return Vector2.one;
			}
		}

		public static Texture GetAllBonesTexture()
		{
			if (AreDatasAvailable())
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
			if (AreDatasAvailable())
			{
				return _Instance.NbBones;
			}
			else return 0;
		}

		public static int GetUserCount()
		{
			if( AreDatasAvailable() )
			{
				return _Instance.KManager.GetUsersCount() ;
			}
			else return 0;
		}

		//public static void CheckForManagerRefresh( bool UseDepth, bool UseSkeleton )
		public static void CheckForManagerRefresh( ExpSettings.DATA_REQUESTED dataRequested)
		{
			if (_Instance == null)
				return;

			//_Instance.StartCoroutine( _Instance._CheckForManagerRefresh(UseDepth, UseSkeleton) );
			_Instance.StartCoroutine( _Instance._CheckForManagerRefresh(dataRequested) );
		}



		private static void InitEmpty()
		{
			EmptyTexture = Texture2D.blackTexture;
		}

		public static bool AreDatasAvailable()
		{
			return (_Instance != null && _Instance.Initialized && KinectManager.Instance != null && KinectManager.Instance.IsInitialized() && _Instance.CurrentSensorInterface !=null );
		}

		private static bool LastDebugValue = false;
		public static void SetDebug(bool isOn)
		{
			LastDebugValue = isOn;

			if (!AreDatasAvailable())
				return;

			if (isOn)
			{
				List<KinectManager.DisplayImageType> disp = new List<KinectManager.DisplayImageType>();
				disp.Add(KinectManager.DisplayImageType.Sensor0ColorImage);
				disp.Add(KinectManager.DisplayImageType.Sensor0DepthImage);
				disp.Add(KinectManager.DisplayImageType.UserBodyImage);
				KinectManager.Instance.displayImages = disp;
			}
			else
			{
				KinectManager.Instance.displayImages.Clear();
			}

		}

		////////////////////////////////////////////////////////////////
		private const string KINECT_PREFAB = "CV/KinectManager";
		private const string K2_INTERFACE_PREFAB = "CV/K2Interface";
		private const string ARKIT_INTERFACE_PREFAB = "CV/ARkitInterface";

		public bool Initialized { get; private set; } = false;

		private GameObject ManagerGO;
		private KinectManager KManager;

		private Texture2D AllBonesTexture;
		private int NbBones = Enum.GetNames(typeof(KinectInterop.JointType)).Length;

		private class UserBonesMetaData
		{
			public float HandsHorizontalSeparation = 0.0f;
			public float HandsVerticalSeparation = 0.0f;
			public float HandsToPelvisFactor = 0.0f;
			public float UserHorizontalPosition = 0.0f;
			public float PelvisToLeftHand = 0.0f;
			public float PelvisToRightHand = 0.0f;
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
		private ExpSettings.DATA_REQUESTED LastResquestedData = ExpSettings.DATA_REQUESTED.Body;
		private void Init(ExpSettings.DATA_REQUESTED dataRequest = ExpSettings.DATA_REQUESTED.Body)
		//private void Init(bool UseDepth = false, bool UseSkeleton = true)
		{
			Initialized = false;

			ManagerGO = Instantiate(Resources.Load<GameObject>(KINECT_PREFAB), transform);
			ManagerGO.name = "KinectManager";
			KManager = ManagerGO.GetComponent<KinectManager>();
			Debug.Log("CVInterface: Init platform " + Application.platform + "/ data req = " + dataRequest);

			LastResquestedData = dataRequest;

			if ( Application.platform == RuntimePlatform.WindowsEditor )
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

			if( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			{
				KManager.getDepthFrames = KinectManager.DepthTextureType.DepthTexture;
				KManager.getBodyFrames = KinectManager.BodyTextureType.UserTexture;

			}
			else if( Application.platform == RuntimePlatform.IPhonePlayer)
			{
				ARKitInterface ark = CurrentSensorInterface as ARKitInterface;
				if ( dataRequest == ExpSettings.DATA_REQUESTED.Body)
				{
					KManager.getDepthFrames = KinectManager.DepthTextureType.None;
					KManager.getBodyFrames = KinectManager.BodyTextureType.UserTexture;
				}
				else if( dataRequest == ExpSettings.DATA_REQUESTED.Depth)
				{
					KManager.getDepthFrames = KinectManager.DepthTextureType.DepthTexture;
					KManager.getBodyFrames = KinectManager.BodyTextureType.None;

					ark.depthMode = com.rfilkov.kinect.ARKitInterface.ArKitDepthMode.EnvironmentDepth;
				}
				else if( dataRequest == ExpSettings.DATA_REQUESTED.Users)
				{
					KManager.getDepthFrames = KinectManager.DepthTextureType.DepthTexture;
					KManager.getBodyFrames = KinectManager.BodyTextureType.None;

					ark.depthMode = com.rfilkov.kinect.ARKitInterface.ArKitDepthMode.HumanDepth;
				}
				else
				{
					Debug.Log("Probleme avec dataRequest : " + dataRequest);
				}

				Debug.Log("arkit depth set to " + ark.depthMode);
			}
			/*
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
				if (Application.platform == RuntimePlatform.IPhonePlayer)
				{
					KManager.getDepthFrames = KinectManager.DepthTextureType.None;
					KManager.getBodyFrames = KinectManager.BodyTextureType.UserTexture;
				}
				else
				{
					KManager.getDepthFrames = KinectManager.DepthTextureType.DepthTexture;
					KManager.getBodyFrames = KinectManager.BodyTextureType.UserTexture;
				}
			}
			*/
			/*
			if( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			{
				KManager.getDepthFrames = KinectManager.DepthTextureType.DepthTexture;
				KManager.getBodyFrames = KinectManager.BodyTextureType.UserTexture;
			}
			*/
			SetDebug(LastDebugValue);
			KManager.StartDepthSensors();
			
			Initialized = true;
		}

		private IEnumerator _CheckForManagerRefresh(ExpSettings.DATA_REQUESTED dataResquested)
		{
			Debug.Log("_CheckForManagerRefresh : " + dataResquested + "/ platform = " + Application.platform );
			bool shouldRefresh = false;

			if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			{
				shouldRefresh = false;
			}
			else if( Application.platform == RuntimePlatform.IPhonePlayer)
			{
				shouldRefresh = (dataResquested != LastResquestedData);				
			}
			else
			{
				shouldRefresh = false;
			}
			//if ( UseDepth && !UseSkeleton)
			/*
		if (dataResquested == ExpSettings.DATA_REQUESTED.Depth)
		{
			if( KManager.getDepthFrames != KinectManager.DepthTextureType.DepthTexture || KManager.getBodyFrames != KinectManager.BodyTextureType.None)
			{
				shouldRefresh = true;
			}
		}
		//else if( !UseDepth && UseSkeleton)
		else if (dataResquested == ExpSettings.DATA_REQUESTED.Body)
		{
			if( KManager.getBodyFrames != KinectManager.BodyTextureType.UserTexture || KManager.getDepthFrames != KinectManager.DepthTextureType.None)
			{
				shouldRefresh = true;
			}
		}
		else if( dataResquested == ExpSettings.DATA_REQUESTED.Users)
		{
			if (KManager.getBodyFrames != KinectManager.BodyTextureType.UserTexture || KManager.getDepthFrames != KinectManager.DepthTextureType.None)
			{
				shouldRefresh = true;
			}
		}
		*/
			/*
			else if( UseDepth && UseSkeleton)
			{
				shouldRefresh = true;
			}
			*/
			/*
			if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			{
				shouldRefresh = false;
			}*/
			Debug.Log("ShouldRefresh = " + shouldRefresh);
			if (shouldRefresh)
			{
				Debug.Log("We should Refresh CVInterface for " + dataResquested);
				Destroy(ManagerGO);
				ManagerGO = null;
				KManager = null;
				yield return null;
				//Init(UseDepth, UseSkeleton);
				Init(dataResquested);
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

				
				/*
				Vector3 HandRightPosition = KManager.GetJointPosition(user, KinectInterop.JointType.HandRight);
				Vector3 HandLeftPosition = KManager.GetJointPosition(user, KinectInterop.JointType.HandLeft);

				Vector3 ElbowRightPosition = KManager.GetJointPosition(user, KinectInterop.JointType.ElbowRight);
				Vector3 ElbowLeftPosition = KManager.GetJointPosition(user, KinectInterop.JointType.ElbowLeft);

				Vector3 PelvisPosition = KManager.GetJointPosition(user, KinectInterop.JointType.Pelvis);
				*/

				Vector3 HandRightPosition = GetJointPos3D(IRISJoints.Joints.HandRight);
				Vector3 HandLeftPosition = GetJointPos3D(IRISJoints.Joints.HandLeft);

				Vector3 ElbowRightPosition = GetJointPos3D(IRISJoints.Joints.ElbowRight);
				Vector3 ElbowLeftPosition = GetJointPos3D(IRISJoints.Joints.ElbowLeft);

				Vector3 PelvisPosition = GetJointPos3D(IRISJoints.Joints.Pelvis);

				UsersMetaDatas[user].UserHorizontalPosition = HandRightPosition.x + (HandLeftPosition.x - HandRightPosition.x) / 2f;// KManager.GetJointPosition(user, KinectInterop.JointType.SpineNaval).x;
				UsersMetaDatas[user].HandsHorizontalSeparation = HandRightPosition.x - HandLeftPosition.x;
				UsersMetaDatas[user].HandsVerticalSeparation = HandRightPosition.y - HandLeftPosition.y;
				

				float distHandTopRight = Math.Abs( HandRightPosition.y - PelvisPosition.y );
				float distHandTopLeft = Math.Abs( HandLeftPosition.y - PelvisPosition.y );

				UsersMetaDatas[user].PelvisToLeftHand = distHandTopLeft;
				UsersMetaDatas[user].PelvisToRightHand = distHandTopRight;

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


		private byte[] boneData;
		private void UpdateAllBonestexture()
		{
			int userCount = KManager.GetUsersCount();

			if (userCount == 0)
			{
				if (EmptyTexture == null)
					InitEmpty();
				AllBonesTexture = EmptyTexture;
				//NbBones = Enum.GetNames(typeof(KinectInterop.JointType)).Length; 
				NbBones = Enum.GetNames(typeof(IRISJoints.Joints)).Length;
				return;
			}
			

			if (AllBonesTexture == null || (userCount > 0 && userCount != AllBonesTexture.height ) )
			{
				NbBones = Enum.GetNames(typeof(IRISJoints.Joints)).Length;
				//NbBones = Enum.GetNames(typeof(KinectInterop.JointType)).Length;
				AllBonesTexture = new Texture2D(NbBones, userCount, TextureFormat.RGBAFloat, false);
				AllBonesTexture.filterMode = FilterMode.Point;
			}
			
				
			
			var lineLenght = NbBones * 4 * 4;
			var totalbytes = lineLenght * userCount;
			if ( boneData == null || boneData.Length != totalbytes)
				boneData = new byte[totalbytes];

			//var joints = Enum.GetValues(typeof(KinectInterop.JointType));
			var joints = Enum.GetValues(typeof(IRISJoints.Joints));

			int u = 0;
			for (u = 0; u < userCount; u++)
			{
				ulong uid = KManager.GetUserIdByIndex(u);
				int userDecal = u * lineLenght;
				int i = 0;

				//foreach (KinectInterop.JointType j in joints)
				foreach (IRISJoints.Joints j in joints)
				{
					//if (j == KinectInterop.JointType.Count)
						//continue;

					if( (int)j >= 23)
					{
						continue;
					}

					Vector3 p = GetJointPos3D(j, u);
					
					//Vector3 p = KManager.GetJointPosition(uid, j);

					float val;
					byte[] biteval;

					//int invertX = (Application.platform == RuntimePlatform.IPhonePlayer)?-1:1;

					//val = p.x * KManager.GetSensorSpaceScale(0).x * invertX; 
					val = p.x * KManager.GetSensorSpaceScale(0).x;
					biteval = BitConverter.GetBytes(val);
					boneData[userDecal + i + 0] = biteval[0];
					boneData[userDecal + i + 1] = biteval[1];
					boneData[userDecal + i + 2] = biteval[2];
					boneData[userDecal + i + 3] = biteval[3];

					val = p.y * KManager.GetSensorSpaceScale(0).y;
					biteval = BitConverter.GetBytes(val);
					boneData[userDecal + i + 4] = biteval[0];
					boneData[userDecal + i + 5] = biteval[1];
					boneData[userDecal + i + 6] = biteval[2];
					boneData[userDecal + i + 7] = biteval[3];

					val = p.z * KManager.GetSensorSpaceScale(0).z;
					biteval = BitConverter.GetBytes(val);
					boneData[userDecal + i + 8] = biteval[0];
					boneData[userDecal + i + 9] = biteval[1];
					boneData[userDecal + i + 10] = biteval[2];
					boneData[userDecal + i + 11] = biteval[3];

					val = (p != Vector3.zero) ? 1f : 0f;
					biteval = BitConverter.GetBytes(val);
					boneData[userDecal + i + 12] = biteval[0];
					boneData[userDecal + i + 13] = biteval[1];
					boneData[userDecal + i + 14] = biteval[2];
					boneData[userDecal + i + 15] = biteval[3];

					i += 16;
				}
			}

			AllBonesTexture.SetPixelData(boneData, 0);
			AllBonesTexture.Apply();
		}

		private void TextureToScreenCoord( Vector2 position )
		{
			if( KManager != null)
			{
				KManager.MapDepthPointToColorCoords(0, position, 0);
			}
		}
	}
}
