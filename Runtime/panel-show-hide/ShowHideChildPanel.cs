using BeatThat.GetComponentsExt;
using BeatThat.CollectionsExt;
using BeatThat.Controllers;
using BeatThat.Properties;
using UnityEngine;
using BeatThat;
using BeatThat.ShowHidePanels;

namespace BeatThat.ShowHidePanels
{
	public class ShowHideChildPanel<T> : Subcontroller where T : IController
	{
		override protected void BindSubcontroller()
		{
			var show = this.AddIfMissing<Show> ();
			Bind (show.onValueChanged, this.OnShow);
			OnShow (show.value);
		}

		private void OnShow(bool show)
		{
			if (show) {
				var c = GetComponentInChildren<T>(true);

				if (c == null) {
					#if UNITY_EDITOR || DEBUG_UNSTRIP
					Debug.LogWarning("[" + Time.frameCount + "] no child of type " + typeof(T));
					#endif
					return;
				}

				c.Show (show);
			}
		}
	}
}



