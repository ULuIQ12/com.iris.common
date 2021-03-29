using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using com.iris.common;

namespace com.iris.common
{

	[CustomEditor(typeof(ExpSettings))]
	public class ExpSettingsEditor : Editor
	{
		SerializedProperty UID;
		SerializedProperty Title;
		SerializedProperty Author;
		SerializedProperty CreationDate;
		SerializedProperty LastUpdateDate;
		//SerializedProperty MotricityLevel;
		SerializedProperty AmplitudeLevel;
		SerializedProperty Instructions;
		SerializedProperty ThumbnailTexture;

		string tempInstructions;
		static CultureInfo culture = new CultureInfo("fr-fr");

		void OnEnable()
		{
			UID = serializedObject.FindProperty("uid");
			Title = serializedObject.FindProperty("Title");
			Author = serializedObject.FindProperty("Author");
			CreationDate = serializedObject.FindProperty("CreationDate");
			LastUpdateDate = serializedObject.FindProperty("LastUpdateDate");
			//MotricityLevel = serializedObject.FindProperty("MotricityLevel");
			AmplitudeLevel = serializedObject.FindProperty("AmplitudeLevel");
			Instructions = serializedObject.FindProperty("Instructions");
			ThumbnailTexture = serializedObject.FindProperty("ThumbnailTexture");


		}



		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.LabelField("General:");
			EditorGUILayout.PropertyField(UID);
			EditorGUILayout.PropertyField(Title);
			EditorGUILayout.PropertyField(Author);
			EditorGUILayout.PropertyField(ThumbnailTexture);
			
			EditorGUILayout.Space();

			var tempDate = DateTime.FromBinary(CreationDate.longValue);
			EditorGUILayout.LabelField("Creation Date:", tempDate.ToString(culture));
			tempDate = DateTime.FromBinary(LastUpdateDate.longValue);
			EditorGUILayout.LabelField("Last Update:", tempDate.ToString(culture));
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Instructions:");
			var style = new GUIStyle(EditorStyles.textArea);
			style.wordWrap = true;
			Instructions.stringValue = EditorGUILayout.TextArea(Instructions.stringValue, style, GUILayout.Height(60));
			EditorGUILayout.Separator();

			EditorGUILayout.PropertyField(AmplitudeLevel);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
