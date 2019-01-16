// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Meta/PointCloud" {
Properties{
        point_size("Point Size", Float) = 5.0
}
  SubShader {
     Pass {
		Cull Off
		ZWrite On
		ZTest Always

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
       
        #include "UnityCG.cginc"

        struct appdata
        {
           float4 vertex : POSITION;
        };

        struct v2f
        {
           float4 vertex : SV_POSITION;
           float4 color : COLOR;
        };
       
        float4x4 depthCameraTUnityWorld;
        float point_size;
       
        v2f vert (appdata v)
        {
           v2f o;
           o.vertex = UnityObjectToClipPos(v.vertex);
           o.color = float4(1.0f, 1.0f, 1.0f, 0.0f);
           return o;
        }
       
        fixed4 frag (v2f i) : SV_Target
        {
           return i.color;
        }
        ENDCG
     }
  }
}

