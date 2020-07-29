using BeatThat.SafeRefs;
using BeatThat.TransformPathExt;
using BeatThat.GetComponentsExt;
using BeatThat.CollectionsExt;
using BeatThat.Controllers;
using UnityEngine;
using System;
using UnityEngine.Events;
using BeatThat.Bindings;
using BeatThat.Transitions;
using BeatThat.Panels;
using BeatThat.StateControllers;

#if UNITY_EDITOR
using BeatThat.StateControllers.ParamsEditorExtensions;
using BeatThat.AnimatorTemplates;
#endif

namespace BeatThat.ShowHidePanels
{
    /// <summary>
    /// Core component in a (PanelController) panel implementation where these are the assumptions about the set up:
    /// 
    /// 1) an Animator is used to 'show' and 'hide' the panel
    /// 2) The Animator has the following properties:
    /// 	A) (bool) 'show' - when set to TRUE, the panel will transition to showing
    /// 	B) (bool) 'isShowing' - should be set by the view to TRUE when the panel is FULLY showing
    /// 	C) (bool) 'isHidden' - should be set by the view to TRUE when the panel is FULLY hidden
    /// 	D) (bool) 'didActivateView' - should be set by the controller when the view has been activated
    /// 
    /// @see package 'panel-interfaces' for the basics about Panel's and package 'panel-controllers' for the basics about PanelController's.
    /// </summary>
    public class ShowHidePanel : BindingBehaviour, Panel
	{
		[Tooltip(@"set TRUE to see managed components in inspector. 
		These include an AnimatorControler and properties Show, ImmediateTrigger, and DidActivateView")]
		public bool m_unhideManagedComponents;

		#if UNITY_EDITOR
		void Awake()
		{
			UpdateManagedComponentVisibility ();
		}

		void OnValidate()
		{
			UpdateManagedComponentVisibility ();
		}

		void Reset()
		{
			ValidateAnimator (createMissing: true);
		}

		public void UpdateManagedComponentVisibility(bool createMissing = true)
		{
			var hf = (m_unhideManagedComponents) ? HideFlags.None : HideFlags.HideInInspector;
			SetHideFlags<Show> (hf, createMissing);
			SetHideFlags<ImmediateTrigger> (hf, createMissing);
			SetHideFlags<DidActivateView> (hf, createMissing);
			SetHideFlags<AnimatorController> (hf, createMissing);
		}

		public void DestroyManagedComponents()
		{
			this.gameObject.DestroyIfPresent<Show> ();
			this.gameObject.DestroyIfPresent<ImmediateTrigger> ();
			this.gameObject.DestroyIfPresent<DidActivateView> ();
			this.gameObject.DestroyIfPresent<AnimatorController> ();
		}

		private void SetHideFlags<T>(HideFlags hf, bool createMissing = true)
			where T : Component
		{
			this.EditComponent<T> (c => c.hideFlags = hf, createMissing);
		}


		public bool ValidateAnimator(bool createMissing = false)
		{
			var a = GetComponent<Animator> ();
			if(a == null && !createMissing) { 
				return false;
			}

			if (a == null) {
				a = this.gameObject.AddComponent<Animator> ();
			}

			UnityEditor.Animations.AnimatorController c;
			if (!ParamEditorExt.GetAnimatorController (this, out c) && !createMissing) {
				return false;
			}

			if (c == null) {
				c = AnimatorTemplate.CreateAnimatorController ("ShowHidePanelControllerTemplate", this.name);
				if (c != null) {
					UnityEditor.Animations.AnimatorController.SetAnimatorController (a, c);
				}
				return c != null;
			}

			return true;

		}
		#endif

		public Transition PrepareTransition(PanelTransition t, OnTransitionFrameDelegate onFrameDel = null)
		{
			return PreparePanelTransition(t, onFrameDel);
		}

		public void StartTransition(PanelTransition t)
		{
			PreparePanelTransition(t, null).StartTransition();
		}

		public void BringInImmediate ()
		{
			if(this.activePanelTransition != null) {
				this.activePanelTransition.Cancel();
			}

			RebindParams();
			this.animator.Play("Showing");
			this.show.value = true;
		}

		public void DismissImmediate ()
		{
			if(this.activePanelTransition != null) {
				this.activePanelTransition.Cancel();
			}

			RebindParams();
			this.animator.Play("Hidden");
			this.show.value = false;
		}

		public Transition PreparePanelTransition(PanelTransition t, OnTransitionFrameDelegate onFrameDel)
		{
			if(t == PanelTransition.IN) {
				return this.showTransition;
			}

			if(t == PanelTransition.OUT) {
				return this.hideTransition;
			}

			throw new NotSupportedException("only supports PanelTransition.IN and PanelTransition.OUT recevieved " + t);
		}


		private void OnShowing(bool showing)
		{
			if(!showing) {
				return;
			}

			if(!this.showTransition.isTransitionRunning) {
//				Debug.LogWarning("[" + Time.frameCount + "][" + this.Path() + "] OnShowing updated but Show transition not running");
				return;
			}
			this.showTransition.Complete();
		}
		private UnityAction<bool> showingAction { get { return m_showingAction?? (m_showingAction = this.OnShowing); } }
		private UnityAction<bool> m_showingAction;

		private void OnHidden(bool hidden)
		{
			if(!hidden) {
				return;
			}

			if(!this.hideTransition.isTransitionRunning) {
//				Debug.LogWarning("[" + Time.frameCount + "][" + this.Path() + "] OnHidden updated but Hide transition not running");
				return;
			}
			this.hideTransition.Complete();
		}
		private UnityAction<bool> hiddenAction { get { return m_hiddenAction?? (m_hiddenAction = this.OnHidden); } }
		private UnityAction<bool> m_hiddenAction;

		override protected void OnDestroy()
		{
			if(this.activePanelTransition != null && this.activePanelTransition.isTransitionRunning) {
				this.activePanelTransition.Cancel();
			}
			base.OnDestroy();
		}

		private Transition showTransition { get { return m_showTransition?? (m_showTransition = new ShowTransition(this)); } }
		private Transition m_showTransition;

		private Transition hideTransition { get { return m_hideTransition?? (m_hideTransition = new HideTransition(this)); } }
		private Transition m_hideTransition;

		private Transition activePanelTransition { get; set; }

		private Animator animator { get { return m_animator?? (m_animator = GetComponent<Animator>()); } }
		private Animator m_animator;

		public Show show { get { return m_show?? (m_show = this.AddIfMissing<Show>()); } }
		private Show m_show;

		private void RebindParams()
		{
			Unbind();
			Bind(AddToViewIfMissing<Showing>().onValueChanged, this.showingAction);
			Bind(AddToViewIfMissing<Hidden>().onValueChanged, this.hiddenAction);
		}


		private Binding hiddenBinding { get; set; }
		private Binding showingBinding { get; set; }

		private T AddToViewIfMissing<T>() where T : Component
		{
			var hasView = GetComponent<HasView>();
			if(hasView != null) {
				var view = hasView.GetViewGameObject(true);
				return view.AddIfMissing<T>();
			}

			Debug.LogWarning("[" + Time.frameCount + "][" + this.Path() + "] no hasView component found. Adding Showing and Hidden params to this GameObject");
			return this.AddIfMissing<T>();
		}

		public ImmediateTrigger immediateTrigger { get { return m_immediateTrigger?? (m_immediateTrigger = this.AddIfMissing<ImmediateTrigger>()); } }
		private ImmediateTrigger m_immediateTrigger;

		private void OnStart(Transition t)
		{
			if(this.activePanelTransition != null && this.activePanelTransition != t) {
				this.activePanelTransition.Cancel();
			}

			this.activePanelTransition = t;
		}

		private void OnComplete(Transition t)
		{
			if(this.activePanelTransition != t) {
				Debug.LogWarning("[" + Time.frameCount + "] [" + this.Path() + "] Receieved transition complete from non-active transition!");
				return;
			}

			this.activePanelTransition = null;
		}

		private void OnCancel(Transition t)
		{
			if(this.activePanelTransition != t) {
				Debug.LogWarning("[" + Time.frameCount + "] [" + this.Path() + "] Receieved transition cancel from non-active transition!");
				return;
			}

			this.activePanelTransition = null;
		}

		class ShowTransition : TransitionBase
		{
			public ShowTransition(ShowHidePanel owner)
			{
				m_owner = new SafeRef<ShowHidePanel>(owner);
			}

			virtual protected bool setShowParamOnStart { get { return true; } }

			override protected void DoStartTransition(float time)
			{
				var o = this.owner;
				if(o == null) {
					Cancel();
					return;
				}
				this.owner.RebindParams();
				this.owner.show.value = this.setShowParamOnStart;
				this.owner.OnStart(this);
			}

			override protected void CompleteTransitionEarly()
			{
				var o = this.owner;
				if(o == null) {
					return;
				}

				o.immediateTrigger.Invoke();
			}

			override protected void DoCancelTransition()
			{
				if(this.owner == null) {
					return;
				}
				this.owner.show.value = false;
				this.owner.OnCancel(this);
			}

			override protected void AfterTransitionCompleteCallback()
			{
				if(this.owner == null) {
					return;
				}
				this.owner.OnComplete(this);
			}
				
			override public string ToString()
			{
				return "[" + GetType() + " owner=" + this.owner + " running=" + this.isTransitionRunning 
					+ ", complete=" + this.isTransitionComplete + "]";
			}

			protected ShowHidePanel owner { get { return m_owner.value; } }
			private SafeRef<ShowHidePanel> m_owner;
		}
			
		class HideTransition : ShowTransition
		{
			public HideTransition(ShowHidePanel owner) : base(owner) {}

			override protected bool setShowParamOnStart { get { return false; } }

			override protected void DoCancelTransition() 
			{
				this.owner.OnCancel(this);
			}
		}


	}
}






