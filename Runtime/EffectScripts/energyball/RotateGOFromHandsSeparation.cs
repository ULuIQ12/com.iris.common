using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.iris.common;

namespace com.iris.common
{
	public enum MODE
	{
		ThreeD, 
		TwoD
	}

    public class RotateGOFromHandsSeparation : MonoBehaviour
    {
		public float multiplier = 1.0f;
		public MODE mode = MODE.ThreeD;
		private Transform refTransform;
        // Start is called before the first frame update
        void Start()
        {
			refTransform = gameObject.GetComponent<Transform>();
        }

        // Update is called once per frame
        void Update()
        {
			if( CVInterface._Instance && refTransform != null)
			{
				float sep;
				if(mode == MODE.ThreeD )
					sep = CVInterface.GetFloat(FXDataProvider.FLOAT_DATA_TYPE.HandsVerticalSeparation);
				else
					sep = CVInterface.GetFloat(FXDataProvider.FLOAT_DATA_TYPE.HandsVerticalSeparation2D);

				refTransform.Rotate(0.0f, sep * multiplier, 0.0f);
			}
        }
    }
}
