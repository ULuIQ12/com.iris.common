using UnityEditor;
using UnityEngine;

namespace com.iris.common
{

	[CanEditMultipleObjects, CustomEditor(typeof(FeedbackEffectController))]
	sealed class FeedbackEffectControllerEditor : Editor
	{
		
		SerializedProperty _tint;
		SerializedProperty _hueShift;
		SerializedProperty BindHueShift;
		SerializedProperty HueShiftData;
		SerializedProperty HueShiftRemap;

		SerializedProperty _offsetX;
		SerializedProperty BindOffsetX;
		SerializedProperty OffsetXData;
		SerializedProperty OffsetXRemap;

		SerializedProperty _offsetY;
		SerializedProperty BindOffsetY;
		SerializedProperty OffsetYData;
		SerializedProperty OffsetYRemap;

		SerializedProperty _rotation;
		SerializedProperty BindRotation;
		SerializedProperty RotationData;
		SerializedProperty RotationRemap;

		SerializedProperty _scale;
		SerializedProperty BindScale;
		SerializedProperty ScaleData;
		SerializedProperty ScaleRemap;

		void OnEnable()
		{
			
			_tint = serializedObject.FindProperty("_tint");
			
			_hueShift = serializedObject.FindProperty("_hueShift");
			BindHueShift = serializedObject.FindProperty("BindHueShift");
			HueShiftData = serializedObject.FindProperty("HueShiftData");
			HueShiftRemap = serializedObject.FindProperty("HueShiftRemap");

			_offsetX = serializedObject.FindProperty("_offsetX");
			BindOffsetX = serializedObject.FindProperty("BindOffsetX");
			OffsetXData = serializedObject.FindProperty("OffsetXData");
			OffsetXRemap = serializedObject.FindProperty("OffsetXRemap");

			_offsetY = serializedObject.FindProperty("_offsetY");
			BindOffsetY = serializedObject.FindProperty("BindOffsetY");
			OffsetYData = serializedObject.FindProperty("OffsetYData");
			OffsetYRemap = serializedObject.FindProperty("OffsetYRemap");

			_rotation = serializedObject.FindProperty("_rotation");
			BindRotation = serializedObject.FindProperty("BindRotation");
			RotationData = serializedObject.FindProperty("RotationData");
			RotationRemap = serializedObject.FindProperty("RotationRemap");

			_scale = serializedObject.FindProperty("_scale");
			BindScale = serializedObject.FindProperty("BindScale");
			ScaleData = serializedObject.FindProperty("ScaleData");
			ScaleRemap = serializedObject.FindProperty("ScaleRemap");



		}

		private Vector2 hueval = new Vector2(0f, 1f);
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			EditorGUILayout.PropertyField(_tint);
			////////////////////////////////////////////////////////////////////////////////////////////////////
			EditorGUILayout.Separator();
			EditorGUI.BeginDisabledGroup(BindHueShift.boolValue);
			EditorGUILayout.Slider(_hueShift, 0f, 0.1f);
			EditorGUI.EndDisabledGroup();

			BindHueShift.boolValue = EditorGUILayout.ToggleLeft("Bind hue shift", BindHueShift.boolValue);

			if (BindHueShift.boolValue)
			{
				EditorGUILayout.PropertyField(HueShiftData);
				HueShiftRemap.vector2Value = EditorGUILayout.Vector2Field("In min/max", HueShiftRemap.vector2Value);
			}
			////////////////////////////////////////////////////////////////////////////////////////////////////
			EditorGUILayout.Separator();
			EditorGUI.BeginDisabledGroup(BindOffsetX.boolValue);
			EditorGUILayout.Slider(_offsetX, -0.1f, 0.1f);
			EditorGUI.EndDisabledGroup();

			BindOffsetX.boolValue = EditorGUILayout.ToggleLeft("Bind OffsetX", BindOffsetX.boolValue);

			if (BindOffsetX.boolValue)
			{
				EditorGUILayout.PropertyField(OffsetXData);
				OffsetXRemap.vector2Value = EditorGUILayout.Vector2Field("In min/max", OffsetXRemap.vector2Value);
			}
			////////////////////////////////////////////////////////////////////////////////////////////////////
			EditorGUILayout.Separator();
			EditorGUI.BeginDisabledGroup(BindOffsetY.boolValue);
			EditorGUILayout.Slider(_offsetY, -0.1f, 0.1f);
			EditorGUI.EndDisabledGroup();

			BindOffsetY.boolValue = EditorGUILayout.ToggleLeft("Bind OffsetY", BindOffsetY.boolValue);

			if (BindOffsetY.boolValue)
			{
				EditorGUILayout.PropertyField(OffsetYData);
				OffsetYRemap.vector2Value = EditorGUILayout.Vector2Field("In min/max", OffsetYRemap.vector2Value);
			}
			////////////////////////////////////////////////////////////////////////////////////////////////////
			EditorGUILayout.Separator();
			EditorGUI.BeginDisabledGroup(BindRotation.boolValue);
			EditorGUILayout.Slider(_rotation, -1f, 1f);
			EditorGUI.EndDisabledGroup();

			BindRotation.boolValue = EditorGUILayout.ToggleLeft("Bind Rotation", BindRotation.boolValue);

			if (BindRotation.boolValue)
			{
				EditorGUILayout.PropertyField(RotationData);
				RotationRemap.vector2Value = EditorGUILayout.Vector2Field("In min/max", RotationRemap.vector2Value);
			}
			////////////////////////////////////////////////////////////////////////////////////////////////////
			EditorGUILayout.Separator();
			EditorGUI.BeginDisabledGroup(BindScale.boolValue);
			EditorGUILayout.Slider(_scale, 0.9f, 1.1f);
			EditorGUI.EndDisabledGroup();

			BindScale.boolValue = EditorGUILayout.ToggleLeft("Bind Scale", BindScale.boolValue);

			if (BindScale.boolValue)
			{
				EditorGUILayout.PropertyField(ScaleData);
				ScaleRemap.vector2Value = EditorGUILayout.Vector2Field("In min/max", ScaleRemap.vector2Value);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
} 

