using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace com.hodges.iosmusic
{
	public class iOSMusicGUI : MonoBehaviour
	{

		public void ShowMusicLibrary()
		{
			// If the plugin is currently playing a song via an Audio Source, stop playback.
			if (iOSMusic.instance.iOSMusicAudioSource.isPlaying)
			{
				iOSMusic.instance.iOSMusicAudioSource.Stop();
				iOSMusic.instance.HasAudioClipStartedPlaying = false;
				iOSMusic.instance.IsAudioClipPaused = false;
			}

			// Open Music library and play song with native player.
			musicManager.openNativeMusicPicker(iOSMusic.instance.ShouldAppendToPlaylist);
		}

		public void PlayPause()
		{
			// If the native player is currently playing, make it pause. If it is currently paused, resume playing.
			musicManager.playPause();
		}

		public void PauseAudioSource()
		{
			iOSMusic.instance.PauseAudio();
		}

		public void UnPauseAudioSource()
		{
			iOSMusic.instance.UnPauseAudio();
		}

		public void LoadAudioClip()
		{
			// Pause the native player if it is playing a song.
			PlayPause();

			iOSMusic.instance.HasAudioClipStartedPlaying = false;
			iOSMusic.instance.IsAudioClipPaused = false;
			DisableButtons();

			// Open Music library and play selection as Audio Clip.
			musicManager.loadAudioClip(iOSMusic.instance.ShouldAppendToPlaylist);
		}

		public void NextSong()
		{
			iOSMusic.instance.HasAudioClipStartedPlaying = false;
			iOSMusic.instance.IsAudioClipPaused = false;
			DisableButtons();

			musicManager.nextSong();
		}

		public void PreviousSong()
		{
			iOSMusic.instance.HasAudioClipStartedPlaying = false;
			iOSMusic.instance.IsAudioClipPaused = false;
			DisableButtons();

			musicManager.previousSong();
		}

		public void RandomSongWithNativeAudioPlayer()
		{
			musicManager.randomSongWithNativeAudioPlayer();
		}

		public void RandomSongWithAudioSource()
		{
			iOSMusic.instance.HasAudioClipStartedPlaying = false;
			iOSMusic.instance.IsAudioClipPaused = false;
			DisableButtons();

			musicManager.randomSongWithAudioSource();
		}

		void DisableButtons()
		{
			// Prevent user from tapping "Next" or "Previous" while a song is in the process of loading.
			GameObject.Find("Button - Next").GetComponent<Button>().interactable = false;
			GameObject.Find("Button - Previous").GetComponent<Button>().interactable = false;
		}
	}
}
