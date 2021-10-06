﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.iris.common
{
	public class SimpleAvatar2D : MonoBehaviour
	{

		public bool FlipLR = false;
		public Camera ProjectionCamera;
		public float ProjectionDistance = 2f;
		public MeshFilter MiddleMF;
		private Mesh MiddleMesh;

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

		private Dictionary<IRISJoints.Joints2D, Transform> JointsToTransforms = new Dictionary<IRISJoints.Joints2D, Transform>();
		private List<Link> links = new List<Link>();

		// Start is called before the first frame update
		void Awake()
		{
			InitAssoc();
			InitLinks();

		}

		// Update is called once per frame
		void Update()
		{
			if (!CVInterface.AreDatasAvailable())
				return;

			foreach (KeyValuePair<IRISJoints.Joints2D, Transform> couples in JointsToTransforms)
			{
				//if (CVInterface.IsJointTracked(couples.Key))
				//{
				Vector3 p = CVInterface.GetJointPos2D(couples.Key);

				if (p == Vector3.zero)
					continue;

				p.z = ProjectionDistance;
				p = ProjectionCamera.ViewportToWorldPoint(p);
				couples.Value.position = Vector3.Lerp(couples.Value.position, p, .5f);
				//}
			}

			Vector3 ol = LeftShoulderT.position;
			Vector3 or = RightShoulderT.position;
			// shoulders positions hack to straighten the middle mesh x-->  <--x
			/*
			LeftShoulderT.position = Vector3.Lerp(ol, or, .15f);
			RightShoulderT.position = Vector3.Lerp(ol, or, .85f);
			*/
			
			foreach (Link l in links)
			{
				Vector3 delta = l.j2.position - l.j1.position;
				if (delta == Vector3.zero)
					continue;

				if (l.tr == null)
					continue;

				

				l.tr.position = l.j1.position + (delta) / 2f;
				l.tr.rotation = Quaternion.LookRotation(delta) * Quaternion.LookRotation(Vector3.up);
				Vector3 s = l.tr.localScale;
				l.tr.localScale = new Vector3(s.x, (l.j2.position - l.j1.position).magnitude / 2f, s.z);
			}
			
			DrawMiddle();

		}

		private void InitAssoc()
		{
			JointsToTransforms[IRISJoints.Joints2D.Head] = HeadT;
			JointsToTransforms[IRISJoints.Joints2D.Neck1] = NeckT;
			//JointsToTransforms[IRISJoints.Joints2D.Neck1] = ThoraxT;
			//JointsToTransforms[IRISJoints.Joints2D.Root] = NavalT;
			JointsToTransforms[IRISJoints.Joints2D.Root] = PelvisT;
			JointsToTransforms[IRISJoints.Joints2D.LeftShoulder1] = LeftShoulderT;
			//JointsToTransforms[IRISJoints.Joints.ClavicleLeft] = LeftClavT;
			JointsToTransforms[IRISJoints.Joints2D.LeftForearm] = LeftElbowT;
			//JointsToTransforms[IRISJoints.Joints2D.LeftHand] = LeftWristT;
			JointsToTransforms[IRISJoints.Joints2D.LeftHand] = LeftHandT;
			JointsToTransforms[IRISJoints.Joints2D.RightShoulder1] = RightShoulderT;
			//JointsToTransforms[IRISJoints.Joints.ClavicleRight] = RightClavT;
			JointsToTransforms[IRISJoints.Joints2D.RightForearm] = RightElbowT;
			//JointsToTransforms[IRISJoints.Joints2D.RightHand] = RightWristT;
			JointsToTransforms[IRISJoints.Joints2D.RightHand] = RightHandT;
			JointsToTransforms[IRISJoints.Joints2D.LeftUpLeg] = LeftHipT;
			JointsToTransforms[IRISJoints.Joints2D.LeftLeg] = LeftKneeT;
			JointsToTransforms[IRISJoints.Joints2D.LeftFoot] = LeftAnkleT;
			//JointsToTransforms[IRISJoints.Joints.FootLeft] = LeftFootT;
			JointsToTransforms[IRISJoints.Joints2D.RightUpLeg] = RightHipT;
			JointsToTransforms[IRISJoints.Joints2D.RightLeg] = RightKneeT;
			JointsToTransforms[IRISJoints.Joints2D.RightFoot] = RightAnkleT;
			//JointsToTransforms[IRISJoints.Joints.FootRight] = RightFootT;

		}

		private void InitLinks()
		{
			Link l0 = new Link();
			l0.j1 = LeftHandT;
			l0.j2 = LeftElbowT;
			l0.tr = LeftWristToLeftElbow;
			links.Add(l0);
			/*
			Link l02 = new Link();
			l02.j1 = LeftHandT;
			l02.j2 = LeftWristT;
			l02.tr = LeftHandToLeftWrist;
			links.Add(l02);
			*/
			Link l1 = new Link();
			l1.j1 = LeftElbowT;
			l1.j2 = LeftShoulderT;
			l1.tr = LeftElbowToLeftShoulder;
			links.Add(l1);
			/*
			Link l22 = new Link();
			l22.j1 = RightHandT;
			l22.j2 = RightWristT;
			l22.tr = RightHandToRightWrist;
			links.Add(l22);
			*/
			Link l2 = new Link();
			l2.j1 = RightHandT;
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
			l8.j1 = LeftHipT;
			l8.j2 = RightHipT;
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
			/*
			Link l11 = new Link();
			l11.j1 = NeckT;
			l11.j2 = ThoraxT;
			l11.tr = NeckToThorax;
			links.Add(l11);
			*/
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

		private Vector3[] middleVerts;
		private int[] middleTri;
		private void DrawMiddle()
		{

			if (MiddleMF == null)
				return;

			if (MiddleMesh == null)
			{
				MiddleMesh = new Mesh();
				middleVerts = new Vector3[4];
				middleTri = new int[] { 0, 1, 2, 2, 3, 0, 0, 3, 2, 2, 1, 0 };
			}


			MiddleMesh.Clear();

			Vector3 p1 = LeftShoulderT.transform.position;
			Vector3 p2 = RightShoulderT.transform.position;
			Vector3 p3 = RightHipT.transform.position;
			Vector3 p4 = LeftHipT.transform.position;

			middleVerts[0] = p1;
			middleVerts[1] = p2;
			middleVerts[2] = p3;
			middleVerts[3] = p4;

			MiddleMesh.SetVertices(middleVerts);
			MiddleMesh.SetTriangles(middleTri, 0);

			MiddleMF.mesh = MiddleMesh;
		}

		public class Link
		{
			public Transform j1;
			public Transform j2;
			public Transform tr;
		}

	}
}