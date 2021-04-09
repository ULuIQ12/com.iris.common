using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace com.iris.common
{
	[CustomEditor(typeof(IRISShaderTextureBinder))]
	public class IRISShaderTextureBinderEditor : Editor
	{
		SerializedProperty TexturePropertyName;
		SerializedProperty TextureToBind;
		SerializedProperty BindScale;
		SerializedProperty TextureScalePropertyName;		
		SerializedProperty BindSizes;
		SerializedProperty TextureSizePropertyName;
		SerializedProperty ShowWarning;

		private GUIStyle WarningStyle;
		
		void OnEnable()
		{
			WarningStyle = new GUIStyle(EditorStyles.label);
			WarningStyle.normal.textColor = Color.red;

			TexturePropertyName = serializedObject.FindProperty("TexturePropertyName");
			TextureToBind = serializedObject.FindProperty("TextureToBind");

			BindScale = serializedObject.FindProperty("BindScale");
			TextureScalePropertyName = serializedObject.FindProperty("TextureScalePropertyName");

			BindSizes = serializedObject.FindProperty("BindSize");
			TextureSizePropertyName = serializedObject.FindProperty("TextureSizePropertyName");
			ShowWarning = serializedObject.FindProperty("ShowWarning");
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(TexturePropertyName);
			EditorGUILayout.PropertyField(TextureToBind);

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(BindScale);
			EditorGUILayout.PropertyField(TextureScalePropertyName);

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(BindSizes);	

			if(BindSizes.boolValue)
			{
				EditorGUILayout.PropertyField(TextureSizePropertyName);
			}

			if(ShowWarning.boolValue)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Warning : bindings incorrect, check your values" , WarningStyle);
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}
