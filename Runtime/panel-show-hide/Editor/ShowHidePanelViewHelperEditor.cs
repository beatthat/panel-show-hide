using BeatThat.CollectionsExt;
using BeatThat.Controllers;
using BeatThat.Controllers.Panels;
using BeatThat.GetComponentsExt;
using UnityEditor;
using UnityEngine;

namespace BeatThat.ShowHidePanels
{
    [CustomEditor(typeof(ShowHidePanelViewHelper))]
	public class ShowHidePanelViewHelperEditor : UnityEditor.Editor 
	{

		private bool showRemoveOption { get; set; }

		override public void OnInspectorGUI()
		{
			var bkgColorSaved = GUI.backgroundColor;
			var fgColorSaved = GUI.color;

			var shp = this.target as ShowHidePanelViewHelper;

			EditorGUI.BeginChangeCheck ();
			base.OnInspectorGUI ();

			this.serializedObject.ApplyModifiedProperties ();

			if (EditorGUI.EndChangeCheck ()) {
				(target as ShowHidePanelViewHelper).UpdateManagedComponentVisibility ();
			}

			if (Application.isPlaying) {
				return;
			}

			if (!shp.ValidateAnimator (createMissing:false)) {
				GUI.backgroundColor = Color.yellow;
				if (GUILayout.Button ("Create Missing AnimatorController")) {
					shp.ValidateAnimator (createMissing:true);
				}
				GUI.backgroundColor = bkgColorSaved;
			}

			if (!shp.ValidateAnimator (createMissing: false)) {
				return;
			}


			if (!shp.ValidateShowParam (createMissing: false)) {
				GUI.backgroundColor = Color.yellow;
				if (GUILayout.Button ("Create Missing Required AnimatorController param 'Show'")) {
					shp.ValidateShowParam (createMissing: true);
				}
				GUI.backgroundColor = bkgColorSaved;
			}

			if (!shp.ValidateShowingParam (createMissing: false)) {
				GUI.backgroundColor = Color.yellow;
				if (GUILayout.Button ("Create Missing Required AnimatorController param 'Showing'")) {
					shp.ValidateShowingParam (createMissing: true);
				}
				GUI.backgroundColor = bkgColorSaved;
			}

			if (!shp.ValidateHiddenParam (createMissing: false)) {
				GUI.backgroundColor = Color.yellow;
				if (GUILayout.Button ("Create Missing Required AnimatorController param 'Hidden'")) {
					shp.ValidateHiddenParam (createMissing: true);
				}
				GUI.backgroundColor = bkgColorSaved;
			}

			if (!shp.ValidateImmediateParam (createMissing: false)) {
				var saveColor = GUI.backgroundColor;
				GUI.backgroundColor = Color.yellow;
				if (GUILayout.Button ("Create Missing Required AnimatorController param 'ImmediateTrigger'")) {
					shp.ValidateImmediateParam (createMissing: true);
				}
				GUI.backgroundColor = saveColor;
			}

			if (shp.GetComponent<IView> () == null) {
				EditorGUILayout.HelpBox ("Missing required View component", MessageType.Warning);
				GUI.backgroundColor = Color.yellow;
				if (GUILayout.Button ("Add a default PanelView")) {
					shp.AddIfMissing<PanelView> ();
				}
				if (GUILayout.Button ("Generate a custom View class")) {
					EditorUtility.DisplayDialog ("Generate a custom View class", "This would be a great feature to have!\nSomeone build it", "OK");
				}
				GUI.backgroundColor = bkgColorSaved;
			}

			GUI.color = Color.yellow;
			this.showRemoveOption = EditorGUILayout.Toggle(new GUIContent("Show Remove Option", TOOLTIP_MSG_FOR_REMOVE), this.showRemoveOption);
			GUI.color = fgColorSaved;

			if(this.showRemoveOption) {
				GUI.backgroundColor = Color.red;
				if (GUILayout.Button (new GUIContent("Remove ShowHidePanelView Setup", TOOLTIP_MSG_FOR_REMOVE))) {
					(this.target as ShowHidePanelViewHelper).DestroyManagedComponents ();
					DestroyImmediate(this.target);
				}
				GUI.backgroundColor = bkgColorSaved;
			}

		}

		private const string TOOLTIP_MSG_FOR_REMOVE = "ShowHidePanelViewHelper creates and manages a set of hidden components. If you want to remove the ShowHidePanelViewHelper set up, it's usually better to use the custom 'Remove' option to ensure all those hidden components get removed as well";
	}

}




