// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CircleEffect" 
{
	Properties
	{
		_BackgroundColor ("Background Color", Color) = (0.0, 0.0, 0.0, 0.0)
		_Color ("Border Color", Color) = (1,1,1,1)
		_Thickness ("Border Thickness", Float) = 0.02
		_Radius ("Radius", Float) = 0.48
	}
	SubShader 
	{
		Tags {"Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Pass 
		{
			CGPROGRAM
			
			// use "vert" function as the vertex shader
			#pragma vertex vert

			// use "frag" function as the pixel (fragment) shader
			#pragma fragment frag

			uniform float _Thickness;
			uniform float _Radius; 
			uniform float4 _Color;
			uniform float4 _BackgroundColor;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
				uint   id     : SV_VertexID;
			};

			// vertex shader outputs ("vertex to fragment")
			struct v2f {
                float4 position : SV_POSITION; // clip space position
				float2 uv : TEXCOORD0;
			};

			

			// vertex shader
            v2f vert (appdata v)
            {
				v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
				o.uv= 1 - v.uv;
				return o;
            }

            // pixel shader
            // color ("SV_Target" semantic)
            fixed4 frag (v2f i) : SV_Target
            {
				float2 center = float2(0.5, 0.5);
				float2 st = i.uv;

				st -= center;

				float dist =  sqrt(dot(st, st));

				//float t = smoothstep(_Radius + _Thickness, _Thickness, dist);
				
				float t = 1.0 + smoothstep(_Radius, _Radius + _Thickness, dist) 
                - smoothstep(_Radius - _Thickness, _Radius, dist);

				fixed4 final_color = lerp(_Color, _BackgroundColor, t);

                return final_color;
            }
			ENDCG
		}
	}
}