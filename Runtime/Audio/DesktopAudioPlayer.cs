using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using Crosstales.FB;

namespace com.iris.common
{
	[RequireComponent(typeof(PlaylistMediaPlayer))]
    public class DesktopAudioPlayer : MonoBehaviour, IAudioPlayer
    {
		public static string[] SUPPORTED_AUDIO_EXT = new string[]
		{
			".mp3",
			".aac",
			".wav",
			".flac",
			".wma",
			".m4a"
		};

		private static DesktopAudioPlayer _Instance;


		public static IAudioPlayer GetInstance()
		{
			return _Instance;
		}

		void Awake()
		{
			if (_Instance != null)
			{
				Destroy(gameObject);
				return;
			}
			else
				_Instance = this;

			Init();
		}

		public AudioSource output1AS;
		public AudioSource output2AS;


		public void Play()
		{
			if (Player.Control != null)
				Player.Control.Play();
		}

		public void Stop()
		{
			if (Player.Control != null)
				Player.Control.Stop();
		}

		public void Pause()
		{			
			if (Player.Control != null)
				Player.Control.Pause();
		}

		public void UnPause()
		{
			
			if (Player.Control != null)
				Player.Control.Play();
		}

		public void LoadAudio()
		{
			if (PlayerInitialized)
			{
				Debug.Log("LLP=" + lastLoadPath);
				FileBrowser.Instance.OpenSingleFolderAsync("",(lastLoadPath != "") ? "" : null);
			}
			else
				throw new System.Exception("LoadAudio called before the player finished initializing !");
		}

		public void Next()
		{
			Player.NextItem();
		}

		public void Previous()
		{
			Player.PrevItem();
		}


		public void OnEnable()
		{
			FileBrowser.Instance.OnOpenFoldersComplete += HandleFolderComplete;
		}

		public void OnDisable()
		{
			FileBrowser.Instance.OnOpenFoldersComplete -= HandleFolderComplete;
		}

		private PlaylistMediaPlayer Player;
		private bool PlayerInitialized = false;
		private string lastLoadPath = "";
		
		private void Init()
		{
			lastLoadPath = Application.persistentDataPath + "/chanson";
			lastLoadPath = lastLoadPath.Replace("/", "\\");
			Player = GetComponent<PlaylistMediaPlayer>();
			if( Player == null )
				throw new System.Exception("No PlaylistMediaPlayer found on this gameObject");

			Player.Events.AddListener(OnMediaPlayerEvent);

			StartCoroutine(WaitForPlayerStartup());
		}

		private IEnumerator WaitForPlayerStartup()
		{
			YieldInstruction wait = new WaitForEndOfFrame();
			while (!MediaPlayer.s_GlobalStartup)
			{
				yield return wait;
			}
			Debug.Log("AudioListener volume = " + AudioListener.volume);
			PlayerInitialized = true;
		}

		private void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
		{
			Debug.Log("MPE : " + et);
			switch (et)
			{
				case MediaPlayerEvent.EventType.ReadyToPlay:
					break;
				case MediaPlayerEvent.EventType.Started:
					break;
				case MediaPlayerEvent.EventType.FirstFrameReady:
					break;
				case MediaPlayerEvent.EventType.MetaDataReady:
					break;
				case MediaPlayerEvent.EventType.FinishedPlaying:
					break;
				case MediaPlayerEvent.EventType.Error:
					//fonctionne pas
					break;
				case MediaPlayerEvent.EventType.StartedSeeking:
					break;
				case MediaPlayerEvent.EventType.FinishedSeeking:
					break;
				case MediaPlayerEvent.EventType.PlaylistItemChanged:
					if (Player.CurrentPlayer.gameObject.name == "MP1")
						AudioProcessor.SetAudioSource(output2AS);
					else
						AudioProcessor.SetAudioSource(output1AS);
					break;
			}
		}

		private void HandleFolderComplete(bool selected, string singleFolder, string[] folders)
		{
			if (singleFolder != "")
			{
				lastLoadPath = singleFolder;
				Debug.Log("HandleFolderComplete ->" + lastLoadPath);
				LoadPlaylist(singleFolder);
			}
		}

		private void LoadPlaylist(string directoryPath)
		{

			if( Player.Control != null )
				Player.Control.Stop();

			List<MediaPlaylist.MediaItem> items = new List<MediaPlaylist.MediaItem>();


			string[] files = Directory.GetFiles(directoryPath);

			if (files.Length == 0)
				return;

			for (int i = 0; i < files.Length; i++)
			{

				string ext = Path.GetExtension(files[i]);
				bool isAudio = false;
				foreach (string e in SUPPORTED_AUDIO_EXT)
				{
					if (e == ext)
					{
						isAudio = true;
						break;
					}
				}
				if (!isAudio)
					continue;

				MediaPlaylist.MediaItem mi = new MediaPlaylist.MediaItem();
				mi.fileLocation = MediaPlayer.FileLocation.AbsolutePathOrURL;
				mi.filePath = files[i];
				mi.autoPlay = false;
				mi.startMode = PlaylistMediaPlayer.StartMode.Immediate;
				mi.progressMode = PlaylistMediaPlayer.ProgressMode.OnFinish;
				mi.progressTimeSeconds = 0f;
				items.Add(mi);
			}

			//mpl.Items = items;


			Player.Playlist.Items = items;
			Player.PlaylistIndex = -1;
			Player.NextItem();
			
		}

	}
}
