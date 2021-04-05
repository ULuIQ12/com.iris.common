using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
    public class DestroyOnPosition : MonoBehaviour
    {
        public enum COORDINATE
		{
			X, 
			Y, 
			Z
		}

		public enum CONDITION
		{
			SUPERIOR,
			INFERIOR
		}

		public COORDINATE coord;
		public CONDITION condition;
		public float Value;

		public void Update()
		{
			if( coord == COORDINATE.X)
			{
				if( condition == CONDITION.INFERIOR)
				{
					if( transform.position.x < Value)
					{
						Kill();
						return;
					}
				}
				else if( condition == CONDITION.SUPERIOR)
				{
					if( transform.position.x > Value)
					{
						Kill();
						return;
					}
				}
			}
			else if( coord == COORDINATE.Y)
			{
				if (condition == CONDITION.INFERIOR)
				{
					if (transform.position.y < Value)
					{
						Kill();
						return;
					}
				}
				else if (condition == CONDITION.SUPERIOR)
				{
					if (transform.position.y > Value)
					{
						Kill();
						return;
					}
				}
			}
			else if (coord == COORDINATE.Z)
			{
				if (condition == CONDITION.INFERIOR)
				{
					if (transform.position.z < Value)
					{
						Kill();
						return;
					}
				}
				else if (condition == CONDITION.SUPERIOR)
				{
					if (transform.position.z > Value)
					{
						Kill();
						return;
					}
				}
			}
		}

		private void Kill()
		{
			Destroy(gameObject);
		}
	}
}
