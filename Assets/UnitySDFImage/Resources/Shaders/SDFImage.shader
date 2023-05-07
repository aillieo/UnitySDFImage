Shader "AillieoUtils/SDFImage"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)
        _NumCircles("Number of Circles", Int) = 0
        _Softness("Softness", Range(0, 0.1)) = 0
        _BlendRadius("BlendRadius", Range(0.0001, 1.0)) = 0.1
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "CanUseSpriteAtlas" = "True"}

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "CGIncludes/SDF2D.cginc"

            #define SDFOperation_Union 0
            #define SDFOperation_Intersection 1
            #define SDFOperation_Subtraction 2

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

            sampler2D _MainTex; 
            float4 _MainTex_ST;
            float4 _Color;
            float4 _CircleDataArray[10];
            int _NumCircles;
            float _BlendRadius;
            float _Softness;

            float smoothmin(float a, float b, float k) {
                float h = max(k - abs(a - b), 0.0f) / k;
                return min(a, b) - h * h * k * 0.25f;
            }

            float smoothmax(float a, float b, float k) {
                float h = max(k - abs(a - b), 0.0f) / k;
                return max(a, b) + h * h * k * 0.25f;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float sdf = 0.0;
                int operation = SDFOperation_Union;

                for (int j = 0; j < _NumCircles; j++)
                {
                    // center 
                    float2 center = _CircleDataArray[j].xy;

                    // radius
                    float radius = _CircleDataArray[j].z;

                    // calculate sdf value
                    float dist = length(i.uv - center);
                    float circleSdf = radius - dist;

                    if (j == 0)
                    {
                        sdf = circleSdf;
                    }
                    else
                    {
                        operation = (int)_CircleDataArray[j].w;

                        switch (operation)
                        {
                        case SDFOperation_Union:
                            sdf = smoothmax(sdf, circleSdf, _BlendRadius);
                            break;
                        case SDFOperation_Intersection:
                            sdf = smoothmin(sdf, circleSdf, _BlendRadius);
                            break;
                        case SDFOperation_Subtraction:
                            sdf = smoothmin(sdf, -circleSdf, _BlendRadius);
                            break;
                        }
                    }
                }

                float halfSoft = _Softness * 0.5f;
                sdf = smoothstep(-halfSoft, halfSoft, sdf);

                float4 col = _Color * sdf;
                col *= tex2D(_MainTex, i.uv);

                return col;
            }

            ENDCG
        }
    }
}
