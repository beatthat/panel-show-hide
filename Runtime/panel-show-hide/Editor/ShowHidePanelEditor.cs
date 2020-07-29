using BeatThat.CollectionsExt;
using BeatThat.Controllers;
using BeatThat.Controllers.Panels;
using BeatThat.GetComponentsExt;
using UnityEditor;
using UnityEngine;

namespace BeatThat.ShowHidePanels
{
    [CustomEditor(typeof(ShowHidePanel))]
	public class ShowHidePanelEditor : UnityEditor.Editor 
	{
		private bool showRemoveOption { get; set; }

		override public void OnInspectorGUI()
		{
			var bkgColorSaved = GUI.backgroundColor;
			var fgColorSaved = GUI.color;

			var shp = this.target as ShowHidePanel;

			EditorGUI.BeginChangeCheck ();
			base.OnInspectorGUI ();
			if (EditorGUI.EndChangeCheck ()) {
				(target as ShowHidePanel).UpdateManagedComponentVisibility ();
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

			if (shp.GetComponent<IController> () == null) {
				EditorGUILayout.HelpBox ("Missing required Controller component", MessageType.Warning);
				GUI.backgroundColor = Color.yellow;
				if (GUILayout.Button ("Add a default PanelController")) {
					shp.AddIfMissing<PanelController> ();
				}
				if (GUILayout.Button ("Generate a custom Controller class")) {
					EditorUtility.DisplayDialog ("Generate a custom Controller class", "This would be a great feature to have!\nSomeone build it", "OK");
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

		private const string TOOLTIP_MSG_FOR_REMOVE = "ShowHidePanel creates and manages a set of hidden components. If you want to remove the ShowHidePanel set up, it's usually better to use the custom 'Remove' option to ensure all those hidden components get removed as well";

	}

}




