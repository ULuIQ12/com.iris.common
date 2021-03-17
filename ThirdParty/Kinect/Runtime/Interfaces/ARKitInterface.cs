using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using com.rfilkov.arkit;
using Unity.Collections;
using Unity.Jobs;

namespace com.rfilkov.kinect
{
    /// <summary>
    /// ARKitInterface is sensor interface to the Apple's AR-Kit Framework for iPhone (Pro) and iPad (Pro).
    /// </summary>
    public class ARKitInterface : DepthSensorBase
    {
        [Tooltip("Whether to set fixed app frame rate or not.")]
        public bool fixedFrameRate = true;

        [Tooltip("Camera used to render the AR background. If left empty, it will default to the main camera.")]
        private Camera arCamera;

        [Tooltip("Whether to update the main camera's projection matrix or not, according to device's camera projection.")]
        public bool updateCameraProjection = true;

        [Tooltip("ARKit depth mode.")]
        public ArKitDepthMode depthMode = ArKitDepthMode.EnvironmentDepth;
        public enum ArKitDepthMode : int { EnvironmentDepth = 1, HumanDepth = 2 };

        [Tooltip("Environment depth quality mode.")]
        private EnvironmentDepthMode envDepthQuality = EnvironmentDepthMode.Best;

        [Tooltip("Minimum depth confidence.")]
        public MinDepthConfMode minDepthConfidence = MinDepthConfMode.Low;
        public enum MinDepthConfMode : int { Low = 0, Medium = 1, High = 2 };

        [Tooltip("Human depth quality mode.")]
        private HumanSegmentationDepthMode humanDepthQuality = HumanSegmentationDepthMode.Best;

        [Tooltip("Human segmentation quality mode.")]
        private HumanSegmentationStencilMode humanSegmentationQuality = HumanSegmentationStencilMode.Best;

        [Tooltip("Smoothing used by the body tracking.")]
        public SmoothingType bodySmoothing = SmoothingType.Default;

        [Tooltip("UI text to display debug messages.")]
        public UnityEngine.UI.Text debugText;

        [Tooltip("Whether to try to detect the floor plane, to estimate the sensor position and rotation.")]
        public bool detectFloorForPoseEstimation = false;


        // session identifier
        private IntPtr m_sessionId = IntPtr.Zero;

        // requested features
        private ulong m_requestedFeatures = 0;

        // config state
        private bool m_configState = false;  // true;  // false;

        // configuration descriptor
        private ARConfigurationDescriptor? m_config = null;
        private ulong m_configCaps = 0;
        private ulong m_configFeatures = 0;

        // configuration identifier
        private IntPtr m_configId = IntPtr.Zero;

        // available features
        private bool m_avlEnvDepth = false;
        private bool m_avlHumanDepth = false;
        private bool m_avlHumanStencil = false;
        private bool m_avlBodyTracking = false;

        // camera intrinsics
        private ARCameraIntrinsics m_cameraIntrinsics;
        private bool m_gotCameraIntrinsics = false;

        // texture infos
        readonly List<ARTextureInfo> m_cameraTextureInfos = new List<ARTextureInfo>();

        // frame properties
        private ARCameraFrame m_cameraFrame;
        private long m_frameTimestampNs = 0;
        private Matrix4x4 m_projectionMatrix = Matrix4x4.identity;
        private Matrix4x4 m_displayMatrix = Matrix4x4.identity;

        // back-image anchor pos
        private Vector2 m_backImageAnchorPos = Vector2.zero;
        private static readonly Vector3 m_centerSpacePos = new Vector3(0f, 0f, 1f);

        // cpu image api
        private ARCpuImage.CpuImageApi m_cpuImageApi = null;

        // cpu images
        private ARCpuImage m_humanStencilImage;
        //private ARCpuImage m_humanDepthImage;
        private ARCpuImage m_depthConfImage;
        private ARCpuImage m_depthDataImage;

        // depth and stencil data
        private NativeArray<ushort> m_depthDataBuffer;
        private ulong m_depthDataTimestamp = 0;
        private NativeArray<byte> m_bodyIndexDataBuffer;
        private ulong m_bodyIndexTimestamp = 0;

        // depth & stencil data index
        private NativeArray<int> m_depthDataIndex;
        private ScreenOrientation m_depthScreenOri = (ScreenOrientation)0;

        // human body joints
        private bool m_useBodyTracking = false;
        private NativeArray<ARHumanBodyJoint> m_bodyJoints3d;
        private ulong m_bodyFrameTimestamp = 0;

        // input subsystem
        private ISubsystem arKitInput = null;

        // camera pose
        private List<UnityEngine.XR.XRNodeState> m_nodeStates = new List<UnityEngine.XR.XRNodeState>();
        private Pose m_cameraPose = Pose.identity;
        private ulong m_cameraPoseTimestamp = 0;
        private bool m_cameraPoseEnabled = false;
        private Matrix4x4 m_cameraPoseInv = Matrix4x4.identity;

        // background texture and material
        private readonly int k_displayTransform = Shader.PropertyToID("_DisplayTransform");
        private RenderTexture m_arBackTexture = null;
        private Material m_arBackMaterial = null;
        private ulong m_arColorFrameTimestamp = 0;

        // reference to the KinectManager
        private KinectManager kinectManager = null;

        // transformed data index
        private NativeArray<int> m_colorCamDepthDataIndex;
        private int m_colorCamDepthColorW = 0, m_colorCamDepthColorH = 0, m_colorCamDepthDepthW = 0, m_colorCamDepthDepthH = 0;

        // transformed data buffers & textures
        private NativeArray<ushort> m_colorCamDepthDataBuffer;
        private NativeArray<byte> m_colorCamBodyIndexDataBuffer;

        private RenderTexture m_depthCamColorTexture = null;
        private Material m_depthCamColorMaterial = null;
        private int m_depthCamColorColorW = 0, m_depthCamColorColorH = 0, m_depthCamColorDepthW = 0, m_depthCamColorDepthH = 0;

        // joint position filter
        private JointPositionsFilter jointPositionFilter = null;

        // floor detector
        public KinectFloorDetector m_floorDetector = null;


        // important features
        private const int FEAT_ENV_DEPTH = 1 << 25;      // EnvironmentDepth
        private const int FEAT_HUMAN_DEPTH = 1 << 13;    // HumanDepth
        private const int FEAT_HUMAN_STENCIL = 1 << 12;  // HumanStencil
        private const int FEAT_BODY_TRACKING = 1 << 10;  // BodyTracking
        //private const int FEAT_BODY_SCALING = 1 << 11;  // BodyScaling


        // depth sensor settings
        [System.Serializable]
        public class ARKitSettings : DepthSensorBase.BaseSensorSettings
        {
            public bool fixedFrameRate;
            public bool updateCameraProjection;
            public int depthMode;
            public int minDepthConfidence;
            public int bodySmoothing;
            public bool detectFloorForPoseEstimation;
        }



        public override KinectInterop.DepthSensorPlatform GetSensorPlatform()
        {
            return KinectInterop.DepthSensorPlatform.ARKit;
        }


        public override System.Type GetSensorSettingsType()
        {
            return typeof(ARKitSettings);
        }


        public override BaseSensorSettings GetSensorSettings(BaseSensorSettings settings)
        {
            if (settings == null)
            {
                settings = new ARKitSettings();
            }

            ARKitSettings extSettings = (ARKitSettings)base.GetSensorSettings(settings);

            extSettings.fixedFrameRate = fixedFrameRate;
            extSettings.updateCameraProjection = updateCameraProjection;
            extSettings.depthMode = (int)depthMode;
            extSettings.minDepthConfidence = (int)minDepthConfidence;
            extSettings.bodySmoothing = (int)bodySmoothing;
            extSettings.detectFloorForPoseEstimation = detectFloorForPoseEstimation;

            return settings;
        }

        public override void SetSensorSettings(BaseSensorSettings settings)
        {
            if (settings == null)
                return;

            base.SetSensorSettings(settings);

            ARKitSettings extSettings = (ARKitSettings)settings;
            fixedFrameRate = extSettings.fixedFrameRate;
            updateCameraProjection = extSettings.updateCameraProjection;
            depthMode = (ArKitDepthMode)extSettings.depthMode;
            minDepthConfidence = (MinDepthConfMode)extSettings.minDepthConfidence;
            bodySmoothing = (SmoothingType)extSettings.bodySmoothing;
            detectFloorForPoseEstimation = extSettings.detectFloorForPoseEstimation;
        }


        public override List<KinectInterop.SensorDeviceInfo> GetAvailableSensors()
        {
            List<KinectInterop.SensorDeviceInfo> alSensorInfo = new List<KinectInterop.SensorDeviceInfo>();

            if (NativeApi.AtLeast13_0())
            {
                KinectInterop.SensorDeviceInfo sensorInfo = new KinectInterop.SensorDeviceInfo();
                sensorInfo.sensorId = SystemInfo.deviceUniqueIdentifier;
                sensorInfo.sensorName = SystemInfo.deviceName;

                sensorInfo.sensorCaps = KinectInterop.FrameSource.TypeColor | KinectInterop.FrameSource.TypeDepth |
                    KinectInterop.FrameSource.TypeBodyIndex | KinectInterop.FrameSource.TypeBody | KinectInterop.FrameSource.TypePose;

                if (consoleLogMessages)
                    Debug.Log(string.Format("  D{0}: {1}, id: {2}", 0, sensorInfo.sensorName, sensorInfo.sensorId));

                alSensorInfo.Add(sensorInfo);
            }
            else
            {
                Debug.LogWarning("ARKitInterface needs at least iOS v13.0!");
            }

            //if (alSensorInfo.Count == 0)
            //{
            //    Debug.Log("  No ARKit device found.");
            //}

            return alSensorInfo;
        }


        public override KinectInterop.SensorData OpenSensor(KinectManager kinectManager, KinectInterop.FrameSource dwFlags, bool bSyncDepthAndColor, bool bSyncBodyAndDepth)
        {
            // workaround - always enable the color frames
            dwFlags |= KinectInterop.FrameSource.TypeColor;

            // save initial parameters
            base.OpenSensor(kinectManager, dwFlags, bSyncDepthAndColor, bSyncBodyAndDepth);
            this.kinectManager = kinectManager;

            string sensorName = "ARKit";
            KinectInterop.FrameSource sensorCaps = KinectInterop.FrameSource.TypeNone;

            try
            {
                if(deviceStreamingMode == KinectInterop.DeviceStreamingMode.PlayRecording)
                {
                    Debug.LogError("ARKit recordings are not currently supported.");
                    return null;
                }
                else
                {
                    // get the list of available sensors
                    List<KinectInterop.SensorDeviceInfo> alSensors = GetAvailableSensors();
                    if (deviceIndex >= alSensors.Count)
                    {
                        Debug.LogError("  D" + deviceIndex + " is not available. You can set the device index to -1, to disable it.");
                        return null;
                    }

                    // sensor serial number
                    sensorPlatform = KinectInterop.DepthSensorPlatform.ARKit;
                    sensorDeviceId = alSensors[deviceIndex].sensorId;
                    sensorName = alSensors[deviceIndex].sensorName;

                    // set the root view
                    NativeApi.EnsureRootViewIsSetup();

                    // construct session
                    m_sessionId = NativeApi.UnityARKit_Session_Construct();

                    // construct camera
                    NativeApi.UnityARKit_Camera_Construct(Shader.PropertyToID("_textureY"), Shader.PropertyToID("_textureCbCr"));

                    // construct occlusion
                    NativeApi.UnityARKit_OcclusionProvider_Construct(Shader.PropertyToID("_HumanStencil"),
                        Shader.PropertyToID("_HumanDepth"), Shader.PropertyToID("_EnvironmentDepth"),
                        Shader.PropertyToID("_EnvironmentDepthConfidence"));

                    // create cpu-image api
                    m_cpuImageApi = new ARCpuImage.CpuImageApi();

                    // request features
                    ulong feature = 0;
                    m_requestedFeatures = 0;

                    // request camera features
                    feature = 1 << 3; // PositionAndRotation
                    NativeApi.SetFeatureRequested(feature, true);
                    m_requestedFeatures |= feature;

                    feature = 1;  // WorldFacingCamera
                    NativeApi.SetFeatureRequested(feature, true);
                    m_requestedFeatures |= feature;

                    feature = 1 << 15;  // AutoFocus
                    NativeApi.SetFeatureRequested(feature, true);
                    m_requestedFeatures |= feature;

                    // color
                    if ((dwFlags & KinectInterop.FrameSource.TypeColor) != 0)
                    {
                        // AR background
                        Shader arBackShader = Shader.Find("Kinect/ARKitBackgroundShader");
                        if (arBackShader != null)
                        {
                            m_arBackMaterial = new Material(arBackShader);
                            //Debug.Log("Created color material with shader: Kinect/ARKitBackgroundShader");
                        }
                    }

                    // depth
                    if ((dwFlags & KinectInterop.FrameSource.TypeDepth) != 0)
                    {
                        if (depthMode == ArKitDepthMode.EnvironmentDepth && NativeApi.UnityARKit_OcclusionProvider_DoesSupportEnvironmentDepth())
                        {
                            NativeApi.UnityARKit_OcclusionProvider_SetRequestedEnvironmentDepthMode(envDepthQuality);  // Fastest=1

                            feature = FEAT_ENV_DEPTH;  // EnvironmentDepth
                            NativeApi.SetFeatureRequested(feature, true);
                            m_requestedFeatures |= feature;
                        }
                        else if(depthMode == ArKitDepthMode.HumanDepth && NativeApi.UnityARKit_OcclusionProvider_DoesSupportBodySegmentationDepth())
                        {
                            NativeApi.UnityARKit_OcclusionProvider_SetRequestedSegmentationDepthMode(humanDepthQuality);  // Fastest=1

                            feature = FEAT_HUMAN_DEPTH;  // HumanDepth
                            NativeApi.SetFeatureRequested(feature, true);
                            m_requestedFeatures |= feature;
                        }
                    }

                    // body-index
                    if ((dwFlags & KinectInterop.FrameSource.TypeBodyIndex) != 0)
                    {
                        if (NativeApi.UnityARKit_OcclusionProvider_DoesSupportBodySegmentationStencil())
                        {
                            NativeApi.UnityARKit_OcclusionProvider_SetRequestedSegmentationStencilMode(humanSegmentationQuality);  // Fastest=1

                            feature = FEAT_HUMAN_STENCIL;  // HumanStencil
                            NativeApi.SetFeatureRequested(feature, true);
                            m_requestedFeatures |= feature;
                        }
                    }

                    // body
                    if ((dwFlags & KinectInterop.FrameSource.TypeBody) != 0)
                    {
                        NativeApi.UnityARKit_HumanBodyProvider_Construct();
                        m_useBodyTracking = true;
                        //m_configState = true;

                        if (NativeApi.UnityARKit_HumanBodyProvider_DoesSupportBodyPose3DEstimation())
                        {
                            feature = FEAT_BODY_TRACKING;  // BodyTracking
                            NativeApi.SetFeatureRequested(feature, true);
                            m_requestedFeatures |= feature;

                            //feature = FEAT_BODY_SCALING;  // BodyScaling
                            //NativeApi.SetFeatureRequested(feature, true);
                            //m_requestedFeatures |= feature;
                        }
                    }

                    // pose
                    m_cameraPoseEnabled = (dwFlags & KinectInterop.FrameSource.TypePose) != 0;

                    //if (m_cameraPoseEnabled)
                    {
                        List<ISubsystemDescriptor> m_subDescriptors = new List<ISubsystemDescriptor>();
                        SubsystemManager.GetAllSubsystemDescriptors(m_subDescriptors);

                        for (int i = 0; i < m_subDescriptors.Count; i++)
                        {
                            ISubsystemDescriptor descr = m_subDescriptors[i];
                            //Debug.Log("Subsystem " + i + " - name: " + descr.id);

                            if (arKitInput == null && descr.id == "ARKit-Input")
                            {
                                arKitInput = descr.Create();
                                arKitInput.Start();
                            }
                        }
                    }

                    // start session
                    NativeApi.UnityARKit_Session_Resume(m_sessionId);

                    // get the best matching session configurations
                    m_config = DetermineSessionConfiguration(m_requestedFeatures);
                    m_configCaps = 0;

                    if (m_config.HasValue)
                    {
                        m_configId = m_config.Value.identifier;
                        m_configCaps = m_config.Value.capabilities;
                        m_configFeatures = m_configCaps & m_requestedFeatures;
                    }
                    else
                    {
                        m_configId = IntPtr.Zero;
                    }

                    // available features
                    m_avlEnvDepth = (m_configFeatures & FEAT_ENV_DEPTH) != 0;  // && !m_useBodyTracking;  // EnvironmentDepth
                    m_avlHumanDepth = (m_configFeatures & FEAT_HUMAN_DEPTH) != 0;  // && !m_useBodyTracking;  // HumanDepth
                    m_avlHumanStencil = (m_configFeatures & FEAT_HUMAN_STENCIL) != 0;  // && !m_useBodyTracking;  // HumanStencil
                    m_avlBodyTracking = (m_configFeatures & FEAT_BODY_TRACKING) != 0;  // && m_useBodyTracking;  // BodyTracking

                    sensorCaps = KinectInterop.FrameSource.TypeColor | KinectInterop.FrameSource.TypePose;
                    if (m_avlEnvDepth || m_avlHumanDepth)
                        sensorCaps |= KinectInterop.FrameSource.TypeDepth;
                    if (m_avlHumanStencil)
                        sensorCaps |= KinectInterop.FrameSource.TypeBodyIndex;
                    if (m_avlBodyTracking)
                        sensorCaps |= KinectInterop.FrameSource.TypeBody;

                    //Debug.Log("Available eDepth: " + m_avlEnvDepth + ", hDepth: " + m_avlHumanDepth + ", hStencil: " + m_avlHumanStencil + ", bTracking: " + m_avlBodyTracking +
                    //    ", caps: " + m_configCaps.ToString("X") + ", req: " + m_requestedFeatures.ToString("X") + ", avl: " + m_configFeatures.ToString("X"));

                    // start camera
                    NativeApi.UnityARKit_Camera_Start();

                    // start occlusion
                    NativeApi.UnityARKit_OcclusionProvider_Start();

                    // start body tracking
                    if (m_useBodyTracking)
                    {
                        NativeApi.UnityARKit_HumanBodyProvider_Start();

                        // init the joint positions filter
                        jointPositionFilter = new JointPositionsFilter();
                        jointPositionFilter.Init(bodySmoothing);
                    }

                    //if(m_arBackMaterial != null && arCamera == null)
                    //{
                    //    arCamera = Camera.main;
                    //}

                    // fix the frame rate
                    int arFps = NativeApi.UnityARKit_Session_GetFrameRate(m_sessionId);
                    if(fixedFrameRate && arFps > 0)
                    {
                        Application.targetFrameRate = arFps;
                        //Debug.Log("ARKit-FPS: " + arFps);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("ARKitInterface: " + ex.ToString());
            }

            // check if the session was successfully created
            if(m_sessionId == IntPtr.Zero)
                return null;

            KinectInterop.SensorData sensorData = new KinectInterop.SensorData();
            sensorData.sensorIntPlatform = sensorPlatform;

            sensorData.sensorId = sensorDeviceId;
            sensorData.sensorName = sensorName;
            sensorData.sensorCaps = sensorCaps;

            // flip color & depth images vertically
            sensorData.colorImageScale = new Vector3(1f, 1f, 1f);
            sensorData.depthImageScale = new Vector3(1f, 1f, 1f);
            sensorData.infraredImageScale = new Vector3(1f, 1f, 1f);
            sensorData.sensorSpaceScale = new Vector3(1f, 1f, 1f);
            sensorData.unitToMeterFactor = 1f;

            // depth camera offset & matrix z-flip
            sensorRotOffset = Vector3.zero;   // if for instance the depth camera is tilted downwards
            sensorRotFlipZ = false;
            sensorRotIgnoreY = false;

            // don't get all frames
            getAllSensorFrames = false;

            // set ar-camera to be the main camera in the scene
            //arCamera = Camera.main;
            //Debug.Log("arCamera: " + arCamera);

            if (consoleLogMessages)
                Debug.Log("D" + deviceIndex + " ARKit-sensor opened: " + sensorDeviceId);

            return sensorData;
        }

        // determines the configuration to use
        private ARConfigurationDescriptor? DetermineSessionConfiguration(ulong reqFeats)
        {
            //Debug.Log("\n\nLooking for configuration supporting features 0x" + reqFeats.ToString("X") + ", count: " + GetFeatCount(reqFeats));
            var descriptors = ARKitHelper.GetSessionConfigurations(m_sessionId, Allocator.Temp);

            if (descriptors.IsCreated)
            {
                using (descriptors)
                {
                    int[] di = new int[descriptors.Length];
                    int[] dc = new int[descriptors.Length];
                    int[] dr = new int[descriptors.Length];

                    int diLen = 0;
                    bool reqBodyTracking = (reqFeats & FEAT_BODY_TRACKING) != 0;

                    for (int i = 0; i < descriptors.Length; i++)
                    {
                        var descriptor = descriptors[i];
                        //Debug.Log("  " + i + ". Config descrId: 0x" + descriptor.identifier.ToString("X") + ", features: 0x" + descriptor.capabilities.ToString("X") + ", rank: " + descriptor.rank);

                        if((reqBodyTracking && ((descriptor.capabilities & FEAT_BODY_TRACKING) != 0)) ||
                            (!reqBodyTracking && ((descriptor.capabilities & reqFeats) != 0)))
                        {
                            di[diLen] = i;
                            dc[diLen] = GetFeatCount(descriptor.capabilities & reqFeats);
                            dr[diLen] = descriptor.rank;

                            //Debug.Log("    Consider descriptor " + di[diLen] + " - reqFeats: 0x" + (descriptor.capabilities & reqFeats).ToString("X") + ", count: " + dc[diLen] + ", rank: " + dr[diLen]);
                            diLen++;
                        }
                    }

                    int si = -1, sc = 0, sr = -1000;
                    for(int i = 0; i < diLen; i++)
                    {
                        if(dc[i] > sc || (dc[i] == sc && dr[i] > sr))
                        {
                            si = di[i];
                            sc = dc[i];
                            sr = dr[i];
                        }
                    }

                    if(si >= 0)
                    {
                        //Debug.Log("Selected descrId: 0x" + descriptors[si].identifier.ToString("X") + ", features: 0x" + descriptors[si].capabilities.ToString("X") + ", rank: " + descriptors[si].rank);
                        return descriptors[si];
                    }
                }
            }

            Debug.LogWarning("Configuration not found for features 0x" + reqFeats.ToString("X"));
            return null;
        }


        // counts the number of features in feat
        private int GetFeatCount(ulong feat)
        {
            int iCount = 0;

            for(int i = 0; i < 64; i++)
            {
                if ((feat & 1) != 0)
                    iCount++;

                feat >>= 1;
                if (feat == 0)
                    break;
            }

            return iCount;
        }


        public override void CloseSensor(KinectInterop.SensorData sensorData)
        {
            base.CloseSensor(sensorData);

            if (m_sessionId != IntPtr.Zero)
            {
                // dispose body tracking
                if (m_useBodyTracking)
                {
                    NativeApi.UnityARKit_HumanBodyProvider_Stop();
                    NativeApi.UnityARKit_HumanBodyProvider_Destruct();
                }

                // stop occlusion
                NativeApi.UnityARKit_OcclusionProvider_Stop();
                NativeApi.UnityARKit_OcclusionProvider_Destruct();

                // stop camera
                NativeApi.UnityARKit_Camera_Stop();
                NativeApi.UnityARKit_Camera_Destruct();

                // stop session
                NativeApi.UnityARKit_Session_Pause(m_sessionId);

                NativeApi.CFRelease(ref m_sessionId);
            }

            // dispose cpu images
            if (m_humanStencilImage.valid)
                m_humanStencilImage.Dispose();
            //if (m_humanDepthImage.valid)
            //    m_humanDepthImage.Dispose();
            if (m_depthConfImage.valid)
                m_depthConfImage.Dispose();
            if (m_depthDataImage.valid)
                m_depthDataImage.Dispose();

            // dispose camera textures
            foreach (var textureInfo in m_cameraTextureInfos)
            {
                textureInfo.Dispose();
            }

            m_cameraTextureInfos.Clear();

            // dispose body joints
            if (m_bodyJoints3d.IsCreated)
            {
                m_bodyJoints3d.Dispose();
            }

            if (arKitInput != null)
            {
                arKitInput.Destroy();
                arKitInput = null;
            }

            // AR background
            if (m_arBackTexture)
            {
                m_arBackTexture.Release();
                m_arBackTexture = null;
            }

            m_arBackMaterial = null;

            // dispose data buffers
            m_depthDataBuffer.Dispose();
            m_bodyIndexDataBuffer.Dispose();

            m_colorCamDepthDataIndex.Dispose();
            //m_depthCamColorDataIndex.Dispose();
            m_colorCamDepthDataBuffer.Dispose();
            m_colorCamBodyIndexDataBuffer.Dispose();

            if(m_depthCamColorTexture != null)
            {
                m_depthCamColorTexture.Release();
                m_depthCamColorTexture = null;
            }

            if(m_floorDetector != null)
            {
                m_floorDetector.FinishFloorDetector();
                m_floorDetector = null;
            }

            if (consoleLogMessages)
                Debug.Log("D" + deviceIndex + " ARKit-sensor closed: " + sensorDeviceId);
        }


        public override void EnablePoseStream(KinectInterop.SensorData sensorData, bool bEnable)
        {
            base.EnablePoseStream(sensorData, bEnable);
            m_cameraPoseEnabled = (frameSourceFlags & KinectInterop.FrameSource.TypePose) != 0;

            if(m_cameraPoseEnabled)
            {
                if(detectFloorForPoseEstimation && m_floorDetector == null)
                {
                    m_floorDetector = new KinectFloorDetector();
                    m_floorDetector.InitFloorDetector(this, sensorData, MAX_DEPTH_DISTANCE_MM);
                }
            }
            else
            {
                if(m_floorDetector != null)
                {
                    m_floorDetector.FinishFloorDetector();
                    m_floorDetector = null;
                }
            }
        }


        public override bool UpdateSensorData(KinectInterop.SensorData sensorData, KinectManager kinectManager, bool isPlayMode)
        {
            if (m_sessionId == IntPtr.Zero)
                return false;

            //Debug.Log("m_configState: " + m_configState + ", m_configId: " + m_configId.ToString("X") + ", m_configFeatures: " + m_configFeatures.ToString("X"));

            // update the session
            if (!m_configState && m_configId != IntPtr.Zero)
            {
                //m_configState = !m_configState;
                NativeApi.UnityARKit_Session_Update(m_sessionId, m_configId, m_configFeatures);
            }
            else
            {
                //m_configState = !m_configState;
                return false;
            }

            // check the session tracking state
            if (NativeApi.UnityARKit_Session_GetTrackingState(m_sessionId) != (int)TrackingState.Tracking)
                return false;

            // get the available AR-Kit frames
            GetARKitFrames(sensorData);
            //Debug.Log("GetARKitFrames() completed, bodyTimestamp: " + m_bodyFrameTimestamp);

            // color frame
            if (sensorData.lastColorFrameTime != m_arColorFrameTimestamp && !isPlayMode)
            {
                //lock (colorFrameLock)
                {
                    // sensorData.colorImageTexture is already set
                    sensorData.lastColorFrameTime = currentColorTimestamp = rawColorTimestamp = m_arColorFrameTimestamp;
                    //Debug.Log("D" + deviceIndex + " ARKit-UpdateColorTimestamp: " + currentColorTimestamp + ", Now: " + DateTime.Now.ToString("HH:mm:ss.fff"));
                }
            }

            // depth frame
            if (sensorData.lastDepthFrameTime != m_depthDataTimestamp && !isPlayMode)
            {
                //lock (depthFrameLock)
                {
                    // depth image
                    if (sensorData.depthImage != null && m_depthDataBuffer != null && sensorData.depthImage.Length == m_depthDataBuffer.Length)
                    {
                        m_depthDataBuffer.CopyTo(sensorData.depthImage);
                    }

                    sensorData.lastDepthFrameTime = currentDepthTimestamp = rawDepthTimestamp = m_depthDataTimestamp;
                    //Debug.Log("D" + deviceIndex + " ARKit-UpdateDepthTimestamp: " + currentDepthTimestamp + ", Now: " + DateTime.Now.ToString("HH:mm:ss.fff"));
                }
            }

            // body index frame
            if (sensorData.lastBodyIndexFrameTime != m_bodyIndexTimestamp)
            {
                //lock (bodyTrackerLock)
                {
                    // body index image
                    if (sensorData.bodyIndexImage != null && m_bodyIndexDataBuffer != null && sensorData.bodyIndexImage.Length == m_bodyIndexDataBuffer.Length)
                    {
                        m_bodyIndexDataBuffer.CopyTo(sensorData.bodyIndexImage);
                    }

                    sensorData.lastBodyIndexFrameTime = currentBodyIndexTimestamp = rawBodyIndexTimestamp = m_bodyIndexTimestamp;
                    //Debug.Log("D" + deviceIndex + " ARKit-UpdateBodyIndexTimestamp: " + currentBodyIndexTimestamp + ", Now: " + DateTime.Now.ToString("HH:mm:ss.fff"));
                }
            }

            // body frame
            if(sensorData.lastBodyFrameTime != m_bodyFrameTimestamp)
            {
                //lock (bodyTrackerLock)
                {
                    currentBodyTimestamp = rawBodyTimestamp = m_bodyFrameTimestamp;
                    //Debug.Log("D" + deviceIndex + " ARKit-UpdateBodyTimestamp: " + currentBodyTimestamp + ", BodyCount: " + trackedBodiesCount + ", Now: " + DateTime.Now.ToString("HH:mm:ss.fff"));
                }
            }

            if(m_cameraPoseEnabled && sensorData.lastSensorPoseFrameTime != m_cameraPoseTimestamp)
            {
                if(m_floorDetector != null && sensorData.depthImage != null)
                {
                    // update min-point-count according to the depth image width
                    m_floorDetector.minFloorPointCount = sensorData.depthImageWidth;

                    if (m_floorDetector.UpdateFloorDetector(sensorData.depthImage, sensorData.lastDepthFrameTime, ref depthFrameLock, minDistance, maxDistance))
                    {
                        Vector3 vSensorPosition = m_floorDetector.GetSensorPosition();
                        vSensorPosition = new Vector3(m_cameraPose.position.x, vSensorPosition.y, m_cameraPose.position.z);
                        Quaternion qSensorRotation = m_floorDetector.GetSensorRotation();

                        //if (sensorRotFlipZ)
                        //{
                        //    Vector3 vSensorRotEuler = qSensorRotation.eulerAngles;
                        //    vSensorRotEuler.z = -vSensorRotEuler.z;
                        //    qSensorRotation = Quaternion.Euler(vSensorRotEuler);
                        //}

                        //lock(poseFrameLock)
                        {
                            rawPosePosition = vSensorPosition - initialPosePosition;
                            rawPoseRotation = Quaternion.Euler(-sensorRotOffset) * qSensorRotation;
                        }
                    }
                }
                else
                {
                    //lock (poseFrameLock)
                    {
                        rawPosePosition = m_cameraPose.position;
                        rawPoseRotation = m_cameraPose.rotation;
                    }
                }

                currentPoseTimestamp = rawPoseTimestamp = m_cameraPoseTimestamp;
            }

            // update the rest of sensor data (body & pose)
            //ulong lastSensorPoseFrameTime = sensorData.lastSensorPoseFrameTime;
            base.UpdateSensorData(sensorData, kinectManager, isPlayMode);

            return true;
        }


        // get the available AR-Kit frames
        private void GetARKitFrames(KinectInterop.SensorData sensorData)
        {
            try
            {
                // make ar-camera to be the main camera in the scene, if not set already
                if (arCamera == null)
                {
                    arCamera = Camera.main;
                }

                if ((!m_gotCameraIntrinsics || sensorData.colorImageWidth != Screen.width || sensorData.colorImageHeight != Screen.height) &&
                    NativeApi.UnityARKit_Camera_TryGetIntrinsics(out m_cameraIntrinsics))
                {
                    m_gotCameraIntrinsics = true;
                    //Debug.Log("  Intrinsics - fLength: " + m_cameraIntrinsics.focalLength + ", pPoint: " + m_cameraIntrinsics.principalPoint + ", res: " + m_cameraIntrinsics.resolution);

                    sensorData.colorImageWidth = Screen.width;  // m_cameraIntrinsics.resolution.x;
                    sensorData.colorImageHeight = Screen.height;  // m_cameraIntrinsics.resolution.y;

                    sensorData.colorImageFormat = TextureFormat.RGB24;
                    sensorData.colorImageStride = 3;  // 3 bytes per pixel

                    float fx, fy, ppx, ppy, flx, fly;
                    if(sensorData.colorImageWidth < sensorData.colorImageHeight)
                    {
                        // portrait
                        fx = (float)sensorData.colorImageWidth / (float)m_cameraIntrinsics.resolution.y;
                        fy = (float)sensorData.colorImageHeight / (float)m_cameraIntrinsics.resolution.x;

                        ppx = m_cameraIntrinsics.principalPoint.y * fx;
                        ppy = m_cameraIntrinsics.principalPoint.x * fy;

                        flx = m_cameraIntrinsics.focalLength.y * fx;
                        fly = m_cameraIntrinsics.focalLength.x * fy;
                    }
                    else
                    {
                        // landscape
                        fx = (float)sensorData.colorImageWidth / (float)m_cameraIntrinsics.resolution.x;
                        fy = (float)sensorData.colorImageHeight / (float)m_cameraIntrinsics.resolution.y;

                        ppx = m_cameraIntrinsics.principalPoint.x * fx;
                        ppy = m_cameraIntrinsics.principalPoint.y * fy;

                        flx = m_cameraIntrinsics.focalLength.x * fx;
                        fly = m_cameraIntrinsics.focalLength.y * fy;
                    }

                    GetCameraIntrinsics(KinectInterop.DistortionType.InverseBrownConrady, sensorData.colorImageWidth, sensorData.colorImageHeight,
                        ppx, ppy, flx, fly, ref sensorData.colorCamIntr, 1);
                    //Debug.Log("Updated color-cam intrinsics for screenOri: " + Screen.orientation + ", colorW: " + sensorData.colorImageWidth + ", colorH: " + sensorData.colorImageHeight +
                    //    ", fx: " + fx + ", fy: " + fy + "\nres: " + m_cameraIntrinsics.resolution + ", ppt: " + m_cameraIntrinsics.principalPoint + ", flen: " + m_cameraIntrinsics.focalLength);
                }

                // get the latest camera frame
                bool bGotCameraFrame = false;
                if (m_gotCameraIntrinsics)
                {
                    bGotCameraFrame = GetLatestCameraFrame(arCamera, sensorData);
                    //Debug.Log("Got camera frame: " + bGotCameraFrame + ", camera: " + arCamera);
                }

                // update sensor pose
                ulong camFrameTimestamp = (ulong)m_frameTimestampNs / 100;
                bool bUpdatedCamPose = UpdateCameraPose(camFrameTimestamp);

                if (bUpdatedCamPose)
                {
                    // get sensor pose inv
                    m_cameraPoseInv.SetTRS(m_cameraPose.position, m_cameraPose.rotation, Vector3.one);
                    m_cameraPoseInv = m_cameraPoseInv.inverse;
                }

                if (bGotCameraFrame && updateCameraProjection && arCamera != null /**&& m_arBackMaterial != null*/)
                {
                    // set camera projection matrix
                    arCamera.projectionMatrix = m_projectionMatrix;

                    Vector2 imgCenter = new Vector2(sensorData.colorImageWidth >> 1, sensorData.colorImageHeight >> 1);
                    Vector2 projCenter = ProjectPoint(sensorData.colorCamIntr, m_centerSpacePos);
                    m_backImageAnchorPos = (imgCenter - projCenter);
                }

                // depth data
                ulong lastDepthFrameTime = m_depthDataTimestamp;
                if ((m_avlEnvDepth && ARKitHelper.TryAcquireEnvDepthDataCpuImage(m_cpuImageApi, out m_depthDataImage)) ||
                    m_avlHumanDepth && ARKitHelper.TryAcquireHumanDepthCpuImage(m_cpuImageApi, out m_depthDataImage))
                {
                    //sensorData.depthImageWidth = m_depthDataImage.width;
                    //sensorData.depthImageHeight = m_depthDataImage.height;

                    int dataBufferLen = m_depthDataImage.width * m_depthDataImage.height;
                    if (sensorData.depthImage == null && (frameSourceFlags & KinectInterop.FrameSource.TypeDepth) != 0)
                    {
                        //rawDepthImage = new ushort[dataBufferLen];
                        sensorData.depthImage = new ushort[dataBufferLen];
                    }

                    if (sensorData.depthCamIntr == null && m_gotCameraIntrinsics)
                    {
                        float fFactor = (float)m_depthDataImage.width / (float)m_cameraIntrinsics.resolution.x;

                        GetCameraIntrinsics(KinectInterop.DistortionType.BrownConrady, m_depthDataImage.width, m_depthDataImage.height,
                            m_cameraIntrinsics.principalPoint.x * fFactor, m_cameraIntrinsics.principalPoint.y * fFactor, m_cameraIntrinsics.focalLength.x * fFactor, m_cameraIntrinsics.focalLength.y * fFactor,
                            ref sensorData.depthCamIntr, 0);
                    }

                    if (!m_depthDataBuffer.IsCreated)
                    {
                        // create depth data buffer, as needed
                        m_depthDataBuffer = new NativeArray<ushort>(dataBufferLen, Allocator.Persistent);
                    }

                    if(!m_depthDataIndex.IsCreated)
                    {
                        // create depth data index
                        m_depthDataIndex = new NativeArray<int>(dataBufferLen, Allocator.Persistent);
                    }

                    if(m_depthScreenOri != Screen.orientation)
                    {
                        // update depth data index, if needed
                        m_depthScreenOri = Screen.orientation;
                        UpdateDepthDataIndex(m_depthScreenOri, m_depthDataImage.width, m_depthDataImage.height, sensorData);

                        // check for depth texture
                        if (kinectManager.getDepthFrames == KinectManager.DepthTextureType.DepthTexture)
                        {
                            KinectInterop.InitSensorData(sensorData, kinectManager);
                        }
                    }

                    // copy device depth data to the ushorts-buffer
                    if (m_avlEnvDepth)
                    {
                        if (minDepthConfidence == MinDepthConfMode.Low || ARKitHelper.TryAcquireEnvDepthConfCpuImage(m_cpuImageApi, out m_depthConfImage))
                        {
                            CopyDepthDataToBuffer(m_depthDataImage.timestamp);

                            if(m_depthConfImage.valid)
                                m_depthConfImage.Dispose();
                        }
                    }
                    else if (m_avlHumanDepth)
                    {
                        CopyHumanDepthToBuffer(m_depthDataImage.timestamp);
                    }

                    m_depthDataImage.Dispose();
                }
                else if(m_avlEnvDepth || m_avlHumanDepth)
                {
                    Debug.Log("TryAcquireEnvDepthData failed.");
                }

                // color camera image
                if (bGotCameraFrame && m_arBackMaterial != null)  // &&
                    //(!isSyncDepthAndColor || (!m_avlEnvDepth && !m_avlHumanDepth) || (m_depthDataTimestamp != lastDepthFrameTime)))
                {
                    UpdateColorCameraTexture(sensorData);
                    //Debug.Log("UpdateColorCameraTexture() done.");
                }

                // human stencil cpu image
                if (m_avlHumanStencil && ARKitHelper.TryAcquireHumanStencilCpuImage(m_cpuImageApi, out m_humanStencilImage))
                {
                    int dataBufferLen = m_humanStencilImage.width * m_humanStencilImage.height;

                    if (sensorData.bodyIndexImage == null && (frameSourceFlags & KinectInterop.FrameSource.TypeBodyIndex) != 0)
                    {
                        //rawBodyIndexImage = new byte[sensorData.depthImageWidth * sensorData.depthImageHeight];
                        sensorData.bodyIndexImage = new byte[dataBufferLen];
                    }

                    if (!m_bodyIndexDataBuffer.IsCreated)
                    {
                        // create depth data buffer, as needed
                        m_bodyIndexDataBuffer = new NativeArray<byte>(dataBufferLen, Allocator.Persistent);
                    }

                    if (!m_depthDataIndex.IsCreated)
                    {
                        // create depth data index
                        m_depthDataIndex = new NativeArray<int>(dataBufferLen, Allocator.Persistent);
                    }

                    if (m_depthScreenOri != Screen.orientation)
                    {
                        // update depth data index, if needed
                        m_depthScreenOri = Screen.orientation;
                        UpdateDepthDataIndex(m_depthScreenOri, m_humanStencilImage.width, m_humanStencilImage.height, sensorData);

                        // check for body-index texture
                        if (kinectManager.getBodyFrames == KinectManager.BodyTextureType.UserTexture ||
                            kinectManager.getBodyFrames == KinectManager.BodyTextureType.BodyTexture)
                        {
                            KinectInterop.InitSensorData(sensorData, kinectManager);
                        }
                    }

                    // copy the human stencil data to the body index buffer
                    CopyBodyIndexDataToBuffer(m_humanStencilImage.timestamp);
                    m_humanStencilImage.Dispose();
                }
                else if(m_avlHumanStencil)
                {
                    Debug.Log("TryAcquireHumanStencil failed.");
                }

                // body tracking
                if (m_avlBodyTracking /**&& DateTime.UtcNow.Ticks >= m_bodyTrackAfterTime*/ &&
                    NativeApi.UnityARKit_HumanBodyProvider_GetHumanBodyPose3DEstimationEnabled())
                {
                    if (alTrackedBodies == null && (frameSourceFlags & KinectInterop.FrameSource.TypeBody) != 0)  // check for body stream
                    {
                        alTrackedBodies = new List<KinectInterop.BodyData>();
                        sensorData.alTrackedBodies = new KinectInterop.BodyData[0]; // new List<KinectInterop.BodyData>();

                        trackedBodiesCount = 0;
                        sensorData.trackedBodiesCount = 0;
                    }

                    m_bodyFrameTimestamp = camFrameTimestamp;
                    using (TrackableChanges<ARHumanBody> changes = ARKitHelper.GetHumanBodyChanges(ARHumanBody.defaultValue, Allocator.Temp))
                    {
                        //Debug.Log("Body changes - added: " + changes.added.Length + ", updated: " + changes.updated.Length + ", removed: " + changes.removed.Length);
                        if(changes.added.Length != 0 || changes.updated.Length != 0 || changes.removed.Length != 0)
                        {
                            foreach (ARHumanBody addedBody in changes.added)
                            {
                                //Debug.Log("Added body: " + addedBody.trackableId.subId1 + ", anchorId: " + addedBody.trackableId + ", state: " + addedBody.trackingState + ", timestamp: " + m_bodyFrameTimestamp);

                                ARKitHelper.GetHumanBodySkeleton(addedBody.trackableId, Allocator.Persistent, ref m_bodyJoints3d);
                                if (addedBody.trackingState != TrackingState.None)
                                {
                                    ulong userId = addedBody.trackableId.subId1;
                                    AddOrUpdateTrackedBody(userId, addedBody.pose, m_bodyJoints3d, m_bodyFrameTimestamp, sensorData);
                                }
                            }

                            List<ulong> alUserIds = GetTrackedBodyIds();
                            foreach (ARHumanBody updatedBody in changes.updated)
                            {
                                //Debug.Log("Updated body: " + userId + ", updatedBody.trackableId.subId1: " + updatedBody.trackableId + ", state: " + updatedBody.trackingState + ", timestamp: " + m_bodyFrameTimestamp);
                                ARKitHelper.GetHumanBodySkeleton(updatedBody.trackableId, Allocator.Temp, ref m_bodyJoints3d);
                                if (updatedBody.trackingState != TrackingState.None)
                                {
                                    ulong userId = updatedBody.trackableId.subId1;
                                    if (alUserIds.Contains(userId))
                                        alUserIds.Remove(userId);

                                    AddOrUpdateTrackedBody(userId, updatedBody.pose, m_bodyJoints3d, m_bodyFrameTimestamp, sensorData);
                                }
                            }

                            foreach (TrackableId removedBodyId in changes.removed)
                            {
                                ulong userId = removedBodyId.subId1;
                                if (alUserIds.Contains(userId))
                                    alUserIds.Remove(userId);

                                //Debug.Log("Removed body: " + userId + ", timestamp: " + m_bodyFrameTimestamp);
                                RemoveTrackedBody(userId, m_bodyFrameTimestamp);
                            }

                            if (alUserIds.Count > 0)
                            {
                                m_bodyFrameTimestamp++;  // make the timestamp different

                                // remove lost bodies
                                foreach (ulong userId in alUserIds)
                                {
                                    //Debug.Log("Lost body: " + userId + ", timestamp: " + m_bodyFrameTimestamp);
                                    RemoveTrackedBody(userId, m_bodyFrameTimestamp);
                                }
                            }

                        }
                    }

                    // clean up user history
                    if (jointPositionFilter != null)
                    {
                        jointPositionFilter.CleanUpUserHistory();
                    }
                }
                else if(m_avlBodyTracking /**&& DateTime.UtcNow.Ticks >= m_bodyTrackAfterTime*/)
                {
                    Debug.Log("HumanBodyPose3DEstimation is disabled.");
                }

                // color2depth frame
                if (colorCamDepthDataFrame != null && lastColorCamDepthFrameTime != m_depthDataTimestamp)
                {
                    TransformDepthFrameToColorCamResolution(sensorData);
                    //Debug.Log("ColorCamDepthFrameTime: " + lastColorCamDepthFrameTime);
                }

                // color2bodyIndex frame
                if (colorCamBodyIndexFrame != null && lastColorCamBodyIndexFrameTime != m_bodyIndexTimestamp)
                {
                    TransformBodyIndexFrameToColorCamResolution(sensorData);
                    //Debug.Log("ColorCamBodyIndexFrameTime: " + lastColorCamBodyIndexFrameTime);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        // body center as sphere
        //private GameObject bodyCenter = null;


        // gets the given camera intrinsics
        private void GetCameraIntrinsics(KinectInterop.DistortionType dType, int width, int height, float ppx, float ppy, float fx, float fy,
            ref KinectInterop.CameraIntrinsics intr, int camType)
        {
            intr = new KinectInterop.CameraIntrinsics();

            intr.cameraType = camType;
            intr.width = width;
            intr.height = height;

            intr.ppx = ppx;
            intr.ppy = ppy;

            intr.fx = fx;
            intr.fy = fy;

            intr.distCoeffs = new float[0];
            //camIntr.coeffs.CopyTo(intr.distCoeffs, 0);

            intr.distType = dType;

            EstimateFOV(intr);
            //Debug.Log(string.Format("Intr{0} - {1}, Res: ({2}, {3}), Ppt: ({4:F2}, {5:F2}), Flen: ({6:F2}, {7:F2})", camType, dType, width, height, ppx, ppy, fx, fy));
        }


        // try to get the latest camera frame
        private bool GetLatestCameraFrame(Camera camera, KinectInterop.SensorData sensorData)
        {
            if (camera == null)
                return false;

            var cameraParams = new ARCameraParams
            {
                zNear = camera.nearClipPlane,
                zFar = camera.farClipPlane,

                screenWidth = Screen.width,
                screenHeight = Screen.height,
                screenOrientation = Screen.orientation
            };

            if (NativeApi.UnityARKit_Camera_TryGetFrame(cameraParams, out m_cameraFrame))
            {
                //Debug.Log("LatestFrame timestamp: " + m_cameraFrame.timestampNs);

                if(m_arBackMaterial != null)
                {
                    ARKitHelper.UpdateCameraTexturesInfos(m_cameraTextureInfos);
                }

                if (m_cameraFrame.hasTimestamp)
                {
                    m_frameTimestampNs = m_cameraFrame.timestampNs;
                }

                if (m_cameraFrame.hasDisplayMatrix)
                {
                    m_displayMatrix = m_cameraFrame.displayMatrix;
                }

                if (m_cameraFrame.hasProjectionMatrix)
                {
                    m_projectionMatrix = m_cameraFrame.projectionMatrix;

                    float resx = Screen.width; float resy = Screen.height;
                    float fx = m_projectionMatrix[0] * resx / 2f; float fy = m_projectionMatrix[5] * resy / 2f;
                    float ppx = resx - (m_projectionMatrix[8] + 1f) * resx / 2f; float ppy = resy - (m_projectionMatrix[9] + 1f) * resy / 2f;
                    GetCameraIntrinsics(KinectInterop.DistortionType.InverseBrownConrady, (int)resx, (int)resy, ppx, ppy, fx, fy, ref sensorData.colorCamIntr, 1);
                }

                return true;
            }

            return false;
        }


        // updates the current camera pose
        private bool UpdateCameraPose(ulong timestamp)
        {
            UnityEngine.XR.InputTracking.GetNodeStates(m_nodeStates);

            foreach (var nodeState in m_nodeStates)
            {
                if (nodeState.nodeType == UnityEngine.XR.XRNode.CenterEye)
                {
                    var currentPose = Pose.identity;
                    var positionSuccess = nodeState.TryGetPosition(out currentPose.position);
                    var rotationSuccess = nodeState.TryGetRotation(out currentPose.rotation);

                    if (positionSuccess)
                        m_cameraPose.position = currentPose.position;
                    if (rotationSuccess)
                        m_cameraPose.rotation = currentPose.rotation;

                    if(positionSuccess || rotationSuccess)
                    {
                        m_cameraPoseTimestamp = timestamp;

                        if(m_floorDetector != null)
                        {
                            // update the imu gravity vector, if floor detection is enabled
                            Vector3 imuUpVector = m_cameraPoseInv.rotation * Vector3.up;  // m_cameraPose.rotation

                            m_floorDetector.UpdateImuUpVector(imuUpVector);
                        }
                    }

                    //Debug.Log("  Got camera pose - pos: " + positionSuccess + ", rot: " + rotationSuccess + ", pos: " + currentPose.position + ", rot: " + currentPose.rotation.eulerAngles);
                    return (positionSuccess || rotationSuccess);
                }
            }

            return false;
        }


        // updates the color camera (background) texture
        private void UpdateColorCameraTexture(KinectInterop.SensorData sensorData)
        {
            int numTextures = m_cameraTextureInfos.Count;

            if (m_arBackMaterial && numTextures > 0)
            {
                //ARTextureDescriptor texDescr0 = m_cameraTextureInfos[0].descriptor;
                int texWidth = Screen.width;  // Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight ? texDescr0.width : texDescr0.height;
                int texHeight = Screen.height;  // Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight ? texDescr0.height : texDescr0.width;

                if (m_arBackTexture == null || m_arBackTexture.width != texWidth || m_arBackTexture.height != texHeight)
                {
                    m_arBackTexture = KinectInterop.CreateRenderTexture(m_arBackTexture, texWidth, texHeight);

                    sensorData.colorImageWidth = texWidth;  // m_arBackTexture.width;
                    sensorData.colorImageHeight = texHeight;  // m_arBackTexture.height;

                    if ((frameSourceFlags & KinectInterop.FrameSource.TypeColor) != 0)
                    {
                        sensorData.colorImageTexture = m_arBackTexture;
                    }

                    //Debug.Log("ColorCamTex w: " + m_arBackTexture.width + ", h: " + m_arBackTexture.height + ", scrOrient: " + Screen.orientation + ", screenW: " + Screen.width + ", screenH: " + Screen.height);
                }

                for (int i = 0; i < numTextures; i++)
                {
                    m_arBackMaterial.SetTexture(m_cameraTextureInfos[i].descriptor.propertyNameId, m_cameraTextureInfos[i].texture);
                }

                //m_arBackMaterial.SetMatrix(k_displayTransform, Matrix4x4.identity);
                m_arBackMaterial.SetMatrix(k_displayTransform, m_displayMatrix);
                //Debug.Log("scrOrient: " + Screen.orientation + ", displayMat - pos: " + (Vector3)m_displayMatrix.GetColumn(3) + ", rot: " + m_displayMatrix.rotation.eulerAngles);

                Graphics.Blit(null, m_arBackTexture, m_arBackMaterial);
                m_arColorFrameTimestamp = (ulong)m_frameTimestampNs / 100;
            }
        }


        // job to convert the depth data from meters to mm
        struct JobSetDepthDataIndex : IJobParallelFor
        {
            [ReadOnly]
            public ScreenOrientation depthScreenOri;  // screen orientation

            [ReadOnly]
            public int depthImageW;  // depth image width

            [ReadOnly]
            public int depthImageH;  // depth image height

            [ReadOnly]
            public int oriImageW;  // oriented image width

            [ReadOnly]
            public int oriImageH;  // oriented image height

            // output index array
            public NativeArray<int> depthDataIndex;  // depth data index


            public void Execute(int i)
            {
                int x = i % oriImageW;
                int y = i / oriImageW;

                switch (depthScreenOri)
                {
                    case ScreenOrientation.Portrait:
                        depthDataIndex[i] = (oriImageW - 1 - x) * depthImageW + (oriImageH - 1 - y);
                        break;

                    case ScreenOrientation.LandscapeRight:
                        depthDataIndex[i] = y * depthImageW + (oriImageW - 1 - x);
                        break;

                    case ScreenOrientation.PortraitUpsideDown:
                        depthDataIndex[i] = x * depthImageW + y;
                        break;

                    case ScreenOrientation.LandscapeLeft:
                        depthDataIndex[i] = (oriImageH - 1 - y) * depthImageW + x;
                        break;

                    default:
                        depthDataIndex[i] = i;
                        break;
                }
            }
        }

        // update the depth data index, according to the current screen orientation
        private void UpdateDepthDataIndex(ScreenOrientation depthScreenOri, int depthImageW, int depthImageH, KinectInterop.SensorData sensorData)
        {
            Vector2Int res = Vector2Int.zero;
            Vector2 ppt = Vector2.zero;
            Vector2 flen = Vector2.zero;

            float fFactorX = m_gotCameraIntrinsics ? ((float)depthImageW / (float)m_cameraIntrinsics.resolution.x) : 0f;
            float fFactorY = m_gotCameraIntrinsics ? ((float)depthImageH / (float)m_cameraIntrinsics.resolution.y) : 0f;

            switch (depthScreenOri)
            {
                case ScreenOrientation.Portrait:
                case ScreenOrientation.PortraitUpsideDown:
                    sensorData.depthImageWidth = depthImageH;
                    sensorData.depthImageHeight = depthImageW;

                    if (m_gotCameraIntrinsics)
                    {
                        res = new Vector2Int(depthImageH, depthImageW);
                        ppt = new Vector2(m_cameraIntrinsics.principalPoint.y * fFactorY, m_cameraIntrinsics.principalPoint.x * fFactorX);
                        flen = new Vector2(m_cameraIntrinsics.focalLength.y * fFactorY, m_cameraIntrinsics.focalLength.x * fFactorX);
                    }
                    break;

                case ScreenOrientation.LandscapeRight:
                case ScreenOrientation.LandscapeLeft:
                default:
                    sensorData.depthImageWidth = depthImageW;
                    sensorData.depthImageHeight = depthImageH;

                    if (m_gotCameraIntrinsics)
                    {
                        res = new Vector2Int(depthImageW, depthImageH);
                        ppt = new Vector2(m_cameraIntrinsics.principalPoint.x * fFactorX, m_cameraIntrinsics.principalPoint.y * fFactorY);
                        flen = new Vector2(m_cameraIntrinsics.focalLength.x * fFactorX, m_cameraIntrinsics.focalLength.y * fFactorY);
                    }
                    break;
            }

            var job = new JobSetDepthDataIndex()
            {
                depthScreenOri = depthScreenOri,
                depthImageW = depthImageW,
                depthImageH = depthImageH,
                oriImageW = sensorData.depthImageWidth,
                oriImageH = sensorData.depthImageHeight,
                depthDataIndex = m_depthDataIndex
            };

            JobHandle jobHandle = job.Schedule(m_depthDataIndex.Length, 64);
            jobHandle.Complete();

            if (m_gotCameraIntrinsics)
            {
                GetCameraIntrinsics(KinectInterop.DistortionType.BrownConrady, res.x, res.y, ppt.x, ppt.y, flen.x, flen.y, ref sensorData.depthCamIntr, 0);
            }

            if(detectFloorForPoseEstimation && m_floorDetector == null)
            {
                // init the floor detector, if needed
                m_floorDetector = new KinectFloorDetector();
                m_floorDetector.InitFloorDetector(this, sensorData, MAX_DEPTH_DISTANCE_MM);
            }

            //Debug.Log("UpdateDepthDataIndex() completed for screenOri: " + depthScreenOri + ", depthW: " + sensorData.depthImageWidth + ", depthH: " + sensorData.depthImageHeight);
        }


        // job to convert the depth data from meters to mm
        struct JobCopyDepthDataToBuffer : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float> depthData;  // depth data

            [ReadOnly]
            public byte minDepthConf;  // minimum depth confidence

            [ReadOnly]
            public NativeArray<byte> depthConf;  // depth confidence (0=low, 1=medium, 2=high)

            [ReadOnly]
            public NativeArray<int> depthIndex;  // depth data index

            // output depth data
            public NativeArray<ushort> depthDataBuffer;  // depth data buffer


            public void Execute(int i)
            {
                int di = depthIndex[i];
                //depthDataBuffer[i] = (ushort)(depthData[di] * 1000f);
                depthDataBuffer[i] = minDepthConf == 255 || depthConf[di] >= minDepthConf ? (ushort)(depthData[di] * 1000f) : (ushort)0;
            }
        }

        // copy the depth data to the ushorts-buffer
        private void CopyDepthDataToBuffer(double timestamp)
        {
            NativeArray<float> arKitDepthData = m_depthDataImage.GetPlane(0).dataAsFloat;

            var job = minDepthConfidence != MinDepthConfMode.Low ?
                new JobCopyDepthDataToBuffer()
                {
                    depthData = arKitDepthData,
                    minDepthConf = (byte)minDepthConfidence,
                    depthConf = m_depthConfImage.GetPlane(0).data,
                    depthIndex = m_depthDataIndex,
                    depthDataBuffer = m_depthDataBuffer
                } :
                new JobCopyDepthDataToBuffer()
                {
                    depthData = arKitDepthData,
                    minDepthConf = (byte)255,
                    depthIndex = m_depthDataIndex,
                    depthDataBuffer = m_depthDataBuffer
                };

            JobHandle jobHandle = job.Schedule(arKitDepthData.Length, 64);
            jobHandle.Complete();

            //int i = (m_depthDataImage.height / 2) * m_depthDataImage.width + m_depthDataImage.width / 2;
            //Debug.Log(string.Format("  DepthBuffer data: {0} {1} {2} {3}, Len: {4}", m_depthDataBuffer[i], m_depthDataBuffer[i + 1], m_depthDataBuffer[i + 2], m_depthDataBuffer[i + 3], m_depthDataBuffer.Length));

            m_depthDataTimestamp = (ulong)(timestamp * 10000000.0);
            //Debug.Log("CopyDepthDataToBuffer() timestamp: " + timestamp + ", ts: " + m_depthDataTimestamp);
        }


        // job to convert the human depth data from meters to mm
        struct JobCopyHumanDepthToBuffer : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float> depthData;  // depth data

            [ReadOnly]
            public NativeArray<int> depthIndex;  // depth data index

            // output depth data
            public NativeArray<ushort> depthDataBuffer;  // depth data buffer


            public void Execute(int i)
            {
                depthDataBuffer[i] = (ushort)(depthData[depthIndex[i]] * 1000f);
            }
        }

        // copy the depth data to the ushorts-buffer
        private void CopyHumanDepthToBuffer(double timestamp)
        {
            NativeArray<float> arKitDepthData = m_depthDataImage.GetPlane(0).dataAsFloat;

            var job = new JobCopyHumanDepthToBuffer()
            {
                depthData = arKitDepthData,
                depthIndex = m_depthDataIndex,
                depthDataBuffer = m_depthDataBuffer
            };

            JobHandle jobHandle = job.Schedule(arKitDepthData.Length, 64);
            jobHandle.Complete();

            m_depthDataTimestamp = (ulong)(timestamp * 10000000.0);
            //Debug.Log("CopyHumanDepthToBuffer() timestamp: " + timestamp + ", ts: " + m_depthDataTimestamp);
        }


        // job to convert the human-stencil data to the body-index buffer
        struct JobCopyBodyIndexDataToBuffer : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<byte> humanStencilData;  // human stencil data

            [ReadOnly]
            public NativeArray<int> depthIndex;  // depth data index

            // output body index data
            public NativeArray<byte> bodyIndexDataBuffer;  // body-index buffer


            public void Execute(int i)
            {
                bodyIndexDataBuffer[i] = humanStencilData[depthIndex[i]] != 0 ? (byte)0 : (byte)255;
            }
        }

        // copy the human-stencil data to the body index-buffer
        private void CopyBodyIndexDataToBuffer(double timestamp)
        {
            NativeArray<byte> arKitHumanStencilData = m_humanStencilImage.GetPlane(0).data;

            var job = new JobCopyBodyIndexDataToBuffer()
            {
                humanStencilData = arKitHumanStencilData,
                depthIndex = m_depthDataIndex,
                bodyIndexDataBuffer = m_bodyIndexDataBuffer
            };

            JobHandle jobHandle = job.Schedule(arKitHumanStencilData.Length, 64);
            jobHandle.Complete();

            m_bodyIndexTimestamp = (ulong)(timestamp * 10000000.0);
            //Debug.Log("CopyBodyIndexDataToBuffer() timestamp: " + timestamp + ", ts: " + m_bodyIndexTimestamp);
        }


        // returns the list of all tracked bodies
        private List<ulong> GetTrackedBodyIds()
        {
            List<ulong> alUserIds = new List<ulong>();

            for (int i = 0; i < alTrackedBodies.Count; i++)
            {
                alUserIds.Add(alTrackedBodies[i].liTrackingID);
            }

            return alUserIds;
        }

        // adds or updates a tracked body skeleton
        private void AddOrUpdateTrackedBody(ulong userId, Pose bodyPose, NativeArray<ARHumanBodyJoint> bodyJoints, ulong bodyFrameTime, KinectInterop.SensorData sensorData)
        {
            // get sensor-to-world matrix
            Matrix4x4 sensorToWorld = GetSensorToWorldMatrix();

            Matrix4x4 matBodyPose = Matrix4x4.identity;
            matBodyPose.SetTRS(bodyPose.position, bodyPose.rotation, Vector3.one);

            float scaleX = sensorData.depthImageScale.x;
            float scaleY = sensorData.depthImageScale.y;

            int bi = -1;
            for(int i = alTrackedBodies.Count - 1; i >= 0; i--)
            {
                if(alTrackedBodies[i].liTrackingID == userId)
                {
                    bi = i;
                    break;
                }
            }

            // create new slot
            bool bIsNew = false;
            if (bi < 0)
            {
                bi = alTrackedBodies.Count;
                bIsNew = true;

                alTrackedBodies.Add(new KinectInterop.BodyData((int)KinectInterop.JointType.Count));
                trackedBodiesCount = (uint)alTrackedBodies.Count;
            }

            // set body data
            KinectInterop.BodyData bodyData = alTrackedBodies[bi];

            if(bIsNew)
            {
                bodyData.liTrackingID = userId;
                bodyData.iBodyIndex = bi;
                bodyData.bIsTracked = true;

                //Debug.Log("Added body - userId: " + userId + ", bi: " + bi + ", numBodies: " + trackedBodiesCount + ", timestamp: " + bodyFrameTime);
            }
            else
            {
                //Debug.Log("Updated body - userId: " + userId + ", bi: " + bi + ", numBodies: " + trackedBodiesCount + ", timestamp: " + bodyFrameTime);
            }

            int jointCount = JointType2ARKitJoint.Length;
            for (int jJT = 0; jJT < jointCount; jJT++)
            {
                KinectInterop.JointData jointData = bodyData.joint[jJT];
                int j = JointType2ARKitJoint[jJT];

                if (j >= 0)
                {
                    ARHumanBodyJoint bodyJoint = bodyJoints[j];
                    jointData.trackingState = bodyJoint.tracked ? KinectInterop.TrackingState.Tracked : KinectInterop.TrackingState.NotTracked;

                    Vector3 jointPos = matBodyPose.MultiplyPoint3x4(bodyJoint.anchorPose.position);
                    jointPos = m_cameraPoseInv.MultiplyPoint3x4(jointPos);  // bodyJoint.anchorPose.position
                    float jPosZ = (bIgnoreZCoordinates && j > 0) ? bodyData.joint[0].kinectPos.z : jointPos.z;

                    jointData.kinectPos = jointPos;
                    jointData.position = sensorToWorld.MultiplyPoint3x4(new Vector3(jointPos.x * scaleX, jointPos.y * scaleY, jPosZ));

                    jointData.orientation = Quaternion.identity;

                    //if (jJT == 0)
                    //{
                    //    Debug.Log("userId: " + userId + ", bodyPos: " + bodyPose.position + ", bodyRot: " + bodyPose.rotation.eulerAngles +
                    //        "\ncamInv pos: " + (Vector3)m_cameraPoseInv.GetColumn(3) + ", rot: " + m_cameraPoseInv.rotation.eulerAngles + ", localPos: " + jointPos +
                    //        "\nsensor pos: " + sensorPosePosition + ", rot: " + sensorPoseRotation.eulerAngles + ", worldPos: " + jointData.position);
                    //}
                }
                else
                {
                    jointData.trackingState = KinectInterop.TrackingState.NotTracked;
                }

                bodyData.joint[jJT] = jointData;

                if (jJT == 0)
                {
                    bodyData.kinectPos = jointData.kinectPos;
                    bodyData.position = jointData.position;
                    bodyData.orientation = jointData.orientation;
                }
            }

            // hand states
            //bodyData.leftHandState = (KinectInterop.HandState)body.HandLeftState;
            //bodyData.rightHandState = (KinectInterop.HandState)body.HandRightState;

            // estimate additional joints
            CalcBodySpecialJoints(ref bodyData);

            // filter joint positions
            if(jointPositionFilter != null)
            {
                //jointPositionFilter.UpdateFilter(ref bodyData);
            }

            // calculate bone dirs
            KinectInterop.CalcBodyJointDirs(ref bodyData);

            // calculate joint orientations
            CalcBodyJointOrients(ref bodyData);

            // body orientation
            bodyData.normalRotation = bodyData.joint[0].normalRotation;
            bodyData.mirroredRotation = bodyData.joint[0].mirroredRotation;

            alTrackedBodies[bi] = bodyData;
        }

        // removes the tracked body by Id
        private void RemoveTrackedBody(ulong userId, ulong bodyFrameTime)
        {
            // remove body data from the list
            int bi = alTrackedBodies.Count;
            for (int i = alTrackedBodies.Count - 1; i >= 0; i--)
            {
                if (alTrackedBodies[i].liTrackingID == userId)
                {
                    bi = i;

                    alTrackedBodies.RemoveAt(bi);
                    trackedBodiesCount = (uint)alTrackedBodies.Count;
                    break;
                }
            }

            // update body indices of the subsequent bodies
            for (int i = bi; i < trackedBodiesCount; i++)
            {
                KinectInterop.BodyData bodyData = alTrackedBodies[bi];
                bodyData.iBodyIndex = i;
                alTrackedBodies[bi] = bodyData;
            }

            //Debug.Log("Removed body - userId: " + userId + ", bi: " + bi + ", numBodies: " + trackedBodiesCount + ", timestamp: " + bodyFrameTime);
        }

        //// removes all tracked bodies
        //private void RemoveAllTrackedBodies()
        //{
        //    for (int i = alTrackedBodies.Count - 1; i >= 0; i--)
        //    {
        //        RemoveTrackedBody(alTrackedBodies[i].liTrackingID, m_bodyFrameTimestamp);
        //    }

        //    List<ulong> alBodyIds = new List<ulong>(m_bodyIdToAnchorId.Keys);
        //    foreach(ulong bodyId in alBodyIds)
        //    {
        //        TrackableId bodyAnchorId = m_bodyIdToAnchorId[bodyId];
        //        m_bodyIdToAnchorId.Remove(bodyId);
        //        Debug.Log("Removing body anchorId: " + bodyAnchorId + " for body: " + bodyId);

        //        if (NativeApi.UnityARKit_refPoints_tryRemove(bodyAnchorId))
        //        {
        //            Debug.Log("Successfully removed body anchor: " + bodyAnchorId);
        //        }
        //    }
        //}


        private static readonly int[] JointType2ARKitJoint =
        {
            (int)JIndex3D.Hips,  // Pelvis = 0,
            (int)JIndex3D.Spine3,  // SpineNaval = 1,
            (int)JIndex3D.Spine7,  // SpineChest = 2,
            (int)JIndex3D.Neck1,  // Neck = 3,
            (int)JIndex3D.Head,  // Head = 4,

            (int)JIndex3D.RightShoulder1,  // ClavicleRight = 10,  // switch left & right joints
            (int)JIndex3D.RightArm,  // ShoulderRight = 11,
            (int)JIndex3D.RightForearm,  // ElbowRight = 12,
            (int)JIndex3D.RightHand,  // WristRight = 13,
            (int)JIndex3D.RightHandIndexStart,  // HandRight = 14,

            (int)JIndex3D.LeftShoulder1,  // ClavicleLeft = 5,
            (int)JIndex3D.LeftArm,  // ShoulderLeft = 6,
            (int)JIndex3D.LeftForearm,  // ElbowLeft = 7,
            (int)JIndex3D.LeftHand,  // WristLeft = 8,
            (int)JIndex3D.LeftHandIndexStart,  // HandLeft = 9,

            (int)JIndex3D.RightUpLeg,  // HipRight = 19,
            (int)JIndex3D.RightLeg,  // KneeRight = 20,
            (int)JIndex3D.RightFoot,  // AnkleRight = 21,
            (int)JIndex3D.RightToes,  // FootRight = 22,

            (int)JIndex3D.LeftUpLeg,  // HipLeft = 15,
            (int)JIndex3D.LeftLeg,  // KneeLeft = 16,
            (int)JIndex3D.LeftFoot,  // AnkleLeft = 17,
            (int)JIndex3D.LeftToes,  // FootLeft = 18,

            (int)JIndex3D.Nose,  // Nose = 23,
            (int)JIndex3D.RightEye,  // EyeRight = 26,
            -1,  // EarRight = 27,
            (int)JIndex3D.LeftEye,  // EyeLeft = 24,
            -1,  // EarLeft = 25,

            (int)JIndex3D.RightHandIndexEnd,  // HandtipRight = 30,
            (int)JIndex3D.RightHandThumbEnd,  // ThumbRight = 31
            (int)JIndex3D.LeftHandIndexEnd,  // HandtipLeft = 28,
            (int)JIndex3D.LeftHandThumbEnd,  // ThumbLeft = 29,
        };


        // 3D skeleton joints
        private enum JIndex3D
        {
            Invalid = -1,
            Root = 0, // parent: <none> [-1]
            Hips = 1, // parent: Root [0]
            LeftUpLeg = 2, // parent: Hips [1]
            LeftLeg = 3, // parent: LeftUpLeg [2]
            LeftFoot = 4, // parent: LeftLeg [3]
            LeftToes = 5, // parent: LeftFoot [4]
            LeftToesEnd = 6, // parent: LeftToes [5]
            RightUpLeg = 7, // parent: Hips [1]
            RightLeg = 8, // parent: RightUpLeg [7]
            RightFoot = 9, // parent: RightLeg [8]
            RightToes = 10, // parent: RightFoot [9]
            RightToesEnd = 11, // parent: RightToes [10]
            Spine1 = 12, // parent: Hips [1]
            Spine2 = 13, // parent: Spine1 [12]
            Spine3 = 14, // parent: Spine2 [13]
            Spine4 = 15, // parent: Spine3 [14]
            Spine5 = 16, // parent: Spine4 [15]
            Spine6 = 17, // parent: Spine5 [16]
            Spine7 = 18, // parent: Spine6 [17]
            LeftShoulder1 = 19, // parent: Spine7 [18]
            LeftArm = 20, // parent: LeftShoulder1 [19]
            LeftForearm = 21, // parent: LeftArm [20]
            LeftHand = 22, // parent: LeftForearm [21]
            LeftHandIndexStart = 23, // parent: LeftHand [22]
            LeftHandIndex1 = 24, // parent: LeftHandIndexStart [23]
            LeftHandIndex2 = 25, // parent: LeftHandIndex1 [24]
            LeftHandIndex3 = 26, // parent: LeftHandIndex2 [25]
            LeftHandIndexEnd = 27, // parent: LeftHandIndex3 [26]
            LeftHandMidStart = 28, // parent: LeftHand [22]
            LeftHandMid1 = 29, // parent: LeftHandMidStart [28]
            LeftHandMid2 = 30, // parent: LeftHandMid1 [29]
            LeftHandMid3 = 31, // parent: LeftHandMid2 [30]
            LeftHandMidEnd = 32, // parent: LeftHandMid3 [31]
            LeftHandPinkyStart = 33, // parent: LeftHand [22]
            LeftHandPinky1 = 34, // parent: LeftHandPinkyStart [33]
            LeftHandPinky2 = 35, // parent: LeftHandPinky1 [34]
            LeftHandPinky3 = 36, // parent: LeftHandPinky2 [35]
            LeftHandPinkyEnd = 37, // parent: LeftHandPinky3 [36]
            LeftHandRingStart = 38, // parent: LeftHand [22]
            LeftHandRing1 = 39, // parent: LeftHandRingStart [38]
            LeftHandRing2 = 40, // parent: LeftHandRing1 [39]
            LeftHandRing3 = 41, // parent: LeftHandRing2 [40]
            LeftHandRingEnd = 42, // parent: LeftHandRing3 [41]
            LeftHandThumbStart = 43, // parent: LeftHand [22]
            LeftHandThumb1 = 44, // parent: LeftHandThumbStart [43]
            LeftHandThumb2 = 45, // parent: LeftHandThumb1 [44]
            LeftHandThumbEnd = 46, // parent: LeftHandThumb2 [45]
            Neck1 = 47, // parent: Spine7 [18]
            Neck2 = 48, // parent: Neck1 [47]
            Neck3 = 49, // parent: Neck2 [48]
            Neck4 = 50, // parent: Neck3 [49]
            Head = 51, // parent: Neck4 [50]
            Jaw = 52, // parent: Head [51]
            Chin = 53, // parent: Jaw [52]
            LeftEye = 54, // parent: Head [51]
            LeftEyeLowerLid = 55, // parent: LeftEye [54]
            LeftEyeUpperLid = 56, // parent: LeftEye [54]
            LeftEyeball = 57, // parent: LeftEye [54]
            Nose = 58, // parent: Head [51]
            RightEye = 59, // parent: Head [51]
            RightEyeLowerLid = 60, // parent: RightEye [59]
            RightEyeUpperLid = 61, // parent: RightEye [59]
            RightEyeball = 62, // parent: RightEye [59]
            RightShoulder1 = 63, // parent: Spine7 [18]
            RightArm = 64, // parent: RightShoulder1 [63]
            RightForearm = 65, // parent: RightArm [64]
            RightHand = 66, // parent: RightForearm [65]
            RightHandIndexStart = 67, // parent: RightHand [66]
            RightHandIndex1 = 68, // parent: RightHandIndexStart [67]
            RightHandIndex2 = 69, // parent: RightHandIndex1 [68]
            RightHandIndex3 = 70, // parent: RightHandIndex2 [69]
            RightHandIndexEnd = 71, // parent: RightHandIndex3 [70]
            RightHandMidStart = 72, // parent: RightHand [66]
            RightHandMid1 = 73, // parent: RightHandMidStart [72]
            RightHandMid2 = 74, // parent: RightHandMid1 [73]
            RightHandMid3 = 75, // parent: RightHandMid2 [74]
            RightHandMidEnd = 76, // parent: RightHandMid3 [75]
            RightHandPinkyStart = 77, // parent: RightHand [66]
            RightHandPinky1 = 78, // parent: RightHandPinkyStart [77]
            RightHandPinky2 = 79, // parent: RightHandPinky1 [78]
            RightHandPinky3 = 80, // parent: RightHandPinky2 [79]
            RightHandPinkyEnd = 81, // parent: RightHandPinky3 [80]
            RightHandRingStart = 82, // parent: RightHand [66]
            RightHandRing1 = 83, // parent: RightHandRingStart [82]
            RightHandRing2 = 84, // parent: RightHandRing1 [83]
            RightHandRing3 = 85, // parent: RightHandRing2 [84]
            RightHandRingEnd = 86, // parent: RightHandRing3 [85]
            RightHandThumbStart = 87, // parent: RightHand [66]
            RightHandThumb1 = 88, // parent: RightHandThumbStart [87]
            RightHandThumb2 = 89, // parent: RightHandThumb1 [88]
            RightHandThumbEnd = 90, // parent: RightHandThumb2 [89]
        }


        // unprojects plane point into the space
        public override Vector3 UnprojectPoint(KinectInterop.CameraIntrinsics intr, Vector2 pixel, float depth)
        {
            float x = (pixel.x - intr.ppx) / intr.fx;
            float y = (pixel.y - intr.ppy) / intr.fy;

            Vector3 point = new Vector3(depth * x, depth * y, depth);

            return point;
        }


        // projects space point onto a plane
        public override Vector2 ProjectPoint(KinectInterop.CameraIntrinsics intr, Vector3 point)
        {
            float x = point.x / point.z;
            float y = point.y / point.z;

            Vector2 pixel = new Vector2(x * intr.fx + intr.ppx, y * intr.fy + intr.ppy);

            return pixel;
        }


        // transforms a point from one space to another
        public override Vector3 TransformPoint(KinectInterop.CameraExtrinsics extr, Vector3 point)
        {
            return point;
        }


        // returns the anchor position of the background raw image
        public override Vector2 GetBackgroundImageAnchorPos(KinectInterop.SensorData sensorData)
        {
            return m_backImageAnchorPos;
        }


        // job to calculate the transformed-to-original data indices
        struct JobSetTransformedDataIndex : IJobParallelFor
        {
            [ReadOnly]
            public int camImageW;  // transformed image width

            [ReadOnly]
            public int origImageW;  // original image width

            [ReadOnly]
            public float camFactorX;  // X-factor to original image

            [ReadOnly]
            public float camFactorY;  // Y-factor to original image

            [ReadOnly]
            public int transX, transY;  // translated image offset

            [ReadOnly]
            public int transW, transH;  // translated image size

            // output index array
            public NativeArray<int> transDataIndex;  // transformed-to-original data index


            public void Execute(int i)
            {
                int cx = i % camImageW;
                int cy = i / camImageW;

                int tx = (int)(cx * camFactorX) + transX;
                int ty = (int)(cy * camFactorY) + transY;

                transDataIndex[i] = (tx >= 0 && tx < transW && ty >= 0 && ty < transH) ? (ty * origImageW + tx) : -1;
            }
        }

        // update the color-cam depth data index
        private void UpdateColorCamDepthDataIndex(KinectInterop.SensorData sensorData)
        {
            float fFactor = sensorData.colorImageWidth > sensorData.colorImageHeight ? (float)sensorData.depthImageWidth / (float)sensorData.colorImageWidth :
                (float)sensorData.depthImageHeight / (float)sensorData.colorImageHeight;

            int tw = Mathf.CeilToInt(sensorData.colorImageWidth * fFactor);
            int th = Mathf.CeilToInt(sensorData.colorImageHeight * fFactor);

            int tx = (sensorData.depthImageWidth - tw) / 2;
            int ty = (sensorData.depthImageHeight - th) / 2;

            var job = new JobSetTransformedDataIndex()
            {
                camImageW = sensorData.colorImageWidth,
                origImageW = sensorData.depthImageWidth,
                camFactorX = fFactor,
                camFactorY = fFactor,

                transX = tx,
                transY = ty,
                transW = tw + tx,
                transH = th + ty,

                transDataIndex = m_colorCamDepthDataIndex
            };

            JobHandle jobHandle = job.Schedule(m_colorCamDepthDataIndex.Length, 64);
            jobHandle.Complete();

            //Debug.Log("UpdateColorCamDepthDataIndex() completed cW: " + sensorData.colorImageWidth + ", cH: " + sensorData.colorImageHeight + ", dW: " + sensorData.depthImageWidth + ", dH: " + sensorData.depthImageHeight +
            //    ", factor: " + fFactor + ", tw: " + tw + ", th: " + th + ", tx: " + tx + ", ty: " + ty);

            //int di1 = sensorData.colorImageWidth - 3;
            //int di2 = (sensorData.colorImageHeight - 1) * sensorData.colorImageWidth;
            //Debug.Log("colorCamDepthDataIndex[" + di1 + "]: " + m_colorCamDepthDataIndex[di1] + " " + m_colorCamDepthDataIndex[di1+1] + " " + m_colorCamDepthDataIndex[di1+2] +
            //    "\ncolorCamDepthDataIndex[" + di2 + "]: " + m_colorCamDepthDataIndex[di2] + " " + m_colorCamDepthDataIndex[di2+1] + " " + m_colorCamDepthDataIndex[di2+2]);
        }


        // updates the point-cloud color shader with the actual data
        protected override bool UpdatePointCloudColorShader(KinectInterop.SensorData sensorData)
        {
            if (pointCloudResolution == PointCloudResolution.DepthCameraResolution)
            {
                if (m_arBackTexture != null && sensorData.depthImageWidth > 0 && sensorData.depthImageHeight > 0 &&
                    sensorData.lastDepthCamColorFrameTime != m_arColorFrameTimestamp)
                {
                    sensorData.lastDepthCamColorFrameTime = m_arColorFrameTimestamp;

                    if(m_depthCamColorMaterial == null)
                    {
                        Shader depthCamColorShader = Shader.Find("Kinect/ARKitDepthCamColorShader");
                        if(depthCamColorShader != null)
                        {
                            m_depthCamColorMaterial = new Material(depthCamColorShader);
                        }
                    }

                    // transform the texture
                    if(m_depthCamColorTexture == null || m_depthCamColorTexture.width != sensorData.depthImageWidth || m_depthCamColorTexture.height != sensorData.depthImageHeight)
                    {
                        m_depthCamColorTexture = KinectInterop.CreateRenderTexture(m_depthCamColorTexture, sensorData.depthImageWidth, sensorData.depthImageHeight);
                    }

                    if (m_depthCamColorMaterial != null &&
                         (m_depthCamColorColorW != sensorData.colorImageWidth || m_depthCamColorColorH != sensorData.colorImageHeight ||
                          m_depthCamColorDepthW != sensorData.depthImageWidth || m_depthCamColorDepthH != sensorData.depthImageHeight))
                    {
                        m_depthCamColorColorW = sensorData.colorImageWidth;
                        m_depthCamColorColorH = sensorData.colorImageHeight;
                        m_depthCamColorDepthW = sensorData.depthImageWidth;
                        m_depthCamColorDepthH = sensorData.depthImageHeight;

                        float fFactor = sensorData.colorImageWidth > sensorData.colorImageHeight ? (float)sensorData.depthImageWidth / (float)sensorData.colorImageWidth :
                            (float)sensorData.depthImageHeight / (float)sensorData.colorImageHeight;

                        int tw = Mathf.CeilToInt(sensorData.colorImageWidth * fFactor);
                        int th = Mathf.CeilToInt(sensorData.colorImageHeight * fFactor);

                        int tx = (sensorData.depthImageWidth - tw) / 2;
                        int ty = (sensorData.depthImageHeight - th) / 2;

                        float factorX = (float)sensorData.depthImageWidth / (float)tw;
                        float factorY = (float)sensorData.depthImageHeight / (float)th;

                        float tMinX = (float)tx / (float)sensorData.depthImageWidth;
                        float tMaxX = (float)(tx + tw) / (float)sensorData.depthImageWidth;
                        float tMinY = (float)ty / (float)sensorData.depthImageHeight;
                        float tMaxY = (float)(ty + th) / (float)sensorData.depthImageHeight;

                        m_depthCamColorMaterial.SetFloat("_FactorX", factorX);
                        m_depthCamColorMaterial.SetFloat("_FactorY", factorY);

                        m_depthCamColorMaterial.SetFloat("_TexMinX", tMinX);
                        m_depthCamColorMaterial.SetFloat("_TexMaxX", tMaxX);
                        m_depthCamColorMaterial.SetFloat("_TexMinY", tMinY);
                        m_depthCamColorMaterial.SetFloat("_TexMaxY", tMaxY);

                        //Debug.Log("UpdatePointCloudColorShader() cW: " + sensorData.colorImageWidth + ", cH: " + sensorData.colorImageHeight + ", dW: " + sensorData.depthImageWidth + ", dH: " + sensorData.depthImageHeight +
                        //    ", factor: " + fFactor + ", tw: " + tw + ", th: " + th + ", tx: " + tx + ", ty: " + ty + ", fx: " + factorX + ", fy: " + factorY +
                        //    ", minX: " + tMinX + ", maxX: " + tMaxX + ", minY: " + tMinY + ", maxY: " + tMaxY);
                    }

                    if (m_depthCamColorMaterial != null)
                    {
                        m_depthCamColorMaterial.SetTexture("_ColorTex", m_arBackTexture);
                        Graphics.Blit(null, m_depthCamColorTexture, m_depthCamColorMaterial);

                        if (sensorData.depthCamColorImageTexture != null)
                        {
                            Graphics.CopyTexture(m_depthCamColorTexture, sensorData.depthCamColorImageTexture);
                        }

                        if (pointCloudColorRT != null)
                        {
                            Graphics.Blit(m_depthCamColorTexture, pointCloudColorRT);
                        }

                        if (pointCloudColorTexture != null)
                        {
                            Graphics.Blit(m_depthCamColorTexture, pointCloudColorTexture);  // CopyTexture
                        }
                    }

                    //Debug.Log("UpdatePointCloudColorShader() timestamp: " + m_arColorFrameTimestamp);
                    return true;
                }
            }
            else
            {
                return base.UpdatePointCloudColorShader(sensorData);
            }

            return false;
        }


        // job to copy the depth frame data to the color-cam-depth data buffer
        struct JobCopyDepthFrameToColorCamBuffer : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<ushort> depthData;  // original data

            [ReadOnly]
            public NativeArray<int> transDataIndex;  // transform data index

            // output depth data
            public NativeArray<ushort> colorCamDepthDataBuffer;  // color-cam res depth-data buffer


            public void Execute(int i)
            {
                colorCamDepthDataBuffer[i] = transDataIndex[i] >= 0 ? depthData[transDataIndex[i]] : (ushort)0;
            }
        }

        // transforms the depth frame to color-camera resolution
        private bool TransformDepthFrameToColorCamResolution(KinectInterop.SensorData sensorData)
        {
            if (colorCamDepthDataFrame != null && m_depthDataBuffer.IsCreated)
            {
                int dataBufferLen = sensorData.colorImageWidth * sensorData.colorImageHeight;

                // create color-cam depth data index
                if (!m_colorCamDepthDataIndex.IsCreated ||
                     m_colorCamDepthColorW != sensorData.colorImageWidth || m_colorCamDepthColorH != sensorData.colorImageHeight ||
                     m_colorCamDepthDepthW != sensorData.depthImageWidth || m_colorCamDepthDepthH != sensorData.depthImageHeight)
                {
                    if (m_colorCamDepthDataIndex.IsCreated)
                        m_colorCamDepthDataIndex.Dispose();
                    m_colorCamDepthDataIndex = new NativeArray<int>(dataBufferLen, Allocator.Persistent);

                    m_colorCamDepthColorW = sensorData.colorImageWidth;
                    m_colorCamDepthColorH = sensorData.colorImageHeight;
                    m_colorCamDepthDepthW = sensorData.depthImageWidth;
                    m_colorCamDepthDepthH = sensorData.depthImageHeight;

                    UpdateColorCamDepthDataIndex(sensorData);
                    //Debug.Log("Updated ColorCamDepthDataIndex for cw: " + colorW + ", ch: " + colorH + ", dw: " + depthW + ", dh: " + depthH);
                }

                if (!m_colorCamDepthDataBuffer.IsCreated || m_colorCamDepthDataBuffer.Length != dataBufferLen)
                {
                    if (m_colorCamDepthDataBuffer.IsCreated)
                        m_colorCamDepthDataBuffer.Dispose();
                    m_colorCamDepthDataBuffer = new NativeArray<ushort>(dataBufferLen, Allocator.Persistent);
                    //Debug.Log("Created colorCamDepthDataBuffer with len: " + dataBufferLen);
                }

                var job = new JobCopyDepthFrameToColorCamBuffer()
                {
                    depthData = m_depthDataBuffer,
                    transDataIndex = m_colorCamDepthDataIndex,
                    colorCamDepthDataBuffer = m_colorCamDepthDataBuffer
                };

                JobHandle jobHandle = job.Schedule(m_colorCamDepthDataBuffer.Length, 64);
                jobHandle.Complete();

                // copy data to the buffer
                //lock (colorCamDepthFrameLock)
                {
                    if(colorCamDepthDataFrame.Length != m_colorCamDepthDataBuffer.Length)
                    {
                        colorCamDepthDataFrame = new ushort[m_colorCamDepthDataBuffer.Length];
                        //Debug.Log(" created colorCamDepthDataFrame");
                    }

                    if(sensorData.colorCamDepthImage != null && sensorData.colorCamDepthImage.Length != m_colorCamDepthDataBuffer.Length)
                    {
                        sensorData.colorCamDepthImage = new ushort[m_colorCamDepthDataBuffer.Length];
                        //Debug.Log(" created sensorData.colorCamDepthImage");
                    }

                    m_colorCamDepthDataBuffer.CopyTo(colorCamDepthDataFrame);
                }

                lastColorCamDepthFrameTime = m_depthDataTimestamp;
                //Debug.Log("TransformDepthFrameToColorCamResolution() timestamp: " + lastColorCamDepthFrameTime);

                return true;
            }

            return false;
        }


        // job to copy the body-index frame data to the color-cam-body-index data buffer
        struct JobCopyBodyIndexFrameToColorCamBuffer : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<byte> bodyIndexData;  // original data

            [ReadOnly]
            public NativeArray<int> transDataIndex;  // transform data index

            // output body index data
            public NativeArray<byte> colorCamBodyIndexDataBuffer;  // color-cam res body-index data buffer


            public void Execute(int i)
            {
                colorCamBodyIndexDataBuffer[i] = transDataIndex[i] >= 0 ? bodyIndexData[transDataIndex[i]] : (byte)255;
            }
        }

        // transforms the body-index frame to color-camera resolution
        protected bool TransformBodyIndexFrameToColorCamResolution(KinectInterop.SensorData sensorData)
        {
            if (colorCamBodyIndexFrame != null && m_bodyIndexDataBuffer.IsCreated)
            {
                int dataBufferLen = sensorData.colorImageWidth * sensorData.colorImageHeight;

                // create color-cam depth data index
                if (!m_colorCamDepthDataIndex.IsCreated ||
                     m_colorCamDepthColorW != sensorData.colorImageWidth || m_colorCamDepthColorH != sensorData.colorImageHeight ||
                     m_colorCamDepthDepthW != sensorData.depthImageWidth || m_colorCamDepthDepthH != sensorData.depthImageHeight)
                {
                    if (m_colorCamDepthDataIndex.IsCreated)
                        m_colorCamDepthDataIndex.Dispose();
                    m_colorCamDepthDataIndex = new NativeArray<int>(dataBufferLen, Allocator.Persistent);

                    m_colorCamDepthColorW = sensorData.colorImageWidth;
                    m_colorCamDepthColorH = sensorData.colorImageHeight;
                    m_colorCamDepthDepthW = sensorData.depthImageWidth;
                    m_colorCamDepthDepthH = sensorData.depthImageHeight;

                    UpdateColorCamDepthDataIndex(sensorData);
                    //Debug.Log("Updated ColorCamDepthDataIndex for cw: " + colorW + ", ch: " + colorH + ", dw: " + depthW + ", dh: " + depthH);
                }

                if (!m_colorCamBodyIndexDataBuffer.IsCreated || m_colorCamBodyIndexDataBuffer.Length != dataBufferLen)
                {
                    if (m_colorCamBodyIndexDataBuffer.IsCreated)
                        m_colorCamBodyIndexDataBuffer.Dispose();
                    m_colorCamBodyIndexDataBuffer = new NativeArray<byte>(dataBufferLen, Allocator.Persistent);
                    //Debug.Log("Created colorCamBodyIndexDataBuffer with len: " + dataBufferLen);
                }

                // execute job
                var job = new JobCopyBodyIndexFrameToColorCamBuffer()
                {
                    bodyIndexData = m_bodyIndexDataBuffer,
                    transDataIndex = m_colorCamDepthDataIndex,
                    colorCamBodyIndexDataBuffer = m_colorCamBodyIndexDataBuffer
                };

                JobHandle jobHandle = job.Schedule(m_colorCamBodyIndexDataBuffer.Length, 64);
                jobHandle.Complete();

                // copy data to the buffer
                //lock(colorCamBodyIndexFrameLock)
                {
                    if (colorCamBodyIndexFrame.Length != m_colorCamBodyIndexDataBuffer.Length)
                    {
                        colorCamBodyIndexFrame = new byte[m_colorCamBodyIndexDataBuffer.Length];
                    }

                    if (sensorData.colorCamBodyIndexImage != null && sensorData.colorCamBodyIndexImage.Length != m_colorCamBodyIndexDataBuffer.Length)
                    {
                        sensorData.colorCamBodyIndexImage = new byte[m_colorCamBodyIndexDataBuffer.Length];
                    }

                    m_colorCamBodyIndexDataBuffer.CopyTo(colorCamBodyIndexFrame);
                }

                lastColorCamBodyIndexFrameTime = m_bodyIndexTimestamp;
                //Debug.Log("TransformBodyIndexFrameToColorCamResolution() timestamp: " + lastColorCamBodyIndexFrameTime);

                return true;
            }

            return false;
        }


    }
}
