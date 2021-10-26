using UnityEngine;

namespace com.iris.common
{

	public class FeedbackEffectController : MonoBehaviour
	{
		#region Editable attributes

		[SerializeField, ColorUsage(false)] Color _tint = Color.white;
		[SerializeField] float _hueShift = 0;
		[SerializeField] bool BindHueShift = false;
		[SerializeField] FXDataProvider.FLOAT_DATA_TYPE HueShiftData;
		[SerializeField] Vector2 HueShiftRemap = new Vector4(0f, 1f, 0f, 0.1f);
		[SerializeField] float _offsetX = 0;
		[SerializeField] bool BindOffsetX = false;
		[SerializeField] FXDataProvider.FLOAT_DATA_TYPE OffsetXData;
		[SerializeField] Vector2 OffsetXRemap = new Vector4(0f, 1f, -0.1f, 0.1f);
		[SerializeField] float _offsetY = 0;
		[SerializeField] bool BindOffsetY = false;
		[SerializeField] FXDataProvider.FLOAT_DATA_TYPE OffsetYData;
		[SerializeField] Vector2 OffsetYRemap = new Vector4(0f, 1f, -0.1f, 0.1f);
		[SerializeField] float _rotation = 0;
		[SerializeField] bool BindRotation = false;
		[SerializeField] FXDataProvider.FLOAT_DATA_TYPE RotationData;
		[SerializeField] Vector2 RotationRemap = new Vector4(0f, 1f, -1f, 1f);
		[SerializeField] float _scale = 1;
		[SerializeField] bool BindScale = false;
		[SerializeField] FXDataProvider.FLOAT_DATA_TYPE ScaleData;
		[SerializeField] Vector2 ScaleRemap = new Vector4(0f, 1f, 0.9f, 1.1f);

		private Vector2 HueShiftRange = new Vector2(0f, 0.1f);
		private Vector2 OffsetXRange = new Vector2(-0.1f, 0.1f);
		private Vector2 OffsetYRange = new Vector2(-0.1f, 0.1f);
		private Vector2 RotationRange = new Vector2(-1f, 1f);
		private Vector2 ScaleRange = new Vector2(0.9f, 1.1f);

		#endregion

		#region Public properties

		public Color tint
		{ get => _tint; set => _tint = value; }

		public float hueShift
		{ get => _hueShift; set => _hueShift = value; }

		public float offsetX
		{ get => _offsetX; set => _offsetX = value; }

		public float offsetY
		{ get => _offsetY; set => _offsetY = value; }

		public float rotation
		{ get => _rotation; set => _rotation = value; }

		public float scale
		{ get => _scale; set => _scale = value; }


		#endregion

		#region Private members

		static int TintID = Shader.PropertyToID("_Tint");
		static int XformID = Shader.PropertyToID("_Xform");

		MaterialPropertyBlock _props;

		public Color GetTintColor()
		{
			if (BindHueShift)
				_hueShift = Remap(HueShiftRemap.x, HueShiftRemap.y, HueShiftRange.x, HueShiftRange.y, FXDataProvider.GetFloat(HueShiftData));

			return new Color(_tint.r, _tint.g, _tint.b, _hueShift);
		}

		public Vector4 GetTransformVector()
		{
			if( BindOffsetX)
				_offsetX = Remap(OffsetXRemap.x, OffsetXRemap.y, OffsetXRange.x, OffsetXRange.y, FXDataProvider.GetFloat(OffsetXData));

			if (BindOffsetY)
				_offsetY = Remap(OffsetYRemap.x, OffsetYRemap.y, OffsetYRange.x, OffsetYRange.y, FXDataProvider.GetFloat(OffsetYData));

			if( BindRotation)
				_rotation = Remap(RotationRemap.x, RotationRemap.y, RotationRange.x, RotationRange.y, FXDataProvider.GetFloat(RotationData));

			if (BindScale)
				_scale = Remap(ScaleRemap.x, ScaleRemap.y, ScaleRange.x, ScaleRange.y, FXDataProvider.GetFloat(ScaleData));

			var angle = Mathf.Deg2Rad * -_rotation;
			return new Vector4
			  (Mathf.Sin(angle), Mathf.Cos(angle), -offsetX, -_offsetY) / _scale;
		}

		#endregion

		private float Remap(float InputLow, float InputHigh, float OutputLow, float OutputHigh, float value)
		{
			value = Mathf.InverseLerp(InputLow, InputHigh, value);
			value = Mathf.Lerp(OutputLow, OutputHigh, value);

			return value;
		}

		internal MaterialPropertyBlock PropertyBlock => UpdatePropertyBlock();

		MaterialPropertyBlock UpdatePropertyBlock()
		{
			
			if (_props == null) _props = new MaterialPropertyBlock();
			_props.SetColor(TintID, GetTintColor());
			_props.SetVector(XformID, GetTransformVector());
			

			return _props;
		}

	}

}
