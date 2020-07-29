using BeatThat.GetComponentsExt;
using BeatThat.CollectionsExt;
using BeatThat.Controllers;
using BeatThat.Properties;
using UnityEngine.Events;
using BeatThat.StateControllers;

namespace BeatThat.ShowHidePanels
{
    public class OnShowActivateView : AnimatorControllerBehaviour<ViewController>
	{
		public enum OnHiddenAction { None = 0, Deactivate = 1, DeactivateView = 2 }
		public OnHiddenAction m_onHiddenAction;

		private Show show { get { return m_show?? (m_show = this.animator.AddIfMissing<Show>()); } }
		private Show m_show;

		private DidActivateView didActivateView { get { return m_didActivateView?? (m_didActivateView = this.animator.AddIfMissing<DidActivateView>()); } }
		private DidActivateView m_didActivateView;

		override protected void BindControllerState()
		{
			Bind(this.show.onValueChanged, this.showChangedAction);
		}

		override protected void DidEnter()
		{
			if(this.show.value) {
				ShowView(true);
				return;
			}
				
			switch(m_onHiddenAction) {
			case OnHiddenAction.Deactivate:
				this.gameObject.SetActive(false);
				break;
			case OnHiddenAction.DeactivateView:
				var view = this.controller.GetViewGameObject(false);
				if(view != null && view.GetBool<Hidden>()) {
					ShowView(false);
				}
				break;
			}
		}

		private void ShowView(bool show)
		{
			var view = this.controller.GetViewGameObject();
			if(view.activeSelf != show) {
				view.SetActive(show);
			}
			this.didActivateView.value = show;
		}

		private void OnShowChanged(bool show)
		{
			if(!show) {
				return;
			}

			ShowView(true);
		}
		private UnityAction<bool> showChangedAction { get { return m_showChangedAction?? (m_showChangedAction = this.OnShowChanged); } }
		private UnityAction<bool> m_showChangedAction;
	

	}
}



