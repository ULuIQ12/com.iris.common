//#ifndef MYHLSLINCLUDE_INCLUDED
//#define MYHLSLINCLUDE_INCLUDED


void Dilate_float(Texture2D basemap, Texture2D dkernel, SamplerState ss, float2 uv, float radius, out float4 output)
{
	float2 size = 0.0;
	size.x = 683.0;
	size.y = 512.0;
	
	float2 invSize = 1.0 / size;
	
	float invKR = 1.0 / float(radius); 
	float acc = 0.0;
	float w = 0.0;


	for (int i = -radius; i <= radius; ++i) 
	{
		for (int j = -radius; j <= radius; ++j)
		{
			float2 rxy = float2(i, j);// vec2(ivec2(i, j));
			float2 kuv = rxy * invKR;
			float2 texOffset = uv + rxy * invSize;
			float kernel = dkernel.Sample(ss, float2(0.5, 0.5) + kuv).x;
			float tex = basemap.Sample(ss, texOffset).x;
			/*
			float kernel = texture(iChannel1, float2(0.5f) + kuv).x;
			float tex = texture(iChannel0, texOffset).x;
			*/
			
			float v = tex + kernel;
			if (v > acc) {
				acc = v;
				w = kernel;
			}
			
		}
	}
	
	//return acc - w;


	output = acc - w;
}