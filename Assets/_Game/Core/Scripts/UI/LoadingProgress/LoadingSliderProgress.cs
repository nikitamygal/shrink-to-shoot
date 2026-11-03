using UnityEngine;
using UnityEngine.UI;

namespace Game.Tools
{
	/// <summary>
	/// A very simple class, meant to be used within a MMSceneLoading screen,
	/// to update a Slider based on loading progress
	/// </summary>
	public class LoadingSliderProgress : MonoBehaviour
	{
		protected Slider _slider;

		/// <summary>
		/// On Awake we store our Slider
		/// </summary>
		protected virtual void Awake()
		{
			_slider = GetComponent<Slider>();

			if (_slider == null)
			{
				Debug.LogError($"{nameof(LoadingSliderProgress)} requires a Slider component.", this);
			}
		}

		/// <summary>
		/// Meant to be called by the MMSceneLoadingManager,
		/// turns the progress of a load into slider value
		/// </summary>
		public virtual void SetProgress(float newValue)
		{
			if (_slider == null)
			{
				return;
			}

			// MMSceneLoadingManager always sends progress in range [0..1]
			_slider.normalizedValue = newValue;
		}
	}
}
