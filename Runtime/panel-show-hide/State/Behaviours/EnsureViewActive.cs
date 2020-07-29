using BeatThat.Controllers;
using BeatThat.Properties;
using UnityEngine;
using BeatThat.StateControllers;

namespace BeatThat.ShowHidePanels
{
	public class EnsureViewActive : AnimatorControllerBehaviour<ViewController>
	{
		public string m_viewActiveProp;

		override protected void DidEnter()
		{
			var view = this.controller.GetViewGameObject(true);
			view.SetActive(true);

			SetBool<DidActivateView>(true);
		}
	}
}

