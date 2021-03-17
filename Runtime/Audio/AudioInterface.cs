using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
    public class AudioInterface : MonoBehaviour, IAudioPlayer
    {

		public void Awake()
		{
			InitPlayer();
		}

		public void LoadAudio()
		{
			Player.LoadAudio();
		}

		public void Next()
		{
			Player.Next();
		}

		public void Pause()
		{
			Player.Pause();
		}

		public void Play()
		{
			Player.Play();
		}

		public void Previous()
		{
			Player.Previous();
		}

		public void Stop()
		{
			Player.Stop();
		}

		public void UnPause()
		{
			Player.UnPause();
		}

		const string IOSPrefab = "AudioPlayer/IOSAudioPlayer";
		const string DesktopPrefab = "AudioPlayer/DesktopAudioPlayer";

		private IAudioPlayer Player;
		private void InitPlayer()
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer)
			{
				GameObject go = Instantiate(Resources.Load<GameObject>(IOSPrefab), transform);
				Player = go.GetComponent<IOSAudioPlayer>();
				go.name = "IOSAudioPlayer";
			}
			else if( Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer )
			{
				GameObject go = Instantiate(Resources.Load<GameObject>(DesktopPrefab), transform);
				Player = go.GetComponent<DesktopAudioPlayer>();
				go.name = "DesktopAudioPlayer";
			}
		}
    }
}
