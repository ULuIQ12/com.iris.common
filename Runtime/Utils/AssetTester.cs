using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using com.iris.common;

public class AssetTester : MonoBehaviour
{
	public bool PlayMusic = false;
	public string autoPlayMusicDir = "D:/_music/Ghost/2015 - Meliora/";
	private AudioInterface ai;
	// Start is called before the first frame update
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
