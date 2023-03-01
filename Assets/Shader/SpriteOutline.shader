Shader "MyShaders/SpriteOutline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _lineWidth("LineWidth", Range(0, 10)) = 1
        _lineColor("LineColor", Color) = (1, 0, 0, 1)
        _alphaThreshold("AlphaThreshold", Range(0, 1)) = .8
        _alpha("Alpha", Range(0, 1)) = 1
        _bodyColor("BodyColor", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags{
            "Queue" = "Transparent"
        }
        // No culling or depth
        Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _lineWidth;
            float4 _lineColor;
            float _alphaThreshold;
            float2 _MainTex_TexelSize;
            float _alpha;
            float4 _bodyColor;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
            float2 up = i.uv + float2(0, 1) * _lineWidth *_MainTex_TexelSize.xy;
            float2 down = i.uv + float2(0, -1) * _lineWidth *_MainTex_TexelSize.xy;
            float2 left = i.uv + float2(-1, 0) * _lineWidth *_MainTex_TexelSize.xy;
            float2 right = i.uv + float2(1, 0) * _lineWidth *_MainTex_TexelSize.xy;

            float w = tex2D(_MainTex, up).a + tex2D(_MainTex, down).a + tex2D(_MainTex, left).a + tex2D(_MainTex, right).a;

            //如果一个点自己是透明的而且周围有不透明的点，那么就是边缘;
            if (col.a < _alphaThreshold && w > (1 - _alphaThreshold)) {
                col.rgba = _lineColor;
                col.a = _alpha;
            }
            else
                col.rgba = col.rgba * _bodyColor;
                return col;
            }
            ENDCG
        }
    }
}
