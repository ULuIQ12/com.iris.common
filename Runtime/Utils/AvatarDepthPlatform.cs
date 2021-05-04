using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;
using com.rfilkov.components;

namespace com.iris.common
{
	[RequireComponent(typeof(AvatarController))]
    public class AvatarDepthPlatform : MonoBehaviour
    {
		private Vector3 iosCamPosition = new Vector3(0.0f, 0.5f, 0f);
		private Vector3 windowsCamPosition = new Vector3(0.0f, 1.0f, 0f);
		private Vector3 iosEulerCamRot = new Vector3(-13.47f, 0.0f, 0f);
		
		public Transform camTransform;

        void Start()
        {
			AvatarController c = GetComponent<AvatarController>();
			
			if (c == null)
				return;

			//Debug.Log("appplaotf = " + Application.platform);
			if( Application.platform == RuntimePlatform.IPhonePlayer)
			{
				//c.flipLeftRight = false;
				c.flipLeftRight = true;
				camTransform.position = iosCamPosition;
				camTransform.rotation = Quaternion.Euler(iosEulerCamRot);


			}
			else
			{
				c.flipLeftRight = false;
				camTransform.localPosition = windowsCamPosition;
				camTransform.localRotation = Quaternion.identity;
			}
        }
		
		public void Update()
		{
			AvatarController c = GetComponent<AvatarController>();

			if (c == null)
				return;

			Debug.Log("appplaotf = " + Application.platform);
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				c.flipLeftRight = true;
				camTransform.position = iosCamPosition;
				camTransform.rotation = Quaternion.Euler(iosEulerCamRot);


			}
			else
			{
				c.flipLeftRight = false;
				camTransform.localPosition = windowsCamPosition;
				camTransform.localRotation = Quaternion.identity;
			}
		}
		
	}
}
