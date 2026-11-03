using UnityEngine;
using Game.Sound;
using Game.Tools;
using VContainer;

namespace Game.Managers
{
	public class BackgroundMusic : InjectMonoBehaviour
	{
		[Inject] private readonly ISoundService _sound;

		/// the background music
		[Tooltip("the audio clip to use as background music")]
		public AudioClip SoundClip;
		/// whether or not the music should loop
		[Tooltip("whether or not the music should loop")]
		public bool Loop = true;
		/// the ID to create this background music with
		[Tooltip("the ID to create this background music with")]
		public int ID = 255;


		/// <summary>
		/// Starts the background music through the configured sound provider.
		/// </summary>
		protected virtual void Start()
		{
			_sound.PlayMusic(SoundClip, Loop, ID);
		}
	}
}
