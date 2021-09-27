using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DentedPixel;

namespace com.iris.common
{
	public class TriggerPoint : MonoBehaviour
	{
		void OnTriggerEnter(Collider other)
		{
			ManagerCraie.HandTrigger(other.gameObject);
		}
	}
}