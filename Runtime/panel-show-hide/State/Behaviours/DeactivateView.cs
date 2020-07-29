using BeatThat.Controllers;
using BeatThat.StateControllers;

namespace BeatThat.ShowHidePanels
{
    public class DeactivateView : AnimatorControllerBehaviour<ViewController>
	{
		override protected void DidEnter()
		{
			var view = this.controller.GetViewGameObject(false);
			if(view == null) {
				return;
			}
			view.SetActive(false);
		}
	}
}

