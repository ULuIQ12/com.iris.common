using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.rfilkov.kinect;
using com.iris.common;
//using DentedPixel;


public class ManagerFlower : MonoBehaviour
{
	public Transform LeftHand;
	public Transform RightHand;
    public int totFlower;
    public float defaultX = 0f;
    public float scaleX = 5f;
    public float defaultY = 0f;
    public float scaleY = 5f;

	public static ManagerFlower Instance;

	public float scaleGlobal = 1f;
	public float width = 1600f;
	public float height = 1200f;
	public GameObject[] bHandRight;
	public GameObject[] bHandLeft;
	public Color[] tabColor;
	public Camera foregroundCamera;

	public GameObject goPanier;
	public GameObject goFlowers;

	KinectManager kinectManager;
	KinectInterop.JointType JointHandRight = KinectInterop.JointType.HandRight;
	KinectInterop.JointType JointHandLeft = KinectInterop.JointType.HandLeft;
	List<ulong> allUserIds;

	Vector2[] stockPos;
	List<GameObject> ListFlower = new List<GameObject>();
	List<Vector3> ListPosFlower = new List<Vector3>();
	List<Vector3> ListScaleFlower = new List<Vector3>();
	List<int> countFlower = new List<int>();
	int numFlowerCueillies = 0;
	Rect backgroundRect;


	private bool IsInitialized = false;

    void Start()
    {
		StartCoroutine(WaitForFrame());
        
	}

	private IEnumerator WaitForFrame()
	{
		yield return new WaitForSeconds(0.5f);
		while( KinectManager.Instance == null || !KinectManager.Instance.IsInitialized())
		{
			yield return null;
		}

		Init();
	}

	private void Init()
	{
		kinectManager = KinectManager.Instance;
		totFlower = 25;// GameObject.FindGameObjectsWithTag("Flower").Length;

		HandTrigger htLeft = LeftHand.gameObject.GetComponent<HandTrigger>();
		htLeft.OnHandCollide += OnHandCollide;
		HandTrigger htRight = RightHand.gameObject.GetComponent<HandTrigger>();
		htRight.OnHandCollide += OnHandCollide;

		IsInitialized = true;
	}

    void Update()
    {
		if (!IsInitialized)
			return;

        if (kinectManager && kinectManager.IsInitialized() && kinectManager.GetUsersCount() >0)
        {
			//allUserIds = kinectManager.GetAllUserIds();
			//for(int i = 0; i<allUserIds.Count; i++ )
			//{ Vector3 pos1 = kinectManager.GetJointPosition( allUserIds[i], JointHandRight); }
			Vector3 sensorScale = kinectManager.GetSensorSpaceScale(0);

			if (Application.platform == RuntimePlatform.IPhonePlayer)
				sensorScale.x *= -1f;

			ulong uid = kinectManager.GetUserIdByIndex(0);
            Vector3 HandPosition = kinectManager.GetJointPosition(uid, JointHandRight);
			if( RightHand != null )
				RightHand.position = new Vector3(defaultX+HandPosition.x*scaleX * sensorScale.x, defaultY+HandPosition.y*scaleY * sensorScale.y, 0);

			HandPosition = kinectManager.GetJointPosition(uid, JointHandLeft);
			if (LeftHand != null)
				LeftHand.position = new Vector3(defaultX + HandPosition.x * scaleX * sensorScale.x, defaultY + HandPosition.y * scaleY * sensorScale.y, 0);

		}
    }

	public void OnDisable()
	{
		try
		{
			HandTrigger htLeft = LeftHand.gameObject.GetComponent<HandTrigger>();
			htLeft.OnHandCollide -= OnHandCollide;
			HandTrigger htRight = RightHand.gameObject.GetComponent<HandTrigger>();
			htRight.OnHandCollide -= OnHandCollide;
		}
		catch (System.Exception e) { Debug.Log(e); };
	}

	public void OnHandCollide(GameObject goFlower)
	{
		if (goFlower.CompareTag("Flower"))
		{
			int idFlower = Instance.ListFlower.IndexOf(goFlower);
			if (idFlower != -1 && !Instance.countFlower.Contains(idFlower))
			{
				Instance.countFlower.Add(idFlower);
				Instance.GoFlowerToPanier(idFlower);
				Instance.numFlowerCueillies++;
				if (Instance.numFlowerCueillies >= Instance.totFlower)
				{
					Instance.Invoke("AppearFlowers", 2f);
				}
			}
		}
	}
	/*
	public static void HandTrigger(GameObject goFlower)
	{
		if (goFlower.CompareTag("Flower"))
		{
			int idFlower = Instance.ListFlower.IndexOf(goFlower);
			if (idFlower != -1 && !Instance.countFlower.Contains(idFlower))
			{
				Instance.countFlower.Add(idFlower);
				Instance.GoFlowerToPanier(idFlower);
				Instance.numFlowerCueillies++;
				if (Instance.numFlowerCueillies >= Instance.totFlower)
				{
					Instance.Invoke("AppearFlowers", 2f);
				}
			}
		}
	}*/

	void GoFlowerToPanier(int idFlower)
	{
		GameObject flower = ListFlower[idFlower];
		float startAngle = flower.transform.rotation.eulerAngles.z;
		float endAngle = startAngle + 720.0f;
		LeanTween.rotateZ(flower, endAngle, 2f).setEase(LeanTweenType.easeInOutQuad);

		Vector3 endPos = new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(-3f, -2.1f), 0f);
		LeanTween.moveLocal(flower, endPos, 2f).setEase(LeanTweenType.easeInOutQuad);

		flower.GetComponent<Renderer>().rendererPriority = Instance.numFlowerCueillies + 2;
	}


	void AppearFlowers()
	{
		RandomizeListePosFlower();
		ShowPanier(false);
		float iDelay = 10;
		for (int i = 0; i < ListFlower.Count; i++)
		{
			GameObject flower = ListFlower[i];
			Vector3 endPos = new Vector3(ListPosFlower[i].x, ListPosFlower[i].y - 0.5f, ListPosFlower[i].z);
			LeanTween.moveLocal(flower, endPos, 0.01f).setDelay(iDelay * 0.1f).setEase(LeanTweenType.easeInOutQuad);

			LeanTween.rotateZ(flower, 180f, 0.5f).setFrom(0f).setDelay(iDelay * 0.1f).setEase(LeanTweenType.easeInOutQuad);

			Vector3 endScale = ListScaleFlower[i];
			Vector3 fromScale = ListScaleFlower[i] * 0.1f;
			LeanTween.scale(flower, endScale, 0.5f).setFrom(fromScale).setDelay(iDelay * 0.1f).setEase(LeanTweenType.easeInOutQuad);

			LeanTween.value(gameObject, 0f, 1f, 0.5f).setEase(LeanTweenType.easeInOutQuad).setDelay(iDelay * 0.1f).setOnUpdate((float val) => {
				Color currentColor = flower.GetComponent<Renderer>().material.GetColor("_UnlitColor");
				currentColor.a = val;
				flower.GetComponent<Renderer>().material.SetColor("_UnlitColor", currentColor);
			});

			iDelay++;
		}

		Invoke("EndApparitionFlowers", 3f);
	}

	void EndApparitionFlowers()
	{
		ShowPanier(true);
		numFlowerCueillies = 0;
		countFlower.Clear();
	}

	void ShowPanier(bool bShow)
	{
		Vector3 endPosPanier, endPosFlowers;
		if (!bShow)
		{
			endPosPanier = new Vector3(0f, -6f, 0f);
			for (int i = 0; i < ListFlower.Count; i++)
			{
				endPosFlowers = new Vector3(ListFlower[i].transform.position.x, ListFlower[i].transform.position.y - 3.5f, ListFlower[i].transform.position.z);
				LeanTween.moveLocal(ListFlower[i], endPosFlowers, 1f).setEase(LeanTweenType.easeInOutQuad);
			}
		}
		else
		{
			endPosPanier = new Vector3(0f, -2.5f, 0f);
		}
		LeanTween.moveLocal(goPanier, endPosPanier, 1f).setEase(LeanTweenType.easeInOutQuad);
	}

	void RandomizeListePosFlower()
	{
		stockPos = new Vector2[25];
		for (int i = 0; i < ListPosFlower.Count; i++)
		{
			ListPosFlower[i] = GetNewRandValueNotInsideTab(i);
		}
	}

	Vector2 GetNewRandValueNotInsideTab(int i)
	{
		bool bAlreadyIn = false;
		int randX = Random.Range(-6, 6);
		int randY = Random.Range(0, 5);

		for (int j = 0; j < stockPos.Length; j++)
		{
			if (stockPos[j].x == randX && stockPos[j].y == randY)
			{
				bAlreadyIn = true;
				break;
			}
		}

		if (!bAlreadyIn)
		{
			stockPos[i] = new Vector2(randX, randY);
		}
		else
		{
			//récursivité !
			stockPos[i] = GetNewRandValueNotInsideTab(i);
		}

		return stockPos[i];
	}

}