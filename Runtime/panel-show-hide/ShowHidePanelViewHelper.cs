using BeatThat.GetComponentsExt;
using BeatThat.CollectionsExt;
using BeatThat.Properties;
using UnityEngine;
using BeatThat.StateControllers;

#if UNITY_EDITOR
using BeatThat.StateControllers.ParamsEditorExtensions;
using BeatThat.AnimatorTemplates;
#endif

namespace BeatThat.ShowHidePanels
{

    /// <summary>
    /// For an (independent/child) View component to function in a typical ShowHidePanel set up,
    /// it needs to have a largish set of sibling components
    /// (an Animator, SyncState[From|to]Parent, Showing, Hidden, etc.)
    /// and even beyong that, the AnimatorController (asset) on the Animator usually
    /// has a specific set of animator parameters that control the show-hide functionality.
    /// 
    /// ShowHidePanelViewHelper is a utility component that doesn't have any of the above behaviour
    /// but instead requires all the various components needed and also helps setup the AnimatorController.
    /// 
    /// By default, ShowHidePanelViewHelper hides most of its managed components in Unity inspector,
    /// but this hiding can be toggled off as well.
    /// </summary>
    [ExecuteInEditMode]
	public class ShowHidePanelViewHelper : MonoBehaviour
	{
		// why not use bit flags for this? because small number of combos and we've have to customize the editor to get it do show as flags
		public enum SyncStateWithParentConfig { EnableToAndFrom = 0, EnableFromDisableTo = 1, EnableToDisableFrom = 2, DisableToAndFrom = 3 }

		[Tooltip(@"set TRUE to see managed components in inspector. 
		These include an AnimatorControler and properties Hidden, Showing, as well as optional SyncStateFromParent and SyncStateToParent")]
		public bool m_unhideManagedComponents;

		[Tooltip("By default, ShowHidePanel View will have an animator that syncs params to and from an animator on the parent Controller. Use these options to disable either 'to' or 'from' or both.")]
		public SyncStateWithParentConfig m_syncStateWithParentConfig;

		[Tooltip("it's common to have a CanvasGroup at the root of a ShowHidePanel view and to have it hidden when the view is activated (so it can then fade in)")]
		public bool m_onEnableSetCanvasGroupAlphaTo0 = true;

		void OnEnable()
		{
			if (!Application.isPlaying) {
				return;
			}

			if (!m_onEnableSetCanvasGroupAlphaTo0) {
				return;
			}

			var cg = GetComponent<CanvasGroup> ();
			if (cg == null) {
				return;
			}

			cg.alpha = 0f;
		}

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
			ValidateAnimatorParameters (createMissing: true);
			UpdateManagedComponentVisibility (createMissing: true);
		}

		public void DestroyManagedComponents()
		{
			this.gameObject.DestroyIfPresent<Showing> ();
			this.gameObject.DestroyIfPresent<Hidden> ();
			this.gameObject.DestroyIfPresent<SyncStateToParent> ();
			this.gameObject.DestroyIfPresent<SyncStateFromParent> ();
			this.gameObject.DestroyIfPresent<AnimatorController> ();
		}

		public void UpdateManagedComponentVisibility(bool createMissing = true)
		{
			var hf = (m_unhideManagedComponents) ? HideFlags.None : HideFlags.HideInInspector;

			SetHideFlags<AnimatorController> (hf, createMissing);
			SetHideFlags<Hidden> (hf, createMissing);
			SetHideFlags<Showing> (hf, createMissing);

			SetHideFlagsAndEnabled<SyncStateFromParent> (hf, 
				(m_syncStateWithParentConfig == SyncStateWithParentConfig.EnableToAndFrom || m_syncStateWithParentConfig == SyncStateWithParentConfig.EnableFromDisableTo),
				createMissing
			);

			SetHideFlagsAndEnabled<SyncStateToParent> (hf, 
				(m_syncStateWithParentConfig == SyncStateWithParentConfig.EnableToAndFrom || m_syncStateWithParentConfig == SyncStateWithParentConfig.EnableToDisableFrom),
				createMissing
			);
		}

		private void SetHideFlagsAndEnabled<T>(HideFlags hf, bool enabled, bool createMissing = true)
			where T : MonoBehaviour
		{
			this.EditComponent<T> (c => { c.hideFlags = hf; c.enabled = enabled; }, createMissing);
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
				c = AnimatorTemplate.CreateAnimatorController ("ShowHidePanelViewTemplate", this.name);
				if (c != null) {
					UnityEditor.Animations.AnimatorController.SetAnimatorController (a, c);
				}
				return c != null;
			}

			return true;

		}

		public bool ValidateParam<T>(bool createMissing = false) where T : Component, Param
		{
			var p = GetComponent<T> ();
			if(p == null && (!createMissing || (p = this.AddIfMissing<T> ()) == null)) {
				return false;
			}

			UnityEditor.Animations.AnimatorController c;
			if (!ParamEditorExt.GetAnimatorController (this, out c)) {
				return false;
			}

			return p.ValidateAnimatorControllerParam (createMissing);
		}

		public bool ValidateHiddenParam(bool createMissing = true)
		{
			return ValidateParam<Hidden> (createMissing);
		}

		public bool ValidateShowingParam(bool createMissing = true)
		{
			return ValidateParam<Showing> (createMissing);
		}

		public bool ValidateShowParam(bool createMissing = true)
		{
			UnityEditor.Animations.AnimatorController c;
			return ParamEditorExt.GetAnimatorController (this, out c)
			&& ParamEditorExt.ValidateAnimatorControllerParam (c, ParamExt.DefaultParamName<Show> (), typeof(bool), createMissing);
		}

		public bool ValidateImmediateParam(bool createMissing = true)
		{
			UnityEditor.Animations.AnimatorController c;
			return ParamEditorExt.GetAnimatorController (this, out c)
				&& ParamEditorExt.ValidateAnimatorControllerParam (c, ParamExt.DefaultParamNameRemovingSuffixes<ImmediateTrigger> ("Trigger"), typeof(Invocable), createMissing);
		}
			
		/// <summary>
		/// Check whether the setup for a ShowHidePanel view has all required components including parameters on AnimatorController asset.
		/// Optionally, tries to create missing parameters.
		/// </summary>
		public bool ValidateAnimatorParameters(bool createMissing = true)
		{
			UnityEditor.Animations.AnimatorController c;

			return ParamEditorExt.GetAnimatorController (this, out c)
				&& ValidateShowParam (createMissing)
				&& ValidateShowingParam (createMissing)
				&& ValidateHiddenParam (createMissing)
				&& ValidateImmediateParam (createMissing);
		}


		#endif



	}
}




