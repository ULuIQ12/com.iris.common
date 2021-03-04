using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace com.iris.common
{
	[CreateAssetMenu(fileName = "Settings", menuName = "IRIS/ExperienceSettings", order = 1)]
	public class ExpSettings : ScriptableObject
	{
		public enum MOTRICITY
		{
			Level1,
			Level2,
			Level3,
			Level4,
			Level5

		}

		public string Title = "Titre de l'effet";
		public string Instructions = "Instructions effet";
		public string Author = "Your name here";
		public long CreationDate = 0;
		public long LastUpdateDate = 0;
		public MOTRICITY MotricityLevel = MOTRICITY.Level1;


		public void Awake()
		{
			if (CreationDate == 0)
			{
				CreationDate = DateTime.Now.ToBinary();
				LastUpdateDate = CreationDate;
			}

		}

	}
}
