Shader "Custom/FullscreenQuad"
{
	Properties
	{
	}

	SubShader
	{
		Cull Off ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
				uint   id     : SV_VertexID;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				float4 verts[4] = {
					float4(1, 1, 0, 1),
					float4(-1, -1, 0, 1),
					float4(-1, 1, 0, 1),
					float4(1, -1, 0, 1),
				};
				o.vertex = verts[v.id];
				o.uv = 1 - v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 final_color;
				final_color = fixed4(i.uv, 0.0, 1.0);
				return final_color;
			}
			ENDCG
		}
	}
}