Shader "Kinect/ARKitDepthCamColorShader"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}
		_ColorTex ("_ColorTex", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
		
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;    
			float4 _MainTex_ST; 

			sampler2D _ColorTex;    
			float4 _ColorTex_ST; 

			float _FactorX;
			float _FactorY;

			float _TexMinX;
			float _TexMaxX;
			float _TexMinY;
			float _TexMaxY;

			struct appdata
			{
				float4 position : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.position); 
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
	            
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				if(i.uv.x >= _TexMinX && i.uv.x < _TexMaxX && i.uv.y >= _TexMinY && i.uv.y < _TexMaxY)
				{
					float2 uvColor = float2((i.uv.x - _TexMinX) * _FactorX, (i.uv.y - _TexMinY) * _FactorY);
					fixed4 texColor = tex2D(_ColorTex, uvColor);
					return texColor;
                }
				else
				{
					return fixed4(0.0, 0.0, 0.0, 0.0);  // invisible
                }
			}

			ENDCG
		}
	}
}
