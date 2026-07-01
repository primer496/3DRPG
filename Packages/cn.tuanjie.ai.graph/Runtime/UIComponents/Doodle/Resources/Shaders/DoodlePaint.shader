Shader "Hidden/DoodlePaint"
{
    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            float4 _Pos;
            float  _Radius;
            float4 _Color;
            float  _Seamless;

            float squareDist(float2 a, float2 b, float2 c)
            {
                float2 ab = b - a;
                float2 ac = c - a;
                float2 bc = c - b;

                float e = dot(ac, ab);
                if (e <= 0.0)
                    return dot(ac, ac);

                float f = dot(ab, ab);
                if (e >= f)
                    return dot(bc, bc);
                return dot(ac, ac) - e * e / f;
            }

            bool isInArea(float2 p, float2 size, float r) {
                return p.x >= r && p.x <= size.x - r && p.y >= r && p.y <= size.y - r;
            }

            float mod(float x, float y) {
                return x - y * floor(x / y);
            }

            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 coord = IN.globalTexcoord.xy;
                float2 a = float2(_Pos.x, _Pos.y);
                float2 b = float2(_Pos.z, _Pos.w);

                if (_Seamless > 0) {
                    float lbx = min(a.x, b.x);
                    float lby = min(a.y, b.y);
                    float2 dxy = float2(mod(lbx, _CustomRenderTextureWidth) - lbx, mod(lby, _CustomRenderTextureHeight) - lby);
                    a += dxy;
                    b += dxy;
                }

                float2 c = float2(coord.x * _CustomRenderTextureWidth, coord.y * _CustomRenderTextureHeight);

                float2 size = float2(_CustomRenderTextureWidth, _CustomRenderTextureHeight);
                float sqDist = squareDist(a, b, c);
                float sqRadius = _Radius * _Radius;
                bool shouldPaint = sqDist < sqRadius;

                if (_Seamless > 0) {
                    // seamless mode
                    if (!isInArea(a, size, _Radius) || !isInArea(b, size, _Radius)) {
                        float2 c1 = float2(c.x + _CustomRenderTextureWidth,    c.y);
                        float2 c2 = float2(c.x,                                c.y + _CustomRenderTextureHeight);
                        float2 c3 = float2(c.x + _CustomRenderTextureWidth,    c.y + _CustomRenderTextureHeight);
                        float sqDist1 = squareDist(a, b, c1);
                        float sqDist2 = squareDist(a, b, c2);
                        float sqDist3 = squareDist(a, b, c3);
                        shouldPaint = shouldPaint || (sqDist1 < sqRadius) || (sqDist2 < sqRadius) || (sqDist3 < sqRadius);
                    }
                }

                //clamp mode
                return shouldPaint ? _Color : tex2D(_SelfTexture2D, IN.globalTexcoord.xy);

            }
            ENDCG
        }
    }
}