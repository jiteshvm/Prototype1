Shader "ProtoShape2D/TextureAndColors"{
    Properties{
    	_Texture("Texture",2D)="white"{}
    	_Color1("Color one",Color)=(1,0.5,0.5,1)
    	_Color2("Color two",Color)=(0.5,0.5,1,1)
    	_GradientScale("Gradient scale",Range(0.01,10.0))=0
    	_GradientAngle("Gradient rotation",Range(-180.0,180.0))=0
    	_GradientOffset("Gradient offest",Range(-1.0,1.0))=0
    	//For positioning a gradient
    	_MinWPos("Min world position",Vector)=(0,0,0,0)
    	_MaxWPos("Max world position",Vector)=(0,0,0,0)
    }
    SubShader{
        Tags{
        	"Queue"="Transparent" 
        	"IgnoreProjector"="True"
        	"RenderType"="Transparent" 
        	"PreviewType"="Plane"
        	"ForceNoShadowCasting"="True"
        	"DisableBatching"="False"
        }
        Cull Off 
        Lighting Off 
        ZWrite Off 
        Fog {Mode Off} 
        Blend One OneMinusSrcAlpha
        Pass{
        	Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _Texture;
            fixed4 _Texture_ST;
            fixed4 _Color1,_Color2;
            fixed4 _MinWPos,_MaxWPos;
            float _GradientScale,_GradientAngle,_GradientOffset;

            struct vertex_in{
            	float4 col:COLOR;
                float4 pos:POSITION;
            };

            struct fragment_in{
            	float4 col:COLOR0;
                float4 pos:SV_POSITION;
                float3 wpos:COLOR1;
            };

            fragment_in vert(vertex_in v){
                fragment_in f;
                f.col=v.col;
              	f.pos=UnityObjectToClipPos(v.pos);
              	f.wpos=mul(unity_ObjectToWorld,v.pos).xyz;
                return f;
            }

            half2 Rotate2D(half2 _in,half _angle){
			    half s,c;
			    sincos(_angle,s,c);
			    float2x2 rot={c,s,-s,c};
			    return mul(rot,_in);
			}

            fixed4 frag(fragment_in f):SV_Target{
            	fixed4 result=float4(1,1,1,1);
            	//Center of the object in world position
            	fixed2 wcenter=(f.wpos-(_MinWPos+((_MaxWPos-_MinWPos)/2)));
            	//Add texture, multiplied by texture scale
            	result*=tex2D(_Texture,mul(unity_WorldToObject,wcenter*(_Texture_ST*0.2)));
				//If colors are the same, don't calculate the gradient
            	if(all(_Color1==_Color2)){
            		result*=_Color1;
            	}else{
	            	//Calculate UV-free fragment position
					half2 gpos=((f.wpos-_MinWPos)/(_MaxWPos-_MinWPos))-0.5+_GradientOffset;
					//Rotate
					gpos=Rotate2D(gpos,radians(_GradientAngle));
					//Calculate color
            		result*=_Color2*clamp(1-(gpos.y*_GradientScale+0.5),0,1)+_Color1*clamp(gpos.y*_GradientScale+0.5,0,1);
            	}
            	//Use vertex alpha if it's smaller. For "anti-aliasing"
            	result.a=min(result.a,f.col.a);
                return result;
            }

            ENDCG
        }
    }
}