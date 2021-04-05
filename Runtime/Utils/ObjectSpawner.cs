using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
    public class ObjectSpawner : MonoBehaviour
    {
		public GameObject PrefabToSpawn;
		public float SpawnInterval;
		public Vector3 PositionLowRandomRange = new Vector3();
		public Vector3 PositionHighRandomRange = new Vector3();
		public float ScaleLowRandomRange = 1f;
		public float ScaleHighRandomRange = 1f;
		private float LastSpawnTime = 0f;
		private Vector3 SpawnPosition = new Vector3();
		private Vector3 SpawnScale = new Vector3();
		
		public void Update()
		{
			if( Time.time - LastSpawnTime > SpawnInterval)
			{
				Spawn();
			}
		}

		private void Spawn()
		{
			//Random.rotation
			SpawnPosition.Set(Random.Range(PositionLowRandomRange.x, PositionHighRandomRange.x), Random.Range(PositionLowRandomRange.y, PositionHighRandomRange.y), Random.Range(PositionLowRandomRange.z, PositionHighRandomRange.z));
			float randomScale = Random.Range(ScaleLowRandomRange, ScaleHighRandomRange);
			SpawnScale.Set(randomScale, randomScale, randomScale);
			GameObject go = Instantiate(PrefabToSpawn, transform, false);
			go.transform.localPosition = SpawnPosition;
			go.transform.rotation = Random.rotation;
			go.transform.localScale = SpawnScale;

			LastSpawnTime = Time.time;
		}
	}
}
