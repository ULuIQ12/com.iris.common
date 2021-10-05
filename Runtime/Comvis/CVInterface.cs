using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using JetBrains.Annotations;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using XRCameraImageConversionParams = UnityEngine.XR.ARSubsystems.XRCpuImage.ConversionParams;
using CameraImageTransformation = UnityEngine.XR.ARSubsystems.XRCpuImage.Transformation;

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
			if (AreDatasAvailable() && BodyManager != null )
			{
				if (BoneControl != null)
				{
					Vector3 pos = BoneControl.GetBoneWorldPosition((int)IRISJoints.getARFJoint(joint));
					pos.x *= -1f;
					return pos;
				}
				else return Vector3.zero;

			}

			return Vector3.zero;
		}

		public static Vector3 GetJointRot3D(IRISJoints.Joints joint, int userIndex = 0)
		{
			if (AreDatasAvailable() && BodyManager != null && _Instance.LastTrackableHumanId != TrackableId.invalidId)
			{
				if (BoneControl != null)
				{
					
					return BoneControl.GetBoneWorldRotation((int)IRISJoints.getARFJoint(joint)).eulerAngles;
				}
				else return Vector3.zero;

			}


			return Vector3.zero;
		}

		public static Vector2 GetJointPos2D(IRISJoints.Joints2D joint, int userIndex = 0) 
		{
			if( AreDatasAvailable() && BodyManager != null )
			{
				_Instance.Update2DBones();
				try
				{
					Vector2 pos = Bones2D[(int)joint];
					

					return pos;
				}
				catch (Exception e) { return Vector2.zero; };
			}
			
			return Vector2.zero;
		}

		public static bool IsJointTracked(IRISJoints.Joints joint, int userIndex = 0)
		{
			if (AreDatasAvailable() && BodyManager != null && BoneControl != null)
			{
				foreach (ARHumanBody human in BodyManager.trackables)
				{
					if (human.trackableId == _Instance.LastTrackableHumanId)
					{
						var joints = human.joints;
						return joints[(int)IRISJoints.getARFJoint(joint)].tracked;
					}
				}
			}

			return false;
		}

		public static float GetFloat( FXDataProvider.FLOAT_DATA_TYPE type, int userIndex = 0)
		{
			if (!AreDatasAvailable())
			{
				return 0.0f;
			}

			ulong userID = 0; 

			_Instance.UpdateUserMetaBoneData(userID);
			_Instance.UpdateUserMetaBoneData2D(userID);

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

				case FXDataProvider.FLOAT_DATA_TYPE.HandsHorizontalSeparation2D:
					return _Instance.UsersMetaDatas2D[userID].HandsHorizontalSeparation;
				case FXDataProvider.FLOAT_DATA_TYPE.HandsToPelvisFactor2D:
					return _Instance.UsersMetaDatas2D[userID].HandsToPelvisFactor;
				case FXDataProvider.FLOAT_DATA_TYPE.HandsVerticalSeparation2D:
					return _Instance.UsersMetaDatas2D[userID].HandsVerticalSeparation;
				case FXDataProvider.FLOAT_DATA_TYPE.UserHorizontalPosition2D:
					return _Instance.UsersMetaDatas2D[userID].UserHorizontalPosition;
				case FXDataProvider.FLOAT_DATA_TYPE.PelvisToLeftHand2D:
					return _Instance.UsersMetaDatas2D[userID].PelvisToLeftHand;
				case FXDataProvider.FLOAT_DATA_TYPE.PelvisToRightHand2D:
					return _Instance.UsersMetaDatas2D[userID].PelvisToRightHand;


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
			_Instance.UpdateUserMetaBoneData2D(userID);
			switch( type)
			{
				case FXDataProvider.BOOL_DATA_TYPE.HandsAboveElbows:
					return _Instance.UsersMetaDatas[userID].HandsAboveElbows;
				case FXDataProvider.BOOL_DATA_TYPE.HandsAboveElbows2D:
					return _Instance.UsersMetaDatas2D[userID].HandsAboveElbows;
				default:
					return false;

			}
			
		}

		public static Texture GetDepthMap()
		{
			NeedDepthRefresh = true;
			if (AreDatasAvailable() && OccManager != null)
			{
				//return OccManager.environmentDepthTexture;
				
				if( m_DepthTexture != null)
					return m_DepthTexture;
			}

			if (EmptyTexture == null)
				InitEmpty();

			return EmptyTexture;
		}
		public static Vector2 LastUsersMapDimensions = new Vector2();
		public static Texture GetUsersMap()
		{
			NeedUserRefresh = true;
			if ( AreDatasAvailable() && OccManager !=null )
			{
				

				if (m_UserTexture != null)
				{
					LastUsersMapDimensions.Set(m_UserTexture.width, m_UserTexture.height);
					return m_UserTexture;
				}
			}

			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Texture GetColorMap()
		{
			NeedCamRefresh = true;
			if (AreDatasAvailable() && CamManager != null)
			{
				
				if (m_CameraTexture != null)
					return m_CameraTexture;
			}

			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		void OnOcclusionFrameReceived(AROcclusionFrameEventArgs eventArgs)
		{
			
		}

		void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
		{
			
		}



		public static Texture GetColorPointCloud()
		{
			// REDO
			/*
			if (AreDatasAvailable())
			{
				return _Instance.CurrentSensorInterface.pointCloudColorTexture;
			}
			*/
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Texture GetVertexPointCloud()
		{
			// REDO
			/*
			if (AreDatasAvailable())
			{
				return _Instance.CurrentSensorInterface.pointCloudVertexTexture;
			}
			*/
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
				// REDO
				/*
				case FXDataProvider.MAP_DATA_TYPE.ColorMap:
					return KinectManager.Instance.GetColorImageScale(0);
				case FXDataProvider.MAP_DATA_TYPE.DepthMap:
					return KinectManager.Instance.GetDepthImageScale(0);
				case FXDataProvider.MAP_DATA_TYPE.UserMap:
					return KinectManager.Instance.GetDepthImageScale(0);
				case FXDataProvider.MAP_DATA_TYPE.ColorPointCloud:
				case FXDataProvider.MAP_DATA_TYPE.VertexPointCloud:
				*/
			default:
					return Vector2.one;
			}
		}

		public static Texture GetAllBonesTexture()
		{
			if (AreDatasAvailable())
			{
				_Instance.UpdateAllBonestexture();
				_Instance.Update2DBones();
				return _Instance.AllBonesTexture;
			}
			if (EmptyTexture == null)
				InitEmpty();
			return EmptyTexture;
		}

		public static Texture GetAllBones2DTexture()
		{
			if (AreDatasAvailable())
			{
				_Instance.UpdateAllBonestexture();
				_Instance.Update2DBones();
				if (Bones2D != null && _Instance.AllBones2DTexture != null)
				{
					
					return _Instance.AllBones2DTexture;
				}
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
				if (BodyManager != null)
					return BodyManager.trackables.count;
				else
					return 0;
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
			return Session != null;
			
			//return (_Instance != null && _Instance.Initialized && KinectManager.Instance != null && KinectManager.Instance.IsInitialized() && _Instance.CurrentSensorInterface !=null );
		}

		private static bool LastDebugValue = false;
		public static void SetDebug(bool isOn)
		{
			LastDebugValue = isOn;

			if (!AreDatasAvailable())
				return;

			// REDO
			/*
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
			*/
			return;

		}

		////////////////////////////////////////////////////////////////
		private const string KINECT_PREFAB = "CV/KinectManager";
		private const string K2_INTERFACE_PREFAB = "CV/K2Interface";
		private const string ARKIT_INTERFACE_PREFAB = "CV/ARkitInterface";

		private const string ARF_INTERFACE_PREFAB = "CV/ARFoundationInterface";

		public ExpSettings.DATA_REQUESTED DefaultMode = ExpSettings.DATA_REQUESTED.Body;

		public bool Initialized { get; private set; } = false;

		private GameObject ManagerGO;
		//private KinectManager KManager;

		private Texture2D AllBonesTexture;
		//private int NbBones = Enum.GetNames(typeof(KinectInterop.JointType)).Length;
			

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
		private Dictionary<ulong, UserBonesMetaData> UsersMetaDatas2D = new Dictionary<ulong, UserBonesMetaData>();

		public void Awake()
		{
			if (_Instance != null)
			{
				Destroy(gameObject);
				return;
			}
			else
				_Instance = this;

			Init(DefaultMode);
		}

		//private DepthSensorBase CurrentSensorInterface;

		private static ARSession Session;
		private static ARCameraManager CamManager;
		//private static ARCameraBackground CamBG;
		private static AROcclusionManager OccManager;
		private static ARHumanBodyManager BodyManager;
		private static BoneController BoneControl;

		private static Coroutine ImgUpdateRoutine;

		private ExpSettings.DATA_REQUESTED LastResquestedData = ExpSettings.DATA_REQUESTED.Body;
		private void Init(ExpSettings.DATA_REQUESTED dataRequest)
		//private void Init(bool UseDepth = false, bool UseSkeleton = true)
		{
			Initialized = false;

			//if( dataRequest == null)


			/*
			ManagerGO = Instantiate(Resources.Load<GameObject>(KINECT_PREFAB), transform);
			ManagerGO.name = "KinectManager";
			*/
			ManagerGO = Instantiate(Resources.Load<GameObject>(ARF_INTERFACE_PREFAB), transform);
			ManagerGO.name = "ARFInterface";
				//KManager = ManagerGO.GetComponent<KinectManager>();
			Debug.Log("CVInterface: Init platform " + Application.platform + "/ data req = " + dataRequest);
			
			LastResquestedData = dataRequest;
			Session = ManagerGO.GetComponent<ARSession>();
			Session.enabled = false;
			CamManager = ManagerGO.GetComponentInChildren<ARCameraManager>();
			//CamBG = ManagerGO.GetComponentInChildren<ARCameraBackground>();			
			
			if (dataRequest == ExpSettings.DATA_REQUESTED.Body)
			{
				OccManager = ManagerGO.GetComponent<AROcclusionManager>();
				BodyManager = ManagerGO.GetComponent<ARHumanBodyManager>();
				BodyManager.humanBodiesChanged += OnHumanBodiesChanged;

				
				OccManager.enabled = false;
				BodyManager.enabled = true;
			}
			else if (dataRequest == ExpSettings.DATA_REQUESTED.Depth)
			{
				OccManager = ManagerGO.GetComponent<AROcclusionManager>();
				BodyManager = ManagerGO.GetComponent<ARHumanBodyManager>();
				OccManager.requestedHumanDepthMode = HumanSegmentationDepthMode.Fastest;
				OccManager.requestedHumanStencilMode = HumanSegmentationStencilMode.Fastest;
				OccManager.requestedOcclusionPreferenceMode = OcclusionPreferenceMode.PreferEnvironmentOcclusion;
				//OccManager.frameReceived += OnOcclusionFrameReceived;
				OccManager.enabled = true;
				BodyManager.enabled = false;
			}
			else if (dataRequest == ExpSettings.DATA_REQUESTED.Users)
			{
				OccManager = ManagerGO.GetComponent<AROcclusionManager>();
				BodyManager = ManagerGO.GetComponent<ARHumanBodyManager>();
				OccManager.requestedHumanDepthMode = HumanSegmentationDepthMode.Fastest;
				OccManager.requestedHumanStencilMode = HumanSegmentationStencilMode.Fastest;
				OccManager.requestedOcclusionPreferenceMode = OcclusionPreferenceMode.PreferHumanOcclusion;
				//OccManager.frameReceived += OnOcclusionFrameReceived;
				OccManager.enabled = true;
				BodyManager.enabled = false;
			}
			else
			{
				Debug.Log("Probleme avec dataRequest : " + dataRequest);
			}
			Session.enabled = true;
			Session.Reset();
			//CamManager.frameReceived += OnCameraFrameReceived;
			SetDebug(LastDebugValue);

			if (ImgUpdateRoutine == null)
				StartCoroutine(ImgUpdate());

			Initialized = true;
		}


		private static bool NeedCamRefresh = false;
		private static bool NeedUserRefresh = false;
		private static bool NeedDepthRefresh = false;

		private static Texture2D m_CameraTexture;
		private static Texture2D m_UserTexture;
		private static Texture2D m_DepthTexture;

		private static bool CpuImgMirrorX = true;
		private static bool CpuImgMirrorY = true;

		private static float CpuTextureScale = 0.3f; // a régler sur build

		private static IEnumerator ImgUpdate()
		{

			while (true)
			{
				if (NeedCamRefresh)
				{
					UpdateCpuImg(CpuImgType.Camera);
					NeedCamRefresh = false;
				}

				if( NeedUserRefresh)
				{
					UpdateCpuImg(CpuImgType.Users);
					NeedUserRefresh = false;
				}

				if( NeedDepthRefresh)
				{
					UpdateCpuImg(CpuImgType.Depth);
					NeedDepthRefresh = false;
				}
				yield return null;
			}
		}
		enum CpuImgType
		{
			Camera, 
			Users, 
			Depth
		}
		public static TextureFormat getFormat(XRCpuImage cpuImage)
		{
			//#if ARFOUNDATION_4_0_2_OR_NEWER
            var format = cpuImage.format.AsTextureFormat();
            if ((int) format != 0) {
                return format;
            }
			//#endif

			return TextureFormat.ARGB32;
		}

		public struct ConvertedCpuImage
		{
			public ConversionParamsSerializable conversionParams;
			[NotNull] public byte[] bytes;
		}

		

		private static void UpdateCpuImg(CpuImgType ImgType )
		{
			bool imageAcquired;
			XRCpuImage cpuImage;
			switch (ImgType)
			{
				case CpuImgType.Users:
					imageAcquired = OccManager.TryAcquireHumanStencilCpuImage(out cpuImage);
					break;
				case CpuImgType.Depth:
					//imageAcquired = OccManager.TryAcquireEnvironmentDepthConfidenceCpuImage(out cpuImage);
					//imageAcquired = OccManager.TryAcquireRawEnvironmentDepthCpuImage(out cpuImage);
					imageAcquired = OccManager.TryAcquireEnvironmentDepthCpuImage(out cpuImage);
					break;
				case CpuImgType.Camera:
				default:
					imageAcquired = CamManager.TryAcquireLatestCpuImage(out cpuImage);
					break;
			}
			
			if (imageAcquired)
			{
				using (cpuImage)
				{
					var format = getFormat(cpuImage);
					var fullWidth = cpuImage.width;
					var fullHeight = cpuImage.height;
					var downsizedWidth = Mathf.RoundToInt(fullWidth * CpuTextureScale);
					var downsizedHeight = Mathf.RoundToInt(fullHeight * CpuTextureScale);
					var conversionParams = new XRCpuImage.ConversionParams
					{
						transformation = ConversionParamsSerializable.GetTransformation(CpuImgMirrorX, CpuImgMirrorY),
						inputRect = new RectInt(0, 0, fullWidth, fullHeight),
						outputDimensions = new Vector2Int(downsizedWidth, downsizedHeight),
						outputFormat = format
					};

					var convertedDataSize = tryGetConvertedDataSize();
					if (convertedDataSize.HasValue)
					{
						using (var buffer = new NativeArray<byte>(convertedDataSize.Value, Allocator.Temp))
						{
							if (tryConvert())
							{
								loadRawTextureData(buffer);
							}

							bool tryConvert()
							{
								try
								{
									cpuImage.ConvertSync(conversionParams, buffer);
									return true;
								}
								catch (Exception e)
								{
									processException(e);
									return false;
								}
							}
						}
					}

					int? tryGetConvertedDataSize()
					{
						try
						{
							return cpuImage.GetConvertedDataSize(conversionParams);
						}
						catch (Exception e)
						{
							processException(e);
							return null;
						}
					}

					void loadRawTextureData(NativeArray<byte> data)
					{
						switch(ImgType)
						{
							case CpuImgType.Camera:
								if( m_CameraTexture == null)
								{
									m_CameraTexture = new Texture2D(downsizedWidth, downsizedHeight, format, false);
								}
								/*
								if (m_CameraTexture != null)
								{
									Destroy(m_CameraTexture);
									m_CameraTexture = null;
								}
								*/
								
								m_CameraTexture.LoadRawTextureData(data);
								m_CameraTexture.Apply();
								break;
							case CpuImgType.Depth:
								/*
								if (m_DepthTexture != null)
								{
									Destroy(m_DepthTexture);
									m_DepthTexture = null;
								}
								*/
								if( m_DepthTexture == null)
									m_DepthTexture = new Texture2D(downsizedWidth, downsizedHeight, format, false);

								m_DepthTexture.LoadRawTextureData(data);
								m_DepthTexture.Apply();
								break;
							case CpuImgType.Users:
								/*
								if (m_UserTexture != null)
								{
									Destroy(m_UserTexture);
									m_UserTexture = null;
								}
								*/
								if( m_UserTexture == null )
									m_UserTexture = new Texture2D(downsizedWidth, downsizedHeight, format, false);

								m_UserTexture.LoadRawTextureData(data);
								m_UserTexture.Apply();
								break;
						}
						
						//image.texture = texture;
					}

				}
			}
		}

		public static void processException(Exception e)
		{
			Debug.LogWarning(e.ToString());
		}

		

		private TrackableId LastTrackableHumanId = TrackableId.invalidId;
		void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
		{
			Debug.Log("Added = " + eventArgs.added.Count);

			foreach (var humanBody in eventArgs.added)
			{
				LastTrackableHumanId = humanBody.trackableId;
				Debug.Log($"Adding a new skeleton [{humanBody.trackableId}].");
				
				BoneControl = GetComponentInChildren<BoneController>();
				Debug.Log("BoneController = " + BoneControl);
			}
			foreach (var humanBody in eventArgs.updated)
			{
				//Debug.Log($"update skeleton [{humanBody.trackableId}].");
				

				//BoneControl = GetComponentInChildren<BoneController>();
				//Debug.Log("BoneController = " + BoneControl);
			}
			foreach (var humanBody in eventArgs.removed)
			{
				Debug.Log($"Removing a skeleton [{humanBody.trackableId}].");
			}

		}


		private IEnumerator _CheckForManagerRefresh(ExpSettings.DATA_REQUESTED dataResquested)
		{
			Debug.Log("_CheckForManagerRefresh : " + dataResquested + "/ platform = " + Application.platform );

			bool shouldRefresh = (dataResquested != LastResquestedData);

			Debug.Log("ShouldRefresh = " + shouldRefresh);
			if (shouldRefresh)
			{
				if( ImgUpdateRoutine != null)
				{
					StopCoroutine(ImgUpdateRoutine);
					ImgUpdateRoutine = null;
				}
				if (BodyManager != null)
				{
					BodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
					BodyManager = null;
				}

				if( OccManager != null )
				{
					//OccManager.frameReceived -= OnOcclusionFrameReceived;
					OccManager = null;
				}

				Session = null;
				//CamManager.frameReceived -= OnCameraFrameReceived;
				CamManager = null;
				BoneControl = null;

				Debug.Log("We should Refresh CVInterface for " + dataResquested);
				Destroy(ManagerGO);
				ManagerGO = null;
				//KManager = null;
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

			if( AreDatasAvailable() && BoneControl != null )
			{
				
				if ((Time.time - UsersMetaDatas[user].lastUpdateTime) <= (1f / (float)Application.targetFrameRate))
					return;

				Vector3 HandRightPosition = GetJointPos3D(IRISJoints.Joints.HandRight);
				Vector3 HandLeftPosition = GetJointPos3D(IRISJoints.Joints.HandLeft);

				Vector3 ElbowRightPosition = GetJointPos3D(IRISJoints.Joints.ElbowRight);
				Vector3 ElbowLeftPosition = GetJointPos3D(IRISJoints.Joints.ElbowLeft);

				Vector3 PelvisPosition = GetJointPos3D(IRISJoints.Joints.Pelvis);

				UsersMetaDatas[user].UserHorizontalPosition = HandRightPosition.x + (HandLeftPosition.x - HandRightPosition.x) / 2f;// KManager.GetJointPosition(user, KinectInterop.JointType.SpineNaval).x;
				UsersMetaDatas[user].HandsHorizontalSeparation = Mathf.Abs( HandRightPosition.x - HandLeftPosition.x );
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

		private void UpdateUserMetaBoneData2D(ulong user)
		{
			if (!UsersMetaDatas2D.ContainsKey(user))
			{
				UsersMetaDatas2D[user] = new UserBonesMetaData();
			}

			if (AreDatasAvailable() && Bones2D != null)
			{

				if ((Time.time - UsersMetaDatas2D[user].lastUpdateTime) <= (1f / (float)Application.targetFrameRate))
					return;


				/*
				Vector3 HandRightPosition = GetJointPos3D(IRISJoints.Joints.HandRight);
				Vector3 HandLeftPosition = GetJointPos3D(IRISJoints.Joints.HandLeft);

				Vector3 ElbowRightPosition = GetJointPos3D(IRISJoints.Joints.ElbowRight);
				Vector3 ElbowLeftPosition = GetJointPos3D(IRISJoints.Joints.ElbowLeft);

				Vector3 PelvisPosition = GetJointPos3D(IRISJoints.Joints.Pelvis);
				*/


				Vector2 HandRightPosition = GetJointPos2D(IRISJoints.Joints2D.RightHand);
				Vector2 HandLeftPosition = GetJointPos2D(IRISJoints.Joints2D.LeftHand);

				Vector2 ElbowRightPosition = GetJointPos2D(IRISJoints.Joints2D.RightForearm);
				Vector2 ElbowLeftPosition = GetJointPos2D(IRISJoints.Joints2D.LeftForearm);

				Vector2 PelvisPosition = GetJointPos2D(IRISJoints.Joints2D.Root);
				

				UsersMetaDatas2D[user].UserHorizontalPosition = HandRightPosition.x + (HandLeftPosition.x - HandRightPosition.x) / 2f;// KManager.GetJointPosition(user, KinectInterop.JointType.SpineNaval).x;
				UsersMetaDatas2D[user].HandsHorizontalSeparation = Mathf.Abs(HandRightPosition.x - HandLeftPosition.x);
				UsersMetaDatas2D[user].HandsVerticalSeparation = HandRightPosition.y - HandLeftPosition.y;


				float distHandTopRight = Math.Abs(HandRightPosition.y - PelvisPosition.y);
				float distHandTopLeft = Math.Abs(HandLeftPosition.y - PelvisPosition.y);

				UsersMetaDatas2D[user].PelvisToLeftHand = distHandTopLeft;
				UsersMetaDatas2D[user].PelvisToRightHand = distHandTopRight;

				UsersMetaDatas2D[user].HandsToPelvisFactor = distHandTopRight + distHandTopLeft;

				if (ElbowLeftPosition.y < HandLeftPosition.y && ElbowRightPosition.y < HandRightPosition.y)
				{
					UsersMetaDatas2D[user].HandsAboveElbows = true;
				}
				else
				{
					UsersMetaDatas2D[user].HandsAboveElbows = false;
				}

				UsersMetaDatas2D[user].lastUpdateTime = Time.time;
			}
		}


		private byte[] boneData;
		private int NbBones = Enum.GetNames(typeof(IRISJoints.Joints)).Length;
		
		private void UpdateAllBonestexture()
		{

			
			//int userCount = KManager.GetUsersCount();
			int userCount = GetUserCount();

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
				//ulong uid = KManager.GetUserIdByIndex(u);
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
					

					float val;
					byte[] biteval;

					//int invertX = (Application.platform == RuntimePlatform.IPhonePlayer)?-1:1;

					//val = p.x * KManager.GetSensorSpaceScale(0).x * invertX; 
					val = p.x;// * KManager.GetSensorSpaceScale(0).x;
					biteval = BitConverter.GetBytes(val);
					boneData[userDecal + i + 0] = biteval[0];
					boneData[userDecal + i + 1] = biteval[1];
					boneData[userDecal + i + 2] = biteval[2];
					boneData[userDecal + i + 3] = biteval[3];

					val = p.y;// * KManager.GetSensorSpaceScale(0).y;
					biteval = BitConverter.GetBytes(val);
					boneData[userDecal + i + 4] = biteval[0];
					boneData[userDecal + i + 5] = biteval[1];
					boneData[userDecal + i + 6] = biteval[2];
					boneData[userDecal + i + 7] = biteval[3];

					val = p.z;// * KManager.GetSensorSpaceScale(0).z;
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


		private static Vector2[] Bones2D;
		private float lastBones2DUpdate = 0;
		private void Update2DBones()
		{
			if (!AreDatasAvailable() || BodyManager == null || CamManager == null)
				return;

			if (Time.time - lastBones2DUpdate < (1f / 30f))
				return;

			var joints = BodyManager.GetHumanBodyPose2DJoints(Allocator.Temp);
			if (!joints.IsCreated)
			{
				return;
			}
			
			if (Bones2D == null || Bones2D.Length != joints.Length)
			{
				Bones2D = new Vector2[joints.Length];
			}
			Debug.Log("B2L =" + Bones2D.Length);
			using (joints)
			{
				for (int i = joints.Length - 1; i >= 0; --i)
				{
					if (joints[i].parentIndex != -1)
					{
						Vector2 pos = joints[i].position;
						if (float.IsNaN(pos.x) || float.IsNaN(pos.y))
							pos = Vector2.zero;
						pos.x = (pos.x - 0.5f) * -1f + 0.5f;

						Bones2D[i] =pos;
					}
				}
			}

			Update2DBonesTexture();

			lastBones2DUpdate = Time.time;

		}

		private Texture2D AllBones2DTexture;
		private byte[] bone2DData;
		

		private void Update2DBonesTexture()
		{
			if (Bones2D == null)
				return;

			if (Bones2D.Length == 0)
				return;

			if (AllBones2DTexture == null || AllBones2DTexture.width != Bones2D.Length)
			{
				//NbBones = Enum.GetNames(typeof(KinectInterop.JointType)).Length;
				AllBones2DTexture = new Texture2D(Bones2D.Length, 1, TextureFormat.RGBAFloat, false);
				AllBones2DTexture.filterMode = FilterMode.Point;
			}

			var lineLenght = Bones2D.Length * 4 * 4;
			var totalbytes = lineLenght ;
			if (bone2DData == null || bone2DData.Length != totalbytes)
				bone2DData = new byte[totalbytes];

			int i = 0;
			int j = 0;
			int userDecal = 0;
			for ( j=0;j< Bones2D.Length;j++)
			{
				Vector2 p = Bones2D[j];

				float val;
				byte[] biteval;

				val = p.x;
				biteval = BitConverter.GetBytes(val);
				bone2DData[userDecal + i + 0] = biteval[0];
				bone2DData[userDecal + i + 1] = biteval[1];
				bone2DData[userDecal + i + 2] = biteval[2];
				bone2DData[userDecal + i + 3] = biteval[3];

				val = p.y;
				biteval = BitConverter.GetBytes(val);
				bone2DData[userDecal + i + 4] = biteval[0];
				bone2DData[userDecal + i + 5] = biteval[1];
				bone2DData[userDecal + i + 6] = biteval[2];
				bone2DData[userDecal + i + 7] = biteval[3];

				val = 0f;
				biteval = BitConverter.GetBytes(val);
				bone2DData[userDecal + i + 8] = biteval[0];
				bone2DData[userDecal + i + 9] = biteval[1];
				bone2DData[userDecal + i + 10] = biteval[2];
				bone2DData[userDecal + i + 11] = biteval[3];

				val = 1f;
				biteval = BitConverter.GetBytes(val);
				bone2DData[userDecal + i + 12] = biteval[0];
				bone2DData[userDecal + i + 13] = biteval[1];
				bone2DData[userDecal + i + 14] = biteval[2];
				bone2DData[userDecal + i + 15] = biteval[3];

				i += 16;
			}

			AllBones2DTexture.SetPixelData(bone2DData, 0);
			AllBones2DTexture.Apply();
		}

		private void TextureToScreenCoord( Vector2 position )
		{
			// REDO
			/*
			if( KManager != null)
			{
				KManager.MapDepthPointToColorCoords(0, position, 0);
			}*/
		}

		//////////////////////////////////////////
	}

	[Serializable]
	public struct ConversionParamsSerializable
	{
		RectIntSerializable inputRect;
		Vector2IntSerializable outputDimensions;
		TextureFormat format;
		bool mirrorX, mirrorY;


		public static ConversionParamsSerializable Create(XRCameraImageConversionParams _)
		{
			return new ConversionParamsSerializable
			{
				inputRect = RectIntSerializable.Create(_.inputRect),
				outputDimensions = Vector2IntSerializable.Create(_.outputDimensions),
				format = _.outputFormat,
				mirrorX = _.transformation.HasFlag(CameraImageTransformation.MirrorX),
				mirrorY = _.transformation.HasFlag(CameraImageTransformation.MirrorY),
			};
		}

		public XRCameraImageConversionParams Deserialize()
		{
			return new XRCameraImageConversionParams
			{
				transformation = GetTransformation(mirrorX, mirrorY),
				inputRect = inputRect.Deserialize(),
				outputDimensions = outputDimensions.Deserialize(),
				outputFormat = format
			};
		}

		/// CameraImageTransformation enum has changed between different AR Foundation versions
		/// This method makes CameraImageTransformation compatible between different AR Foundation versions
		public static CameraImageTransformation GetTransformation(bool mirrorX, bool mirrorY)
		{
			var result = CameraImageTransformation.None;
			if (mirrorX)
			{
				result |= CameraImageTransformation.MirrorX;
			}

			if (mirrorY)
			{
				result |= CameraImageTransformation.MirrorY;
			}

			return result;
		}

		public override string ToString()
		{
			//return this.AllFieldsAndPropsToString();
			return ToString();
		}
	}

	[Serializable]
	public struct RectIntSerializable
	{
		int x, y, w, h;

		public static RectIntSerializable Create(RectInt _)
		{
			return new RectIntSerializable
			{
				x = _.x,
				y = _.y,
				w = _.width,
				h = _.height
			};
		}

		public RectInt Deserialize()
		{
			return new RectInt(x, y, w, h);
		}
	}


	[Serializable]
	public struct Vector2IntSerializable
	{
		int x, y;

		public static Vector2IntSerializable Create(Vector2Int _)
		{
			return new Vector2IntSerializable
			{
				x = _.x,
				y = _.y
			};
		}

		public Vector2Int Deserialize()
		{
			return new Vector2Int(x, y);
		}
	}

	public static class CpuImageExtensions
	{
		public static
#if !ARFOUNDATION_4_0_2_OR_NEWER
						unsafe
#endif
						void ConvertSync(this XRCpuImage cpuImage, XRCameraImageConversionParams conversionParams, NativeArray<byte> buffer)
		{
			cpuImage.Convert(conversionParams,
#if ARFOUNDATION_4_0_2_OR_NEWER
								buffer);
#else
							new IntPtr(Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafePtr(buffer)), buffer.Length);
#endif
		}
	}
}
