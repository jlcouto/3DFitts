Shader "HandCursor/HandCursorWriteStencil" {
	Properties
	{
		[PerRendererData] _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_CutOff("Cut Threshold", Range(0,1)) = 0.2
	}
	SubShader
	{
		Pass
		{
			Stencil
			{
				Ref 2
				Comp Always
				Pass Replace
				ZFail Keep
			}

			ZWrite On
			ZTest  Always

			Blend SrcAlpha OneMinusSrcAlpha
			Fog{ Mode Off }
			CGPROGRAM
			#pragma exclude_renderers gles flash
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct appdata {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float4 screen_uv : TEXCOORD1;
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				o.screen_uv = ComputeScreenPos(o.pos);
				return o;
			}

			sampler2D _MainTex;
			float _CutOff;


			fixed4 frag (v2f i) : SV_Target {
				fixed4 c = tex2D(_MainTex, i.uv);
				clip(c.a - _CutOff);
				return c;
			}

			ENDCG
		}
	}
}