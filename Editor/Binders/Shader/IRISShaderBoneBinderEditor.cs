using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace com.iris.common
{
	[CustomEditor(typeof(IRISShaderBoneBinder))]
	public class IRISShaderBoneBinderEditor : Editor
	{
		SerializedProperty Bones;

		void OnEnable()
		{
			Bones = serializedObject.FindProperty("Bones");
		}


		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(Bones);
			serializedObject.ApplyModifiedProperties();
		}
	}
	
}
