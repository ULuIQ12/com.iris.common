using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace com.iris.common
{

	public class S_Spawn : MonoBehaviour
	{


		public GameObject Rocket_1;
		public GameObject Rocket_2;
		public GameObject Rocket_3;
		public GameObject Rocket_4;
		public GameObject Rocket_5;
		public GameObject Rocket_6;

		public List<GameObject> ListSpawn;

		public List<GameObject> ListSpawn_2;

		public List<GameObject> ListSpawn_3;

		public GameObject Spawn2Pos;

		private float randomAngle = 0;
		// private Vector3 Pos = new Vector3();
		private GameObject PrefabToSpawn;

		public int SpawnInterval_1;
		public int SpawnInterval_2;

		//public Vector3 PositionLowRandomRange = new Vector3();
		//public Vector3 PositionHighRandomRange = new Vector3();

		public float ScaleLowRandomRange = 1f;
		public float ScaleHighRandomRange = 1f;
		private float LastSpawnTime = 0f;
		// private Vector3 SpawnPosition = new Vector3();

		public float AngleForceMin_Gauche;
		public float AngleForceMax_Gauche;

		public float AngleForceMin_Droit;
		public float AngleForceMax_Droit;

		public float JumpForceMin;
		public float JumpForceMax;

		public VisualEffect Effect;

		private int anciennumber = 100;




		public void Update()
		{
			int randomSpawn = Random.Range(SpawnInterval_1, SpawnInterval_2);

			Effect.SendEvent("OnExplosion");


			if (Time.time - LastSpawnTime > randomSpawn)
			{

				int rnd = Random.Range(0, 5);

				if (rnd == 0 || rnd ==1)
				{
					for (int i = 0; i < 4; i++)
					{

						Spawn(ListSpawn[Random.Range(0, 1)]);
						Spawn(ListSpawn[Random.Range(2, 4)]);

					}
				}
				else if (rnd == 2 || rnd==3)
				{
					for (int i = 0; i < 4; i++)
					{

						Spawn_2(ListSpawn_2[Random.Range(0, 1)]);
						Spawn_2(ListSpawn_2[Random.Range(2, 4)]);

					}
				}
				else if (rnd==4)
				{
					Spawn_3(ListSpawn_3);
				}

				LastSpawnTime = Time.time;
			}		

		}

		private void Spawn(GameObject Pos)
		{
			int randomnumber = Random.Range(1, 5);


			if (randomnumber == anciennumber)
			{
				randomnumber = Random.Range(1, 5);
			}
			else
				if (randomnumber == 1)
			{
				PrefabToSpawn = Rocket_1;

			}
			else if (randomnumber == 2)
			{
				PrefabToSpawn = Rocket_2;
			}

			else if (randomnumber == 3)
			{
				PrefabToSpawn = Rocket_3;
			}
			else if (randomnumber == 4)
			{
				PrefabToSpawn = Rocket_4;
			}
			else if (randomnumber == 5)
			{
				PrefabToSpawn = Rocket_5;

			}
			anciennumber = randomnumber;

			float randomJump = Random.Range(JumpForceMin, JumpForceMax);

			if (Pos.CompareTag("Spawn_Gauche"))
			{
				randomAngle = Random.Range(AngleForceMin_Gauche, AngleForceMax_Gauche);

			}
			else
			{
				randomAngle = Random.Range(AngleForceMin_Droit, AngleForceMax_Droit);

			}

			//SpawnPosition.Set(Random.Range(PositionLowRandomRange.x, PositionHighRandomRange.x), Random.Range(PositionLowRandomRange.y, PositionHighRandomRange.y), Random.Range(PositionLowRandomRange.z, PositionHighRandomRange.z));


			GameObject go = Instantiate(PrefabToSpawn, Pos.transform.position, Quaternion.identity);
			//go.transform.localPosition = SpawnPosition;
			//go.transform.rotation = Random.rotation;
			go.GetComponent<Rigidbody>().AddForce(new Vector3(randomAngle, randomJump, 0), ForceMode.Impulse);





		}

		private void Spawn_2(GameObject Pos)
		{
			
			PrefabToSpawn = Rocket_5;

			int randomnumber = Random.Range(1, 5);


			if (randomnumber == anciennumber)
			{
				randomnumber = Random.Range(1, 5);
			}
			else
			{
				if (randomnumber == 1)
				{
					PrefabToSpawn = Rocket_1;

				}
				else if (randomnumber == 2)
				{
					PrefabToSpawn = Rocket_2;
				}

				else if (randomnumber == 3)
				{
					PrefabToSpawn = Rocket_3;
				}
				else if (randomnumber == 4)
				{
					PrefabToSpawn = Rocket_4;
				}
				else if (randomnumber == 5)
				{
					PrefabToSpawn = Rocket_5;

				}
			}
			anciennumber = randomnumber;

	
			GameObject go = Instantiate(PrefabToSpawn, Pos.transform.position, Quaternion.identity);
			float randomJump = Random.Range(JumpForceMin, JumpForceMax);
			go.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1,1), Random.Range(22, 25), 0), ForceMode.Impulse);

			

		}

		private void Spawn_3(List<GameObject> Liste)
		{

			PrefabToSpawn = Rocket_6;

			int Angle = 8;


			for (int i = 0; i < Liste.Count; i++)
			{
				GameObject go = Instantiate(PrefabToSpawn, Liste[i].transform.position, Quaternion.identity);
				float randomJump = Random.Range(JumpForceMin, JumpForceMax);
				go.GetComponent<Rigidbody>().AddForce(new Vector3(Angle, Random.Range(22, 25), 0), ForceMode.Impulse);

				Angle = -8;

			}
		}
	}
}


