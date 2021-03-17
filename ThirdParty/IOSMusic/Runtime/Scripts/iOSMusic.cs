using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

namespace com.hodges.iosmusic
{
	[RequireComponent(typeof(AudioSource))]
	public class iOSMusic : MonoBehaviour
	{

		public static iOSMusic instance = null;
		private float[] musicData;

		[SerializeField]
		private AudioSource _audioSource;
		public AudioSource iOSMusicAudioSource
		{
			get { return _audioSource; }
			set { _audioSource = value; }
		}

		private AudioClip _audioClip;
		public AudioClip iOSMusicClip
		{
			get { return _audioClip; }
			set { _audioClip = value; }
		}

		private bool _hasAudioClipStartedPlaying;
		public bool HasAudioClipStartedPlaying
		{
			get { return _hasAudioClipStartedPlaying; }
			set { _hasAudioClipStartedPlaying = value; }
		}

		private bool _isAudioClipPaused;
		public bool IsAudioClipPaused
		{
			get { return _isAudioClipPaused; }
			set { _isAudioClipPaused = value; }
		}

		private bool _shouldAppendToPlaylist;
		public bool ShouldAppendToPlaylist
		{
			get { return _shouldAppendToPlaylist; }
			set { _shouldAppendToPlaylist = value; }
		}

		void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				Destroy(gameObject);
			}
			DontDestroyOnLoad(gameObject);
		}

		// Use this for initialization
		void Start()
		{
			_audioSource = GetComponent<AudioSource>();

			ShouldAppendToPlaylist = HasAudioClipStartedPlaying = false;

			musicManager.setupiOSMusic();
		}

		// Update is called once per frame
		void Update()
		{
			CheckAudioSourcePlayback();
		}

		void CheckAudioSourcePlayback()
		{
			// If an song playing via an Audio Source finishes playing, attempt to load the next song in the playlist via an Audio Source.
			if (HasAudioClipStartedPlaying && !iOSMusicAudioSource.isPlaying && !IsAudioClipPaused)
			{
				HasAudioClipStartedPlaying = false;
				musicManager.nextSong();
			}
		}

		public void PauseAudio()
		{
			iOSMusicAudioSource.Pause();
			IsAudioClipPaused = true;
		}

		public void UnPauseAudio()
		{
			iOSMusicAudioSource.UnPause();
			IsAudioClipPaused = false;
		}

		public void HandleAppendToggleChange(bool toggle)
		{
			// Handles the "Append to Playlist" toggle state change.
			ShouldAppendToPlaylist = toggle;
		}

		#region ShuffleMode

		public void RandomSongWithNativeAudioPlayer()
		{
			// get a random song from the user's library and play it with the native audio audio player
			musicManager.randomSongWithNativeAudioPlayer();
		}

		public void RandomSongWithAudioSource()
		{
			// get a random song from the user's library that is compatible with Audio Source mode and play it
			musicManager.randomSongWithAudioSource();
		}

		#endregion

		void ResetButtonStates()
		{
			if (GameObject.Find("Button - Next") && GameObject.Find("Button - Previous"))
			{
				GameObject.Find("Button - Next").GetComponent<Button>().interactable = true;
				GameObject.Find("Button - Previous").GetComponent<Button>().interactable = true;
			}
		}

		public void LoadAudioClip()
		{
			if (iOSMusicAudioSource.isPlaying)
			{
				iOSMusicAudioSource.Stop();
				Resources.UnloadUnusedAssets();

				if (iOSMusicAudioSource.clip != null)
				{
					iOSMusicAudioSource.clip = null;
				}
			}

			string path = Application.persistentDataPath.Substring(0, Application.persistentDataPath.Length - 5);
			path = path.Substring(0, path.LastIndexOf('/'));
			string songPath = path + "/Documents/" + "song" + ".m4a";
			StartCoroutine(LoadMusic(songPath));

		}

		IEnumerator LoadMusic(string songPath)
		{
			if (System.IO.File.Exists(songPath))
			{
				iOSMusicAudioSource.Stop();

				using (var uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + songPath, AudioType.AUDIOQUEUE))
				{
					((DownloadHandlerAudioClip)uwr.downloadHandler).streamAudio = true;

					yield return uwr.SendWebRequest();

					if (uwr.isNetworkError || uwr.isHttpError)
					{
						Debug.LogError(uwr.error);
						yield break;
					}

					DownloadHandlerAudioClip dlHandler = (DownloadHandlerAudioClip)uwr.downloadHandler;

					if (dlHandler.isDone)
					{
						AudioClip audioClip = dlHandler.audioClip;

						if (audioClip != null)
						{
							_audioClip = DownloadHandlerAudioClip.GetContent(uwr);

							// extract music data - optional
							//ExtractMusicData(_audioClip);

							iOSMusicAudioSource.clip = _audioClip;
							iOSMusicAudioSource.loop = false;
							iOSMusicAudioSource.Play();
							HasAudioClipStartedPlaying = true;
							Debug.Log("Playing song using Audio Source!");
							ResetButtonStates();
						}
						else
						{
							Debug.Log("Couldn't find a valid AudioClip :(");
						}
					}
					else
					{
						Debug.Log("The download process is not completely finished.");
					}
				}
			}
			else
			{
				Debug.Log("Unable to locate converted song file.");
			}
		}

		void ExtractMusicData(AudioClip songClip)
		{
			musicData = new float[songClip.samples * songClip.channels];

			Debug.Log("Extracting music data... found " + musicData.Length.ToString() + " samples!");

			// Use GetData() to access audio sample data
			songClip.GetData(musicData, 0);

			// do some processing
			//for (int i = 0; i < musicData.Length; ++i)
			//{
			//}
		}

		#region Metadata extraction

		void ExtractTitle(string title)
		{
			Debug.Log("Title: " + title);
		}

		void ExtractArtist(string artist)
		{
			Debug.Log("Artist: " + artist);
		}

		void ExtractAlbumTitle(string albumTitle)
		{
			Debug.Log("Album title: " + albumTitle);
		}

		void ExtractBPM(string bpm)
		{
			Debug.Log("BPM: " + bpm);
		}

		void ExtractGenre(string genre)
		{
			Debug.Log("Genre: " + genre);
		}

		void ExtractLyrics(string lyrics)
		{
			Debug.Log("Lyrics: " + lyrics);
		}

		void ExtractDuration(string duration)
		{
			Debug.Log("Duration: " + duration);
		}

		void ExtractArtwork()
		{
			Texture2D tex = null;
			byte[] fileData;
			string artworkPath = Application.persistentDataPath + "/songArtwork.png";

			if (File.Exists(artworkPath))
			{
				fileData = File.ReadAllBytes(artworkPath);
				tex = new Texture2D(2, 2);
				tex.LoadImage(fileData);

				// Convert newly created texture to a Sprite and assign it to the Canvas Image object.
				Sprite artworkSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 40);
				GameObject.Find("Image - Album Artwork").GetComponent<Image>().sprite = artworkSprite;
			}
		}

		#endregion

		void UserDidCancel()
		{
			ResetButtonStates();
			Debug.Log("User has cancelled the song selection.");
		}
	}
}
