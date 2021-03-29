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
		}

		public enum AMPLITUDE
		{
			Small, 
			Large
		}

		public string uid = "UNIQUE_ID";
		public string Title = "Titre de l'effet";
		public string Instructions = "Instructions effet";
		public string Author = "Your name here";
		public long CreationDate = 0;
		public long LastUpdateDate = 0;
		public Texture2D ThumbnailTexture;
		//public MOTRICITY MotricityLevel = MOTRICITY.Level1;
		public AMPLITUDE AmplitudeLevel = AMPLITUDE.Large;


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
