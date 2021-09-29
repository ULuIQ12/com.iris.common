using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using com.rfilkov.kinect;

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

		public enum Joints2D
		{
			Invalid = -1,
			Head = 0, // parent: Neck1 [1]
			Neck1 = 1, // parent: Root [16]
			RightShoulder1 = 2, // parent: Neck1 [1]
			RightForearm = 3, // parent: RightShoulder1 [2]
			RightHand = 4, // parent: RightForearm [3]
			LeftShoulder1 = 5, // parent: Neck1 [1]
			LeftForearm = 6, // parent: LeftShoulder1 [5]
			LeftHand = 7, // parent: LeftForearm [6]
			RightUpLeg = 8, // parent: Root [16]
			RightLeg = 9, // parent: RightUpLeg [8]
			RightFoot = 10, // parent: RightLeg [9]
			LeftUpLeg = 11, // parent: Root [16]
			LeftLeg = 12, // parent: LeftUpLeg [11]
			LeftFoot = 13, // parent: LeftLeg [12]
			RightEye = 14, // parent: Head [0]
			LeftEye = 15, // parent: Head [0]
			Root = 16, // parent: <none> [-1]
		}

		// 3D joint skeleton
		public enum JointIndices3D
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
		const int k_NumSkeletonJoints3D = 91;

		protected static readonly Dictionary<Joints, JointIndices3D> IRISJointToARF = new Dictionary<Joints, JointIndices3D>
		{
			{Joints.Pelvis, JointIndices3D.Spine1},
			{Joints.SpineNaval, JointIndices3D.Spine2},
			{Joints.SpineChest, JointIndices3D.Spine5},
			{Joints.Neck, JointIndices3D.Neck1},
			{Joints.Head, JointIndices3D.Head},

			{Joints.ClavicleLeft, JointIndices3D.LeftShoulder1},
			{Joints.ShoulderLeft, JointIndices3D.LeftArm},
			{Joints.ElbowLeft, JointIndices3D.LeftForearm},
			{Joints.WristLeft, JointIndices3D.LeftHand},
			{Joints.HandLeft, JointIndices3D.LeftHandMidEnd},

			{Joints.ClavicleRight, JointIndices3D.RightShoulder1},
			{Joints.ShoulderRight, JointIndices3D.RightArm},
			{Joints.ElbowRight, JointIndices3D.RightForearm},
			{Joints.WristRight, JointIndices3D.RightHand},
			{Joints.HandRight, JointIndices3D.RightHandMidEnd},

			{Joints.HipLeft, JointIndices3D.LeftUpLeg},
			{Joints.KneeLeft, JointIndices3D.LeftLeg},
			{Joints.AnkleLeft, JointIndices3D.LeftFoot},
			{Joints.FootLeft, JointIndices3D.LeftFoot},

			{Joints.HipRight, JointIndices3D.RightUpLeg},
			{Joints.KneeRight, JointIndices3D.RightLeg},
			{Joints.AnkleRight, JointIndices3D.RightFoot},
			{Joints.FootRight, JointIndices3D.RightFoot},

			{Joints.Nose, JointIndices3D.Nose},
			{Joints.EyeLeft, JointIndices3D.LeftEye},
			{Joints.EarLeft, JointIndices3D.LeftEye},
			{Joints.EyeRight, JointIndices3D.RightEye},
			{Joints.EarRight, JointIndices3D.RightEye},
			{Joints.HandtipLeft, JointIndices3D.LeftHandMidEnd},
			{Joints.ThumbLeft, JointIndices3D.LeftHandThumbEnd},
			{Joints.HandtipRight, JointIndices3D.RightHandMidEnd},
			{Joints.ThumbRight, JointIndices3D.RightHandThumbEnd}


		};

		public static JointIndices3D getARFJoint(Joints joint)
		{
			
			return IRISJointToARF[joint];
		}

		/*
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
		*/
	}
}
