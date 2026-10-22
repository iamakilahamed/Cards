Shader "CAT/StencilMask"
{
    Properties
    {
        _MainTex ("Mask Texture", 2D) = "white" {}
        _StencilID ("Stencil ID", Int) = 1
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.1
    }
    SubShader
    {
        // Renders just before standard transparent objects
        Tags { "Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
        
        // Do not draw any visible pixels or write to the Z-buffer
        ColorMask 0
        ZWrite Off
        Cull Off

        Stencil
        {
            Ref [_StencilID]
            Comp Always
            Pass Replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Cutoff;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // Discard pixels that are transparent so they don't write to the stencil
                clip(col.a - _Cutoff);
                return col;
            }
            ENDCG
        }
    }
}