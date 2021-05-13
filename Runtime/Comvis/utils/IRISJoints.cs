using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;

namespace com.iris.common
{
    public class IRISJoints
    {
		public enum Joints
		{
			Pelvis = 0,
			SpineNaval = 1,
			SpineChest = 2,
			Neck = 3,
			Head = 4,

			ClavicleLeft = 5,
			ShoulderLeft = 6,
			ElbowLeft = 7,
			WristLeft = 8,
			HandLeft = 9,

			ClavicleRight = 10,
			ShoulderRight = 11,
			ElbowRight = 12,
			WristRight = 13,
			HandRight = 14,

			HipLeft = 15,
			KneeLeft = 16,
			AnkleLeft = 17,
			FootLeft = 18,

			HipRight = 19,
			KneeRight = 20,
			AnkleRight = 21,
			FootRight = 22,

			Nose = 23,
			EyeLeft = 24,
			EarLeft = 25,
			EyeRight = 26,
			EarRight = 27,

			HandtipLeft = 28,
			ThumbLeft = 29,
			HandtipRight = 30,
			ThumbRight = 31,
		}

		protected static readonly Dictionary<Joints, KinectInterop.JointType> IRISJointToKinectJoint = new Dictionary<Joints, KinectInterop.JointType>
		{
			{Joints.Pelvis, KinectInterop.JointType.Pelvis},
			{Joints.SpineNaval, KinectInterop.JointType.SpineNaval},
			{Joints.SpineChest, KinectInterop.JointType.SpineChest},
			{Joints.Neck, KinectInterop.JointType.Neck},
			{Joints.Head, KinectInterop.JointType.Head},

			{Joints.ClavicleLeft, KinectInterop.JointType.ClavicleLeft},
			{Joints.ShoulderLeft, KinectInterop.JointType.ShoulderLeft},
			{Joints.ElbowLeft, KinectInterop.JointType.ElbowLeft},
			{Joints.WristLeft, KinectInterop.JointType.WristLeft},
			{Joints.HandLeft, KinectInterop.JointType.HandLeft},

			{Joints.ClavicleRight, KinectInterop.JointType.ClavicleRight},
			{Joints.ShoulderRight, KinectInterop.JointType.ShoulderRight},
			{Joints.ElbowRight, KinectInterop.JointType.ElbowRight},
			{Joints.WristRight, KinectInterop.JointType.WristRight},
			{Joints.HandRight, KinectInterop.JointType.HandRight},

			{Joints.HipLeft, KinectInterop.JointType.HipLeft},
			{Joints.KneeLeft, KinectInterop.JointType.KneeLeft},
			{Joints.AnkleLeft, KinectInterop.JointType.AnkleLeft},
			{Joints.FootLeft, KinectInterop.JointType.FootLeft},

			{Joints.HipRight, KinectInterop.JointType.HipRight},
			{Joints.KneeRight, KinectInterop.JointType.KneeRight},
			{Joints.AnkleRight, KinectInterop.JointType.AnkleRight},
			{Joints.FootRight, KinectInterop.JointType.FootRight},
		};

		protected static readonly Dictionary<Joints, KinectInterop.JointType> IRISJointToKinectJointReverse = new Dictionary<Joints, KinectInterop.JointType>
		{
			{Joints.Pelvis, KinectInterop.JointType.Pelvis},
			{Joints.SpineNaval, KinectInterop.JointType.SpineNaval},
			{Joints.SpineChest, KinectInterop.JointType.SpineChest},
			{Joints.Neck, KinectInterop.JointType.Neck},
			{Joints.Head, KinectInterop.JointType.Head},

			{Joints.ClavicleLeft, KinectInterop.JointType.ClavicleRight},
			{Joints.ShoulderLeft, KinectInterop.JointType.ShoulderRight},
			{Joints.ElbowLeft, KinectInterop.JointType.ElbowRight},
			{Joints.WristLeft, KinectInterop.JointType.WristRight},
			{Joints.HandLeft, KinectInterop.JointType.HandRight},

			{Joints.ClavicleRight, KinectInterop.JointType.ClavicleLeft},
			{Joints.ShoulderRight, KinectInterop.JointType.ShoulderLeft},
			{Joints.ElbowRight, KinectInterop.JointType.ElbowLeft},
			{Joints.WristRight, KinectInterop.JointType.WristLeft},
			{Joints.HandRight, KinectInterop.JointType.HandLeft},

			{Joints.HipLeft, KinectInterop.JointType.HipRight},
			{Joints.KneeLeft, KinectInterop.JointType.KneeRight},
			{Joints.AnkleLeft, KinectInterop.JointType.AnkleRight},
			{Joints.FootLeft, KinectInterop.JointType.FootRight},

			{Joints.HipRight, KinectInterop.JointType.HipLeft},
			{Joints.KneeRight, KinectInterop.JointType.KneeLeft},
			{Joints.AnkleRight, KinectInterop.JointType.AnkleLeft},
			{Joints.FootRight, KinectInterop.JointType.FootLeft},
		};

		public static KinectInterop.JointType GetKinectJoint( Joints joint)
		{
			return IRISJointToKinectJoint[joint];
		}

		public static KinectInterop.JointType GetInvertedKinectJoint(Joints joint)
		{
			return IRISJointToKinectJointReverse[joint];
		}
	}
}
