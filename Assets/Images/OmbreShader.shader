Shader "Custom/OmbreShader"
{
    Properties
    {
        _CenterBottomColor ("Center Bottom Color", Color) = (1,1,1,1)
        _EdgeColor ("Edge Color", Color) = (1,1,1,0)
        _Alpha ("Alpha", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _CenterBottomColor;
            fixed4 _EdgeColor;
            float _Alpha;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float distanceFromBottomCenter = distance(i.texcoord, float2(0.5, 0.0));
                float gradientFactor = saturate(1.0 - distanceFromBottomCenter * 2.0);

                fixed4 color = lerp(_EdgeColor, _CenterBottomColor, gradientFactor);
                color.a *= _Alpha; // Apply the alpha transparency
                return color;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
