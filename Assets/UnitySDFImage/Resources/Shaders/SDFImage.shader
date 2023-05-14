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

            #define Shape_Circle 0
            #define Shape_Rect 1
            #define Shape_RegularPolygon 2

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            float sdfCircle(float2 pt, inout uint index)
            {
                float2 center = float2(readFromBuffer(index++), readFromBuffer(index++));

                float2 radius = float2(readFromBuffer(index++), readFromBuffer(index++)) * 0.5f;

                float dx = center.x - pt.x;
                float dy = center.y - pt.y;
                dx = dx / radius.x;
                dy = dy / radius.y;
                float normalizedDistance = sqrt(dx * dx + dy * dy);
                return 1 - normalizedDistance;
            }
            
            float sdfRect(float2 pt, inout uint index)
            {
                float2 center = float2(readFromBuffer(index++), readFromBuffer(index++));

                float2 sizeHalf = float2(readFromBuffer(index++), readFromBuffer(index++)) * 0.5f;

                float dx = center.x - pt.x;
                float dy = center.y - pt.y;
                dx = dx / sizeHalf.x;
                dy = dy / sizeHalf.y;

                float2 normalizedDistance = float2(abs(dx) - 0.5, abs(dy) - 0.5);

                float dist = max(max(normalizedDistance.x, normalizedDistance.y), 0);
                return 0.5 - dist;
            }
            
            float sdfRegularPolygon(float2 pt, inout uint index)
            {
                float2 center = float2(readFromBuffer(index++), readFromBuffer(index++));
                    
                float2 sizeHalf = float2(readFromBuffer(index++), readFromBuffer(index++)) * 0.5f;
                int n = (int)readFromBuffer(index++);
                float startAngle = readFromBuffer(index++);

                float dx = pt.x - center.x;
                float dy = pt.y - center.y;
                dx = dx / sizeHalf.x;
                dy = dy / sizeHalf.y;

                float2 pointLocal = float2(dx, dy);

                float angle = 2 * PI / n;
                float halfAngle = angle / 2;
                float cosHalfAngle = cos(halfAngle);
                float sinHalfAngle = sin(halfAngle);
                
                float radius = cosHalfAngle / (1.0f + cosHalfAngle);
                
                float pointAngle = atan2(pointLocal.y, pointLocal.x) - startAngle;
                if (pointAngle < 0)
                {
                    pointAngle += 2 * PI;
                }
                
                int closestEdgeIndex = (int)floor(pointAngle / angle);
                
                float angleEdge = closestEdgeIndex * angle + startAngle;
                float2 pointEdge = float2(radius * cos(angleEdge), radius * sin(angleEdge));
                float angleNextEdge = (closestEdgeIndex + 1) * angle + startAngle;
                float2 pointNextEdge = float2(radius * cos(angleNextEdge), radius * sin(angleNextEdge));
                
                float distToSegment = distanceToLine(pointLocal,pointEdge,pointNextEdge );
                
                return radius - distToSegment;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float sdf = 0;
                float sdfTemp = 0;
                int operation = SDFOperation_Union;
                int shape = Shape_Circle;

                for (int j = 0; j < _SDFDataLength; )
                {
                    bool first = j == 0;
                
                    shape = (int)readFromBuffer(j++);

                    switch (shape)
                    {
                    case Shape_Circle:
                        sdfTemp = sdfCircle(i.uv, j);
                        break;
                    case Shape_Rect:
                        sdfTemp = sdfRect(i.uv, j);
                        break;
                    case Shape_RegularPolygon:
                        sdfTemp = sdfRegularPolygon(i.uv, j);
                        break;
                    }

                    operation = (int)readFromBuffer(j++);
                
                    if (first)
                    {
                        sdf = sdfTemp;
                    }
                    else
                    {
                        switch (operation)
                        {
                        case SDFOperation_Union:
                            sdf = smax(sdf, sdfTemp, _BlendRadius);
                            break;
                        case SDFOperation_Intersection:
                            sdf = smin(sdf, sdfTemp, _BlendRadius);
                            break;
                        case SDFOperation_Subtraction:
                            sdf = smin(sdf, -sdfTemp, _BlendRadius);
                            break;
                        }
                    }
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
