using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;

namespace com.iris.common
{
    public class SimpleAvatar : MonoBehaviour
    {

		public bool FlipLR = false;

		public Transform HeadT;
		public Transform NeckT;
		public Transform ThoraxT;
		public Transform NavalT;
		public Transform PelvisT;

		public Transform LeftShoulderT;
		//public Transform LeftClavT;
		public Transform LeftElbowT;
		public Transform LeftWristT;
		public Transform LeftHandT;

		public Transform RightShoulderT;
		//public Transform RightClavT;
		public Transform RightElbowT;
		public Transform RightWristT;
		public Transform RightHandT;

		public Transform LeftHipT;
		public Transform LeftKneeT;
		public Transform LeftAnkleT;
		//public Transform LeftFootT;

		public Transform RightHipT;
		public Transform RightKneeT;
		public Transform RightAnkleT;
		//public Transform RightFootT;

		public Transform LeftHandToLeftWrist;
		public Transform LeftWristToLeftElbow;
		public Transform LeftElbowToLeftShoulder;
		public Transform RightHandToRightWrist;
		public Transform RightWristToRightElbow;
		public Transform RightElbowToRightShoulder;

		public Transform LeftAnkleToLeftKnee;
		public Transform LeftKneeToLeftHip;
		public Transform RightAnkleToRightKnee;
		public Transform RightKneeToRightHip;

		public Transform LeftShoulderToRightShoulder;
		public Transform PelvisToThorax;
		public Transform HeadToNeck;
		public Transform NeckToThorax;

		public Transform LeftShoulderToPelvis;
		public Transform RightShoulderToPelvis;
		public Transform LeftHipToThorax;
		public Transform RightHipToTHorax;

		public Transform LeftShoulderToLeftHip;
		public Transform RightShoulderToRightHip;
		
		private Dictionary<KinectInterop.JointType, Transform> JointsToTransforms = new Dictionary<KinectInterop.JointType, Transform>();
		private List<Link> links = new List<Link>();
		private Link testLink;
		// Start is called before the first frame update
		void Awake()
        {
			InitAssoc();
			InitLinks();
			if (Application.platform == RuntimePlatform.IPhonePlayer)
				FlipLR = true;

		}

        // Update is called once per frame
        void Update()
        {
			if (!CVInterface.AreDatasAvailable())
				return;

			
			ulong uid = KinectManager.Instance.GetUserIdByIndex(0);
			
			foreach( KeyValuePair<KinectInterop.JointType, Transform> couples in JointsToTransforms)
			{
				if (KinectManager.Instance.IsJointTracked(uid, couples.Key))
				{
					Vector3 p = KinectManager.Instance.GetJointPosition(uid, couples.Key);
					if (FlipLR)
						p.x *= -1f;
					couples.Value.position = Vector3.Lerp( couples.Value.position,  p, .5f);
				}
			}

			foreach( Link l in links)
			{
				l.tr.position = l.j1.position + (l.j2.position - l.j1.position) / 2f; ;
				l.tr.rotation = Quaternion.LookRotation(l.j2.position - l.j1.position) * Quaternion.LookRotation(Vector3.down);
				Vector3 s = l.tr.localScale;
				l.tr.localScale = new Vector3(s.x, (l.j2.position - l.j1.position).magnitude / 2f , s.z);
			}

			

		}

		private void InitAssoc()
		{
			JointsToTransforms[KinectInterop.JointType.Head] = HeadT;
			JointsToTransforms[KinectInterop.JointType.Neck] = NeckT;
			JointsToTransforms[KinectInterop.JointType.SpineChest] = ThoraxT;
			JointsToTransforms[KinectInterop.JointType.SpineNaval] = NavalT;
			JointsToTransforms[KinectInterop.JointType.Pelvis] = PelvisT;
			JointsToTransforms[KinectInterop.JointType.ShoulderLeft] = LeftShoulderT;
			//JointsToTransforms[KinectInterop.JointType.ClavicleLeft] = LeftClavT;
			JointsToTransforms[KinectInterop.JointType.ElbowLeft] = LeftElbowT;
			JointsToTransforms[KinectInterop.JointType.WristLeft] = LeftWristT;
			JointsToTransforms[KinectInterop.JointType.HandLeft] = LeftHandT;
			JointsToTransforms[KinectInterop.JointType.ShoulderRight] = RightShoulderT;
			//JointsToTransforms[KinectInterop.JointType.ClavicleRight] = RightClavT;
			JointsToTransforms[KinectInterop.JointType.ElbowRight] = RightElbowT;
			JointsToTransforms[KinectInterop.JointType.WristRight] = RightWristT;
			JointsToTransforms[KinectInterop.JointType.HandRight] = RightHandT;
			JointsToTransforms[KinectInterop.JointType.HipLeft] = LeftHipT;
			JointsToTransforms[KinectInterop.JointType.KneeLeft] = LeftKneeT;
			JointsToTransforms[KinectInterop.JointType.AnkleLeft] = LeftAnkleT;
			//JointsToTransforms[KinectInterop.JointType.FootLeft] = LeftFootT;
			JointsToTransforms[KinectInterop.JointType.HipRight] = RightHipT;
			JointsToTransforms[KinectInterop.JointType.KneeRight] = RightKneeT;
			JointsToTransforms[KinectInterop.JointType.AnkleRight] = RightAnkleT;
			//JointsToTransforms[KinectInterop.JointType.FootRight] = RightFootT;

		}

		private void InitLinks()
		{
			Link l0 = new Link();
			l0.j1 = LeftWristT;
			l0.j2 = LeftElbowT;
			l0.tr = LeftWristToLeftElbow;
			links.Add(l0);

			Link l02 = new Link();
			l02.j1 = LeftHandT;
			l02.j2 = LeftWristT;
			l02.tr = LeftHandToLeftWrist;
			links.Add(l02);

			Link l1 = new Link();
			l1.j1 = LeftElbowT;
			l1.j2 = LeftShoulderT;
			l1.tr = LeftElbowToLeftShoulder;			
			links.Add(l1);

			Link l22 = new Link();
			l22.j1 = RightHandT;
			l22.j2 = RightWristT;
			l22.tr = RightHandToRightWrist;
			links.Add(l22);

			Link l2 = new Link();
			l2.j1 = RightWristT;
			l2.j2 = RightElbowT;
			l2.tr = RightWristToRightElbow;
			links.Add(l2);

			Link l3 = new Link();
			l3.j1 = RightElbowT;
			l3.j2 = RightShoulderT;
			l3.tr = RightElbowToRightShoulder;
			links.Add(l3);

			Link l4 = new Link();
			l4.j1 = LeftAnkleT;
			l4.j2 = LeftKneeT;
			l4.tr = LeftAnkleToLeftKnee;
			links.Add(l4);

			Link l5 = new Link();
			l5.j1 = LeftKneeT;
			l5.j2 = LeftHipT;
			l5.tr = LeftKneeToLeftHip;
			links.Add(l5);

			Link l6 = new Link();
			l6.j1 = RightAnkleT;
			l6.j2 = RightKneeT;
			l6.tr = RightAnkleToRightKnee;
			links.Add(l6);

			Link l7 = new Link();
			l7.j1 = RightKneeT;
			l7.j2 = RightHipT;
			l7.tr = RightKneeToRightHip;
			links.Add(l7);

			Link l8 = new Link();
			l8.j1 = PelvisT;
			l8.j2 = ThoraxT;
			l8.tr = PelvisToThorax;
			links.Add(l8);

			Link l9 = new Link();
			l9.j1 = LeftShoulderT;
			l9.j2 = RightShoulderT;
			l9.tr = LeftShoulderToRightShoulder;
			links.Add(l9);

			Link l10 = new Link();
			l10.j1 = HeadT;
			l10.j2 = NeckT;
			l10.tr = HeadToNeck;
			links.Add(l10);

			Link l11 = new Link();
			l11.j1 = NeckT;
			l11.j2 = ThoraxT;
			l11.tr = NeckToThorax;
			links.Add(l11);

			Link l12 = new Link();
			l12.j1 = LeftShoulderT;
			l12.j2 = NavalT;
			l12.tr = LeftShoulderToPelvis;
			links.Add(l12);

			Link l13 = new Link();
			l13.j1 = RightShoulderT;
			l13.j2 = NavalT;
			l13.tr = RightShoulderToPelvis;
			links.Add(l13);

			Link l14 = new Link();
			l14.j1 = LeftHipT;
			l14.j2 = ThoraxT;
			l14.tr = LeftHipToThorax;
			links.Add(l14);

			Link l15 = new Link();
			l15.j1 = RightHipT;
			l15.j2 = ThoraxT;
			l15.tr = RightHipToTHorax;
			links.Add(l15);

			Link l16 = new Link();
			l16.j1 = LeftShoulderT;
			l16.j2 = LeftHipT;
			l16.tr = LeftShoulderToLeftHip;
			links.Add(l16);

			Link l17 = new Link();
			l17.j1 = RightShoulderT;
			l17.j2 = RightHipT;
			l17.tr = RightShoulderToRightHip;
			links.Add(l17);



		}

		public class Link
		{
			public Transform j1;
			public Transform j2;
			public Transform tr;
		}

    }
}
