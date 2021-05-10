using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
    public class AudioInterface : MonoBehaviour, IAudioPlayer
    {
		public static Action<string> OnFolderComplete;

		public void Awake()
		{
			InitPlayer();
		}
		public string GetTitle()
		{
			return Player.GetTitle();
		}
		public void LoadAudio(bool isRandom = false)
		{
			Player.LoadAudio();
		}

		public void LoadInternalAudio(string songDir = "", bool isRandom = false)
		{
			if (songDir != "")
				Player.LoadInternalAudio(songDir, isRandom);
			else
				LoadAudio(isRandom);
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

		public void PlayPause()
		{
			if (Player.IsPaused())
				Play();
			else
				Pause();
		}

		public bool IsPaused()
		{
			return Player.IsPaused();
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

		public float GetVolume()
		{
			return AudioListener.volume;
		}

		public void SetVolume(float value)
		{
			AudioListener.volume = value;
		}

		const string IOSPrefab = "AudioPlayer/IOSAudioPlayer";
		const string DesktopPrefab = "AudioPlayer/DesktopAudioPlayer";

		public IAudioPlayer Player;

		public bool Initialized { get; private set; } = false;

		private void InitPlayer()
		{
			if( Application.platform == RuntimePlatform.IPhonePlayer )// ||Application.platform == RuntimePlatform.WindowsEditor )
			{
				GameObject go = Instantiate(Resources.Load<GameObject>(IOSPrefab), transform);
				Player = go.GetComponent<IOSAudioPlayer>();
				IOSAudioPlayer p = Player as IOSAudioPlayer;
				
				go.name = "IOSAudioPlayer";
			}
			else if( Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer )
			{
				GameObject go = Instantiate(Resources.Load<GameObject>(DesktopPrefab), transform);
				Player = go.GetComponent<DesktopAudioPlayer>();
				DesktopAudioPlayer.OnFolderComplete += HandleOnFolderComplete;
				
				go.name = "DesktopAudioPlayer";
			}

			Initialized = true;
		}

		private void HandleOnFolderComplete(string folder)
		{
			OnFolderComplete?.Invoke(folder);
		}

		
	}
}
