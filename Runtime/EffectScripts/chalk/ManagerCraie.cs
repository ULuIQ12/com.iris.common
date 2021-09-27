using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using com.rfilkov.kinect;
using DentedPixel;

namespace com.iris.common
{
	public class ManagerCraie : MonoBehaviour
	{
		public static ManagerCraie Instance;
		public float defaultX = 0f;
		public float defaultY = 0f;
		public float scaleGlobal = 1f;
		public float width = 1600f;
		public float height = 1200f;
		public Camera foregroundCamera;
		public GameObject[] bHandRight;
		public GameObject[] bHandLeft;
		public Color[] tabColor;
		public GameObject[] tabGoPointer;
		public Material[] tabMatBG;
		public GameObject goBGToModify;

		Rect backgroundRect;

		// REDO
		/*
		KinectManager kinectManager;
		*/
		//KinectInterop.JointType JointHandRight = KinectInterop.JointType.HandRight;
		//KinectInterop.JointType JointHandLeft = KinectInterop.JointType.HandLeft;
		List<ulong> allUserIds;

		List<int> countPointer = new List<int>();
		//int numFlowerCueillies = 0;

		List<List<Vector2>> listTabs = new List<List<Vector2>>();
		List<Vector2> tab1 = new List<Vector2> { new Vector2(1.7f, 1f), new Vector2(-2.4f, 1f) };
		List<Vector2> tab2 = new List<Vector2> { new Vector2(-0.3f, -1f), new Vector2(-0.3f, 3.1f) };
		List<Vector2> tab3 = new List<Vector2> { new Vector2(1.4f, -0.2f), new Vector2(-0.9f, 2.6f), new Vector2(-2.2f, -0.9f) };
		List<Vector2> tab4 = new List<Vector2> { new Vector2(-2.4f, 3.1f), new Vector2(1.8f, 3.2f), new Vector2(1.8f, -1.1f), new Vector2(-2.4f, -1f) };
		List<Vector2> tab5 = new List<Vector2> { new Vector2(1.6f, 2.8f), new Vector2(-1.4f, 3f), new Vector2(-0.3f, 1.1f), new Vector2(1.1f, -0.6f), new Vector2(-2f, -1f) };
		List<Vector2> tab6 = new List<Vector2> { new Vector2(-2.7f, -1.2f), new Vector2(0.3f, 3.4f), new Vector2(1.4f, -1.4f), new Vector2(-1.1f, 0.1f), new Vector2(1.1f, 0.1f) };
		List<Vector2> tab7 = new List<Vector2> { new Vector2(-2.4f, -1.2f), new Vector2(-1.5f, 3.4f), new Vector2(1.5f, 3.1f), new Vector2(-0.8f, 1.1f), new Vector2(0.7f, -1.3f) };
		List<Vector2> tab8 = new List<Vector2> { new Vector2(1.8f, 2.5f), new Vector2(-0.9f, 3.3f), new Vector2(-2.3f, 1.4f), new Vector2(-1.2f, -1.4f), new Vector2(1.5f, -0.3f) };
		List<Vector2> tab9 = new List<Vector2> { new Vector2(-2.5f, -1.3f), new Vector2(-1.6f, 3.5f), new Vector2(1.7f, 2.7f), new Vector2(1.1f, -0.6f), new Vector2(-1.4f, -1.3f) };
		List<Vector2> tab10 = new List<Vector2> { new Vector2(1.9f, 3.5f), new Vector2(-1.1f, 3.6f), new Vector2(1.1f, 1.2f), new Vector2(-2.2f, -1.2f), new Vector2(1.1f, -1.4f) };
		int currentTab = 0;
		int currentPointer;
		int totGOPointer = 5;

		IEnumerator Start()
		{
			while(!CVInterface.AreDatasAvailable())
			{
				yield return null;
			}
			Instance = this;

			/* REDO
			while (KinectManager.Instance == null || !KinectManager.Instance.IsInitialized())
			{
				yield return null;
			}

			Instance = this;
			kinectManager = KinectManager.Instance;
			*/
			
			for (int i = 0; i < bHandRight.Length; i++)
			{
				bHandRight[i].GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", tabColor[i]);
				bHandLeft[i].GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", tabColor[i]);
			}
			yield return null;

			listTabs.Add(tab1);
			listTabs.Add(tab2);
			listTabs.Add(tab3);
			listTabs.Add(tab4);
			listTabs.Add(tab5);
			listTabs.Add(tab6);
			listTabs.Add(tab7);
			listTabs.Add(tab8);
			listTabs.Add(tab9);
			listTabs.Add(tab10);

			InitTableau();
		}


		public float scaleX = 6f;
		public float scaleY = 5f;

		void Update()
		{
			/*
			if (kinectManager && kinectManager.IsInitialized() && kinectManager.GetUsersCount() > 0)
			{
				width = Screen.width;
				height = Screen.height;
				backgroundRect = new Rect(defaultX - (((width * scaleGlobal) - width) / 2), defaultY - (((height * scaleGlobal) - height) / 2), width * scaleGlobal, height * scaleGlobal);

				allUserIds = kinectManager.GetAllUserIds();
				for (int i = 0; i < allUserIds.Count; i++)
				{
					if (Application.platform == RuntimePlatform.IPhonePlayer)
					{
						MoveHand(allUserIds[i], (int)KinectInterop.JointType.HandRight, bHandLeft[i].transform, backgroundRect);
						MoveHand(allUserIds[i], (int)KinectInterop.JointType.HandLeft, bHandRight[i].transform, backgroundRect);
					}
					else
					{
						MoveHand(allUserIds[i], (int)KinectInterop.JointType.HandLeft, bHandLeft[i].transform, backgroundRect);
						MoveHand(allUserIds[i], (int)KinectInterop.JointType.HandRight, bHandRight[i].transform, backgroundRect);
					}

				}
				for (int i = allUserIds.Count; i < bHandLeft.Length; i++)
				{
					bHandLeft[i].transform.position = new Vector3(i * 5f, i * 5f, i * 5f);
					bHandRight[i].transform.position = new Vector3(i * 5f, i * 5f, i * 5f);
				}


			}
			*/
		}
		/*
		private Transform sensorTransform;
		private Vector3 NotDetectedPos = new Vector3(50f, 50f - 50f);
		// overlays the given object over the given user joint
		private void MoveHand(ulong userId, int jointIndex, Transform overlayObj, Rect imageRect)
		{
			if (foregroundCamera)
			{
				// get the background rectangle (use the portrait background, if available)
				backgroundRect = foregroundCamera.pixelRect;
			}

			if (kinectManager.IsJointTracked(userId, jointIndex))
			{

				Vector3 posJoint = kinectManager.GetJointPosition(userId, jointIndex);


				if (posJoint != Vector3.zero)
				{

					overlayObj.position = posJoint;
					
				}
				else
				{
					overlayObj.position = NotDetectedPos;
				}
			}
		}*/

		void InitTableau()
		{
			Debug.Log("--------------------------------------------InitTableau " + currentTab + "  " + listTabs.Count);
			if (currentTab >= listTabs.Count) currentTab = 0;
			currentPointer = 0;

			for (int i = 0; i < listTabs[currentTab].Count; i++)
			{
				Debug.Log(i + "   :  " + listTabs[currentTab].Count);
				tabGoPointer[i].transform.localPosition = new Vector3(listTabs[currentTab][i].x, listTabs[currentTab][i].y - 1.0f, -0.3f);
				AppearPointer(tabGoPointer[i]);
			}

			//fait disparaitre les numéros non utilisés 
			for (int i = listTabs[currentTab].Count; i < totGOPointer; i++)
			{
				tabGoPointer[i].transform.position = new Vector3(-5f, -5f, -5f);
			}
			Debug.Log(goBGToModify);
			Debug.Log("-material " + goBGToModify.GetComponent<MeshRenderer>().material);
			Debug.Log("-tabMatBG[currentTab] " + tabMatBG[currentTab]);

			goBGToModify.GetComponent<MeshRenderer>().material = tabMatBG[currentTab];
		}

		public static void HandTrigger(GameObject goPointer)
		{
			if (goPointer.CompareTag("Flower"))
			{
				int idPointer = Instance.ArrayContains(Instance.tabGoPointer, goPointer);
				if (idPointer != -1 && idPointer == Instance.currentPointer)
				{
					Instance.DisappearPointer(idPointer);
					Instance.currentPointer++;
					if (Instance.currentPointer >= Instance.listTabs[Instance.currentTab].Count)
					{
						Instance.currentTab++;
						Instance.Invoke("InitTableau", 1);
					}
				}
			}
		}

		void DisappearPointer(int idPointer)
		{
			GameObject goPointer = tabGoPointer[idPointer];
			float startAngle = goPointer.transform.rotation.eulerAngles.z;
			float endAngle = startAngle + 720.0f;
			LeanTween.rotateZ(goPointer, endAngle, 1f).setEase(LeanTweenType.easeInOutQuad);

			LeanTween.value(gameObject, 1f, 0f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float val) =>
			{
				Color currentColor = goPointer.GetComponent<Renderer>().material.GetColor("_BaseColor");
				currentColor.a = val;
				goPointer.GetComponent<Renderer>().material.SetColor("_BaseColor", currentColor);
			});

			goPointer.GetComponent<BoxCollider>().enabled = false;
		}

		void AppearPointer(GameObject goPointer)
		{
			goPointer.GetComponent<BoxCollider>().enabled = true;
			LeanTween.value(gameObject, 0f, 1f, 0.1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float val) =>
			{
				Color currentColor = goPointer.GetComponent<Renderer>().material.GetColor("_BaseColor");
				currentColor.a = val;
				goPointer.GetComponent<Renderer>().material.SetColor("_BaseColor", currentColor);
			});
		}


		int ArrayContains(GameObject[] array, GameObject g)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == g) return i;
			}
			return -1;
		}

	}
}