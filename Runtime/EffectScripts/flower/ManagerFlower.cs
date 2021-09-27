using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using com.rfilkov.kinect;
using com.iris.common;
//using DentedPixel;


public class ManagerFlower : MonoBehaviour
{

	public Transform LeftHand;
	public Transform RightHand;
	/*
	public float defaultX = 0f;
	public float scaleX = 5f;
	public float defaultY = 0f;
	public float scaleY = 5f;
	*/
	public static ManagerFlower Instance;

	//public float scaleGlobal = 1f;
	//public float width = 1600f;
	//public float height = 1200f;
	public GameObject[] bHandRight;
	public GameObject[] bHandLeft;
	public Color[] tabColor;
	public Camera foregroundCamera;

	public GameObject goPanier;
	public GameObject goFlowers;

	public Transform BasketTarget;

	//KinectManager kinectManager;
	//KinectInterop.JointType JointHandRight = KinectInterop.JointType.HandRight;
	//KinectInterop.JointType JointHandLeft = KinectInterop.JointType.HandLeft;
	List<ulong> allUserIds;

	Vector3[] stockPos;
	List<GameObject> ListFlower = new List<GameObject>();
	List<Vector3> ListPosFlower = new List<Vector3>();
	List<Vector3> ListScaleFlower = new List<Vector3>();
	List<int> countFlower = new List<int>();
	int numFlowerCueillies = 0;
	Rect backgroundRect;


	void Start()
	{
		StartCoroutine(WaitForFrame());

	}

	private IEnumerator WaitForFrame()
	{
		yield return new WaitForSeconds(0.5f);
		/* REDO
		while (KinectManager.Instance == null || !KinectManager.Instance.IsInitialized())
		{
			yield return null;
		}
		*/
		Init();
	}

	private void Init()
	{
		/* REDO
		kinectManager = KinectManager.Instance;
		*/
		Instance = this;

		for (int i = 0; i < goFlowers.transform.childCount; i++)
		{
			Transform t = goFlowers.transform.GetChild(i);
			ListFlower.Add(t.gameObject);
			ListPosFlower.Add(t.position);
			ListScaleFlower.Add(t.localScale);
		}


		HandTrigger htLeft = LeftHand.gameObject.GetComponent<HandTrigger>();
		htLeft.OnHandCollide += OnHandCollide;
		HandTrigger htRight = RightHand.gameObject.GetComponent<HandTrigger>();
		htRight.OnHandCollide += OnHandCollide;
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
				if (Instance.numFlowerCueillies >= Instance.ListFlower.Count)
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

		//Vector3 endPos = new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(-3f, -2.1f), 0f);
		Vector3 endPos = new Vector3(BasketTarget.position.x + Random.Range(-0.25f, 0.25f), BasketTarget.position.y + Random.Range(-0.25f, -0.25f), BasketTarget.position.z);
		//LeanTween.moveLocal(flower, endPos, 2f).setEase(LeanTweenType.easeInOutQuad);
		LeanTween.move(flower, endPos, 2f).setEase(LeanTweenType.easeInOutQuad);

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
			LeanTween.move(flower, endPos, 0.01f).setDelay(iDelay * 0.1f).setEase(LeanTweenType.easeInOutQuad);

			LeanTween.rotateZ(flower, 180f, 0.5f).setFrom(0f).setDelay(iDelay * 0.1f).setEase(LeanTweenType.easeInOutQuad);

			Vector3 endScale = ListScaleFlower[i];
			Vector3 fromScale = ListScaleFlower[i] * 0.1f;
			LeanTween.scale(flower, endScale, 0.5f).setFrom(fromScale).setDelay(iDelay * 0.1f).setEase(LeanTweenType.easeInOutQuad);

			LeanTween.value(gameObject, 0f, 1f, 0.5f).setEase(LeanTweenType.easeInOutQuad).setDelay(iDelay * 0.1f).setOnUpdate((float val) => {
				Color currentColor = flower.GetComponent<Renderer>().material.GetColor("_BaseColor");
				currentColor.a = val;
				flower.GetComponent<Renderer>().material.SetColor("_BaseColor", currentColor);
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
			endPosPanier = new Vector3(0f, -6f, BasketTarget.position.z);
			for (int i = 0; i < ListFlower.Count; i++)
			{
				endPosFlowers = new Vector3(ListFlower[i].transform.position.x, ListFlower[i].transform.position.y - 3.5f, ListFlower[i].transform.position.z);
				LeanTween.moveLocal(ListFlower[i], endPosFlowers, 1f).setEase(LeanTweenType.easeInOutQuad);
			}
		}
		else
		{
			endPosPanier = new Vector3(0f, -4.6f, BasketTarget.position.z);
		}
		LeanTween.moveLocal(goPanier, endPosPanier, 1f).setEase(LeanTweenType.easeInOutQuad);
	}

	void RandomizeListePosFlower()
	{
		stockPos = new Vector3[25];
		for (int i = 0; i < ListPosFlower.Count; i++)
		{
			ListPosFlower[i] = GetNewRandValueNotInsideTab(i);
		}
	}

	Vector3 GetNewRandValueNotInsideTab(int i)
	{
		bool bAlreadyIn = false;
		float amp = Remap(0f, 1f, 0.5f, 1.0f, FXDataProvider.GetFloat(FXDataProvider.FLOAT_DATA_TYPE.AmplitudeSetting) );

		float randX = Random.Range(-5.5f * amp, 5.5f * amp);
		float randY = Random.Range(-2f * amp, 4f * amp);

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
			stockPos[i] = new Vector3(randX, randY, 10f);
		}
		else
		{
			//récursivité !
			stockPos[i] = GetNewRandValueNotInsideTab(i);
		}

		return stockPos[i];
	}

	private float Remap(float InputLow, float InputHigh, float OutputLow, float OutputHigh, float value )
	{
		value = Mathf.InverseLerp(InputLow, InputHigh, value);
		value = Mathf.Lerp(OutputLow, OutputHigh, value);

		return value;
	}

}