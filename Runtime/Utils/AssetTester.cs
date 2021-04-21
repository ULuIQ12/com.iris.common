using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using com.iris.common;
using UnityEditor;

namespace com.iris.common
{
	public class AssetTester : MonoBehaviour
	{

		private static AssetTester Instance;
		public bool PlayMusic = false;
		public string autoPlayMusicDir = "D:/_music/Ghost/2015 - Meliora/";
		private AudioInterface ai;

		public void Awake()
		{
			if (Instance != null || SceneManager.GetSceneByName("AppLoading") != null )
				Destroy(gameObject);
		}

		IEnumerator Start()
		{
			if (autoPlayMusicDir != "" && PlayMusic)
			{

				while ((ai = GetComponent<AudioInterface>()) == null)
				{
					yield return null;
				}

				while (!ai.Initialized)
				{
					yield return null;
				}

				ai.LoadInternalAudio(autoPlayMusicDir);
				yield return new WaitForSeconds(1.5f);
				ai.Play();
			}
		}
	}
}