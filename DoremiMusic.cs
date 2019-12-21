using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Doremi Multichannel Loop", menuName = "Doremi Audio Engine/Multichannel Loop")]
public class DoremiMusic : ScriptableObject {
	public List<AudioClip> audio;
	public int loopEndSamples;
	public int loopStartSamples;
}


// [CustomEditor(typeof(DoremiMusic))]
// public class DoremiMusicEditor : Editor {
// 	SerializedProperty audio;
// 	SerializedProperty loopEndSamples;
// 	SerializedProperty loopStartSamples;

// 	void OnEnable() {
// 		audio = serializedObject.FindProperty("audio");
// 		loopEndSamples = serializedObject.FindProperty("loopEndSamples");
// 		loopStartSamples = serializedObject.FindProperty("loopStartSamples");
// 	}

// 	public override void OnInspectorGUI() {
// 		serializedObject.Update();
// 		EditorGUILayout.PropertyField(audio);

// 		serializedObject.ApplyModifiedProperties();
// 	}
// }