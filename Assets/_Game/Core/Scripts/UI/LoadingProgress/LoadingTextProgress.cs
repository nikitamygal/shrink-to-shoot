using System.Globalization;
using UnityEngine;
using TMPro;
using MoreMountains.Tools;

namespace Game.UI
{
	/// <summary>
	/// A simple class meant to be used within a MMSceneLoading screen,
	/// to update a TextMeshPro text based on loading progress
	/// </summary>
	public class LoadingTextProgress : MonoBehaviour
	{
		/// the value to which the progress' zero value should be remapped to
		[Tooltip("The value to which the progress' zero value should be remapped to")]
		public float RemapMin = 0f;

		/// the value to which the progress' one value should be remapped to
		[Tooltip("The value to which the progress' one value should be remapped to")]
		public float RemapMax = 100f;

		/// the amount of decimals to display
		[Tooltip("The amount of decimals to display")]
		public int NumberOfDecimals = 0;

		protected TextMeshProUGUI _text;

		/// <summary>
		/// On Awake we grab our TMP_Text and store it
		/// </summary>
		protected virtual void Awake()
		{
			_text = GetComponent<TextMeshProUGUI>();

			if (_text == null)
			{
				Debug.LogError($"{nameof(LoadingTextProgress)} requires a TextMeshProUGUI component.", this);
			}
			SetProgress(0f);
		}

		/// <summary>
		/// Updates the text with the progress value
		/// </summary>
		public virtual void SetProgress(float newValue)
		{
			if (_text == null)
			{
				return;
			}

			float remappedValue = MMMaths.Remap(newValue, 0f, 1f, RemapMin, RemapMax);
			float displayValue = MMMaths.RoundToDecimal(remappedValue, NumberOfDecimals);

			string valueString = displayValue.ToString(CultureInfo.InvariantCulture);
			_text.text = $"Loading {valueString}%";
		}
	}
}
