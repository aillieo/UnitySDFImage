Shader "AillieoUtils/SDFImage"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
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
            #define SDFOperation_ShapeBlending 3

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color: COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color: COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _SDFDataBuffer[16];
            int _SDFDataLength;
            float _BlendRadius;
            float _Softness;

            float readFromBuffer(uint index)
            {
                uint indexInArray = index / 4;
                uint indexInVector = index % 4;
                return _SDFDataBuffer[indexInArray][indexInVector];
            }

            float normalizedDistance(float2 center, float2 p, float2 r)
            {
                float dx = center.x - p.x;
                float dy = center.y - p.y;
                dx = dx / r.x;
                dy = dy / r.y;
                return sqrt(dx * dx + dy * dy);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float sdf = 0.0;
                int operation = SDFOperation_Union;

                for (int j = 0; j < _SDFDataLength; )
                {
                    uint startIndex = j;
                    
                    // center 
                    float2 center = float2(readFromBuffer(startIndex), readFromBuffer(startIndex + 1));

                    // radius
                    float2 radius = float2(readFromBuffer(startIndex + 2), readFromBuffer(startIndex + 3)) * 0.5f;

                    // calculate sdf value
                    float dist = normalizedDistance(center, i.uv, radius);
                    float circleSdf = 1 - dist;

                    if (j == 0)
                    {
                        sdf = circleSdf;
                    }
                    else
                    {
                        operation = (int)readFromBuffer(startIndex + 4);

                        switch (operation)
                        {
                        case SDFOperation_Union:
                            sdf = smax(sdf, circleSdf, _BlendRadius);
                            break;
                        case SDFOperation_Intersection:
                            sdf = smin(sdf, circleSdf, _BlendRadius);
                            break;
                        case SDFOperation_Subtraction:
                            sdf = smin(sdf, -circleSdf, _BlendRadius);
                            break;
                        }
                    }
                    
                    j = startIndex + 5;
                }

                float halfSoft = _Softness * 0.5f;
                sdf = smoothstep(-halfSoft, halfSoft, sdf);

                float4 col = i.color * sdf;
                col *= tex2D(_MainTex, i.uv);

                return col;
            }

            ENDCG
        }
    }
}
