using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFX_Test : MonoBehaviour
{
    public VisualEffect Effect;

    public GameObject Rocket;

    public GameObject ZoneExplosion;

    public bool CanExplose = false;

    public float timeLeft = 10.0f;



    private void Update()
    {
        Effect.SendEvent("OnFire");

        if (Rocket.GetComponent<Rigidbody>().isKinematic == true)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                Rocket.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }

        
    private void OnMouseDown()
    {
        if (CanExplose)
        {          

            Effect.SendEvent("OnExplosion");

            Effect.SetFloat("Particule_Size", 0f);

            Effect.SetFloat("Trail_Size", 0f);

            

            Rocket.GetComponent<Rigidbody>().isKinematic = true;

            Rocket.GetComponent<SphereCollider>().enabled = false;

        }

    }

    private void OnTriggerEnter(Collider ZoneExplosion)
    {
        CanExplose = true;
    }
    
    private void OnCollisionEnter(Collision other)
    {
       
        if (CanExplose && other.gameObject.CompareTag("Player"))
        {
            Effect.SendEvent("OnExplosion");

            Effect.SetFloat("Particule_Size", 0f);

            Rocket.GetComponent<Rigidbody>().isKinematic = true;

            Rocket.GetComponent<SphereCollider>().enabled = false;
        }        
    }

}
