Shader "Hidden/DoodleExport"
{
    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #pragma vertex vert  
            #pragma fragment frag  

            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            appdata vert(appdata v)
            {
                appdata o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(appdata i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);
                return fixed4(color.rgb, 1.0);
            }
            ENDCG
        }
    }
}