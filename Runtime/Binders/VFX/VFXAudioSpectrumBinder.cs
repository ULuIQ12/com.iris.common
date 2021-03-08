using System.Collections;
using System.Collections.Generic;
using System;	
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace com.iris.common
{
	[AddComponentMenu("IRIS/VFX/Audio Spectrum Binder")]
	[VFXBinder("IRIS/Audio Spectrum to AttributeMap")]
	class VFXAudioSpectrumBinder : VFXBinderBase
	{
		public string NbSamplesProperty { get { return (string)m_NbSamplesProperty; } set { m_NbSamplesProperty = value; } }
		[VFXPropertyBinding("System.UInt32"), SerializeField, UnityEngine.Serialization.FormerlySerializedAs("m_SamplesParameter")]
		protected ExposedProperty m_NbSamplesProperty = "Samples";

		public string TextureProperty { get { return (string)m_TextureProperty; } set { m_TextureProperty = value; } }
		[VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField, UnityEngine.Serialization.FormerlySerializedAs("m_TextureParameter")]
		protected ExposedProperty m_TextureProperty = "SpectrumTexture";

		private FFTWindow FFTWindow = FFTWindow.BlackmanHarris;
		public uint NbSamples = 64;

		private Texture2D m_Texture;
		private float[] m_AudioCache;
		private Color[] m_ColorCache;

		public override bool IsValid(VisualEffect component)
		{
			
			bool texture = component.HasTexture(TextureProperty);
			bool count = component.HasUInt(NbSamplesProperty);

			return texture && count;
		}

		void UpdateTexture()
		{
			if (m_Texture == null || m_Texture.width != NbSamples)
			{
				m_Texture = new Texture2D((int)NbSamples, 1, TextureFormat.RFloat, false);
				m_AudioCache = new float[NbSamples];
				m_ColorCache = new Color[NbSamples];
			}

			AudioListener.GetSpectrumData(m_AudioCache, 0, FFTWindow);

			for (int i = 0; i < NbSamples; i++)
			{
				m_ColorCache[i] = new Color(m_AudioCache[i], 0, 0, 0);
			}

			m_Texture.SetPixels(m_ColorCache);
			m_Texture.name = "AudioSpectrum" + NbSamples;
			m_Texture.Apply();
		}

		public override void UpdateBinding(VisualEffect component)
		{
			UpdateTexture();
			component.SetTexture(TextureProperty, m_Texture);
			component.SetUInt(NbSamplesProperty, NbSamples);
		}

		public override string ToString()
		{
			return string.Format("IRIS: Audio Spectrum");
		}
	}
}
