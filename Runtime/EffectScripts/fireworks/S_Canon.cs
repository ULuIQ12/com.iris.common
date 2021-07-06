using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{

    public class S_Canon : MonoBehaviour
    {
        public float JumpForce;
        public float AngleForce;
        public GameObject Rocket;

        private void Start()
        {
            //Rigidbody rgd = Rocket.GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Rocket.GetComponent<Rigidbody>().AddForce(new Vector3(AngleForce, JumpForce, 0), ForceMode.Impulse);
            }
        }
    }
}
