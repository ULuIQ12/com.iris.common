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
    KinectManager kinectManager;
    KinectInterop.JointType JointHandRight = KinectInterop.JointType.HandRight;
	KinectInterop.JointType JointHandLeft = KinectInterop.JointType.HandLeft;
    List<GameObject> ListFlower = new List<GameObject>();
    int numFlowerCueillies = 0;

    void OnEnable()
    {
        kinectManager = KinectManager.Instance;
		totFlower = 25;// GameObject.FindGameObjectsWithTag("Flower").Length;

		HandTrigger htLeft = LeftHand.gameObject.GetComponent<HandTrigger>();
		htLeft.OnHandCollide += OnHandCollide;
		HandTrigger htRight = RightHand.gameObject.GetComponent<HandTrigger>();
		htRight.OnHandCollide += OnHandCollide;
	}

    void Update()
    {
        if (kinectManager && kinectManager.IsInitialized() && kinectManager.GetUsersCount() >0)
        {
            //allUserIds = kinectManager.GetAllUserIds();
            //for(int i = 0; i<allUserIds.Count; i++ )
            //{ Vector3 pos1 = kinectManager.GetJointPosition( allUserIds[i], JointHandRight); }

            ulong uid = kinectManager.GetUserIdByIndex(0);
            Vector3 HandPosition = kinectManager.GetJointPosition(uid, JointHandRight);
			if( RightHand != null )
				RightHand.position = new Vector3(defaultX+HandPosition.x*scaleX, defaultY+HandPosition.y*scaleY, 0);

			HandPosition = kinectManager.GetJointPosition(uid, JointHandLeft);
			if (LeftHand != null)
				LeftHand.position = new Vector3(defaultX + HandPosition.x * scaleX, defaultY + HandPosition.y * scaleY, 0);

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

	public void OnHandCollide( GameObject go )
	{
		if (!ListFlower.Contains(go))
		{
			ListFlower.Add(go);
			DisappearFlower(go);
			numFlowerCueillies++;
			Debug.Log("fleur cueillies N° " + numFlowerCueillies + " sur " + totFlower);
			if (numFlowerCueillies >= totFlower)
			{
				Invoke("AppearFlowers", 3);
			}
		}
	}

	/*
    void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.CompareTag("Flower") )
		if (true)
			{
            //Destroy(other.gameObject);
            if(!ListFlower.Contains(other.gameObject)){
                ListFlower.Add(other.gameObject);
                DisappearFlower(other.gameObject);
                numFlowerCueillies ++;
                Debug.Log("fleur cueillies N° "+numFlowerCueillies+" sur "+totFlower);
                if(numFlowerCueillies >= totFlower){
                    Invoke("AppearFlowers", 3);
                }
            }
        }
    }*/

    void DisappearFlower(GameObject flower)
    {
        
        float startAngle = flower.transform.rotation.eulerAngles.z;
        float endAngle = startAngle + 720.0f;
        LeanTween.rotateZ( flower, endAngle, 2f).setEase(LeanTweenType.easeInOutQuad);
        
        Vector3 endPos = new Vector3(flower.transform.localPosition.x + 5.0f, flower.transform.localPosition.y + 1.0f, flower.transform.localPosition.z);
        Debug.Log("ca réapparait !! "+flower+ "  "+flower.transform.position + "   " + endPos);
        LeanTween.moveLocal( flower, endPos, 2f).setEase(LeanTweenType.easeInOutQuad);

        Vector3 startScale = flower.transform.localScale;
        Vector3 endScale = startScale*2f;
        LeanTween.scale( flower, endScale, 2f).setEase(LeanTweenType.easeInOutQuad);

        LeanTween.value( gameObject, 1f, 0f, 1f).setEase(LeanTweenType.easeInOutQuad).setDelay(1f).setOnUpdate( (float val)=>{
            Color currentColor = flower.GetComponent<Renderer>().material.GetColor("_BaseColor");
            currentColor.a = val;
            flower.GetComponent<Renderer>().material.SetColor("_BaseColor", currentColor);
        });
        

    }

    void AppearFlowers()
    {   
        float iDelay = 0;
        foreach(GameObject flower in ListFlower)
        {
            Vector3 endPos = new Vector3(flower.transform.localPosition.x - 5.0f, flower.transform.localPosition.y - 1.0f, flower.transform.localPosition.z);
            //Debug.Log("ca réapparait !! "+flower + "   "  +flower.transform.position + "   " + endPos);
            LeanTween.moveLocal( flower, endPos, 0.01f).setDelay(iDelay*0.2f).setEase(LeanTweenType.easeInOutQuad);

            float startAngle = flower.transform.rotation.eulerAngles.z;
            float endAngle = startAngle + 180.0f;
            LeanTween.rotateZ( flower, endAngle, 0.5f).setDelay(iDelay*0.2f).setEase(LeanTweenType.easeInOutQuad);

            Vector3 startScale = flower.transform.localScale;
            Vector3 endScale = startScale*0.5f;
            Vector3 fromScale = startScale*0.1f;
            LeanTween.scale( flower, endScale, 0.5f).setFrom(fromScale).setDelay(iDelay*0.2f).setEase(LeanTweenType.easeInOutQuad);

            LeanTween.value( gameObject, 0f, 1f, 0.5f).setEase(LeanTweenType.easeInOutQuad).setDelay(iDelay*0.2f).setOnUpdate( (float val)=>{
                Color currentColor = flower.GetComponent<Renderer>().material.GetColor("_BaseColor");
                currentColor.a = val;
                flower.GetComponent<Renderer>().material.SetColor("_BaseColor", currentColor);
            } );    

            iDelay ++;        
        }

        numFlowerCueillies = 0;
        ListFlower.Clear();
    }

}