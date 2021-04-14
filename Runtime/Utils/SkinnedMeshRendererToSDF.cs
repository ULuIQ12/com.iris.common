using System;
using UnityEngine;
using UnityEngine.VFX;

namespace com.iris.common
{
    public class SkinnedMeshRendererToSDF : MonoBehaviour
    {
		public Vector3 resolution = new Vector3(64, 32, 32);
		public Transform _CollidersRoot = null;
		public BoxCollider Zone;
		public string VFXPropertyName = "MeshSDF";
		public VisualEffect Vfx;

		public Collider[] Colliders;
		private Texture3D SDF;
		private byte[] Datas;

		private bool Initialized = false;

		private Vector3 vCenter = new Vector3();
		private Vector3 offset = new Vector3();

		public void Start()
		{
			CreateTexture();
			GetColliders();
			CreateDataArray();

			UpdateData();

			Initialized = true;
		}

		public void Update()
		{
			if (!Initialized)
				return;

			UpdateData();
		}

		private void CreateTexture()
		{
			SDF = new Texture3D((int)resolution.x, (int)resolution.y, (int)resolution.z, TextureFormat.RFloat, false);
			SDF.filterMode = FilterMode.Point;
			SDF.wrapMode = TextureWrapMode.Clamp;
		}

		private void GetColliders()
		{
			Colliders = _CollidersRoot.GetComponentsInChildren<Collider>();
		}

		private void CreateDataArray()
		{
			int count = (int)resolution.x * (int)resolution.y * (int)resolution.z * 4;
			Datas = new byte[count];

			vCenter.x = (Zone.size.x / resolution.x) / 2f;
			vCenter.y = (Zone.size.y / resolution.y) / 2f;
			vCenter.z = (Zone.size.z / resolution.z) / 2f;

			offset.x = Zone.center.x + vCenter.x;
			offset.y = Zone.center.y + vCenter.y;
			offset.z = Zone.center.z + vCenter.z;
		}
		private float MAXDIST = 5f;
		private void UpdateData()
		{
			//Debug.Log("Update Start : " + Time.time);
			Vector3 p = new Vector3();
			int index = 0;
			int i, j, k = 0;
			byte[] tempByte;
			//MAXDIST = vCenter.x * 2f;

			for ( i=0;i<resolution.z;i++)
			{
				p.z = offset.z + ( (i/resolution.z) -0.5f ) * Zone.size.z;

				for (j=0;j<resolution.y;j++)
				{
					p.y = offset.y + ( (j/resolution.y) - 0.5f) * Zone.size.y;

					for (k=0;k<resolution.x;k++)
					{
						p.x = offset.x + ( (k/resolution.x) - 0.5f) * Zone.size.x;

						float dist = MAXDIST;
						foreach (Collider c in Colliders)
						{

							Vector3 res = Physics.ClosestPoint(p, c,c.transform.position, c.transform.rotation);

							float d = Vector3.Distance(p, res);
							d = Mathf.Clamp(d, 0, MAXDIST) ;
							if (d < dist)
								dist = d;
						}
						
						tempByte = BitConverter.GetBytes(dist / MAXDIST);
						Datas[index + 0] = tempByte[0];
						Datas[index + 1] = tempByte[1];
						Datas[index + 2] = tempByte[2];
						Datas[index + 3] = tempByte[3];

						index +=4;
					}
				}
			}

			SDF.SetPixelData(Datas, 0);
			SDF.Apply();

			if( Vfx != null)
			{
				if (Vfx.HasTexture(VFXPropertyName))
					Vfx.SetTexture(VFXPropertyName, SDF);
			}
		}

	}
}
