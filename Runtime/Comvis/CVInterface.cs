using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;

namespace com.iris.common
{
    public class CVInterface : MonoBehaviour
    {
		public static CVInterface _Instance;
		private static Texture2D EmptyTexture = new Texture2D(0, 0);

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

		public static Texture GetDepthMap()
		{
			if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized())
			{
				return KinectManager.Instance.GetDepthImageTex(0);
			}
			return EmptyTexture;
		}

		public static Texture GetUsersMap()
		{
			if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized())
			{
				return KinectManager.Instance.GetUsersImageTex(0);
			}

			return EmptyTexture;
		}

		public static Texture GetColorMap()
		{
			if (KinectManager.Instance != null && KinectManager.Instance.IsInitialized())
			{
				return KinectManager.Instance.GetColorImageTex(0);
			}
			return EmptyTexture;
		}


		////////////////////////////////////////////////////////////////
		private const string KINECT_PREFAB = "CV/KinectManager";
		private GameObject ManagerGO;
		private KinectManager KManager;

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

		private void Init()
		{
			ManagerGO = Instantiate(Resources.Load<GameObject>(KINECT_PREFAB), transform);
			KManager = ManagerGO.GetComponent<KinectManager>();
		}
	}
}
