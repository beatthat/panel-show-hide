using BeatThat.GetComponentsExt;
using BeatThat.CollectionsExt;
using BeatThat.Controllers;
using BeatThat.StateControllers;
using UnityEngine;

namespace BeatThat.ShowHidePanels
{
	public class Show : BoolStateProperty {}

	public static class ShowExtenstions
	{
		public static void Show(this GameObject go, bool show = true)
		{
			go.SetActive(show);
			SetShow(go, show);
		}

		public static void Show(this IController controller, bool show = true) 
		{
			if(show) {
				controller.gameObject.SetActive(true);
				if(!controller.isBound) {
					controller.ResetBindGo();
				}

				SetShow(controller, true);
			}
			else {
				SetShow(controller, false);
			}
		}

		public static void ShowWith<T>(this IController<T> controller, T model) where T : class
		{
			controller.gameObject.SetActive(true);
			if(!controller.isBound || !object.ReferenceEquals(controller.model, model)) {
				controller.GoWith(model);
			}
			SetShow(controller, true);
		}

		private static void SetShow(IController c, bool show)
		{
			SetShow(c.gameObject, show);
		}

		private static void SetShow(GameObject go, bool show)
		{
			var prop = go.GetComponent<Show>();
			if(prop != null) {
				prop.value = show;
				return;
			}

			// this is a ShowWith extension functions are convenience and should work with non-animator based objects
			if(go.GetComponent<Animator>() == null) {
				return;
			}
			go.AddIfMissing<Show>().value = show;
		}
	}
}



