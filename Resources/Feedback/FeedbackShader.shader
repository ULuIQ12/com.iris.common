Shader "Unlit/Feedback"
{
	Properties 
	{
	    _MainTex ("Base (RGB)", 2D) = "white" {}

	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass
		{
            Name "Feedback"
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			
            
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

			TEXTURE2D(_CameraColorTexture);
			SAMPLER(sampler_CameraColorTexture);
            

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

			float4 _Tint; // R, G, B, Hue shift
			float4 _Xform;
            
            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv        : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            float SampleDepth(float2 uv)
            {
#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
                return SAMPLE_TEXTURE2D_ARRAY(_CameraDepthTexture, sampler_CameraDepthTexture, uv, unity_StereoEyeIndex).r;
#else
                return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
#endif
            }
			float3 SampleColor(float2 uv)
			{
#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
				return SAMPLE_TEXTURE2D_ARRAY(_CameraColorTexture, sampler_CameraColorTexture, uv, unity_StereoEyeIndex).rgb;
#else
				return SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, uv);
#endif
			}

			float3x3 ConstructTransformMatrix()
			{
				return float3x3(_Xform.y, -_Xform.x, _Xform.z,
					_Xform.x, _Xform.y, _Xform.w,
					0, 0, 1);
			}

			SamplerState default_trilinear_clamp_sampler;
			float3 SampleFeedbackTexture(float2 uv)
			{
				return SAMPLE_TEXTURE2D(_MainTex, default_trilinear_clamp_sampler, uv).rgb;
			}

			float3 ApplyTint(float3 rgb)
			{
				rgb = RgbToHsv(rgb);
				rgb.x = frac(rgb.x + _Tint.a);
				rgb = HsvToRgb(rgb);
				rgb *= _Tint.rgb;
				return rgb;
			}
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                output.uv = input.uv;
                
                return output;
            }
            
            float4 frag (Varyings input) : SV_Target 
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				
				float2 uv = input.uv;
				uv = mul(ConstructTransformMatrix(), float3(uv - 0.5, 1)).xy + 0.5;
				
				float3 c1 = SampleColor(input.uv);
				float3 c2 = ApplyTint( SampleFeedbackTexture(uv) );
				
				float3 cdepth = (SampleDepth(input.uv) == 0 )? c2 : c1;
				return float4(cdepth, 1);

            }
            
			#pragma vertex vert
			#pragma fragment frag
			
			ENDHLSL
		}
	} 
	FallBack "Diffuse"
}
