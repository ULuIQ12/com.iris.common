using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
	
    public class HandTrigger : MonoBehaviour
    {
		public Action<GameObject> OnHandCollide;

		void OnTriggerEnter(Collider other)
		{
			if( other.gameObject.GetComponent<HandTrigger>() == null)
			{
				OnHandCollide?.Invoke(other.gameObject);
			}
			//if (other.gameObject.CompareTag("Flower") )
			/*
			if (true)
			{
				//Destroy(other.gameObject);
				if (!ListFlower.Contains(other.gameObject))
				{
					ListFlower.Add(other.gameObject);
					DisappearFlower(other.gameObject);
					numFlowerCueillies++;
					Debug.Log("fleur cueillies N° " + numFlowerCueillies + " sur " + totFlower);
					if (numFlowerCueillies >= totFlower)
					{
						Invoke("AppearFlowers", 3);
					}
				}
			}*/
		}
	}
}
