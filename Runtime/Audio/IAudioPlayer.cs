using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.iris.common
{
    public interface IAudioPlayer
    {
		void Play();
		void Stop();
		void Pause();
		void UnPause();
		
		void Next();
		void Previous();

		void LoadAudio();
		void LoadInternalAudio(string songDir);

		
    }
}
