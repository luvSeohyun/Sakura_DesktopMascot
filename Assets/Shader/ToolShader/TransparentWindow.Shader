Shader "Custom/RTAlphaGamma"
{
    Properties
    {
        // _MainTex ("Base (RGB)", 2D) = "white" { }
        _Gamma ("Gamma", Range(1, 5)) = 2.5
    }
    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200
            
            CGPROGRAM
            
            #pragma vertex VertexShaderFunction
            #pragma fragment PixelShaderFunction
            
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float _Gamma;
            
            struct v2f
            {
                float4 position: SV_POSITION;
                float2 uv: TEXCOORD0;
            };
            
            v2f VertexShaderFunction(appdata_img i)
            {
                v2f o;
                o.position = UnityObjectToClipPos(i.vertex);
                o.uv = i.texcoord;
                return o;
            }
            
            float4 PixelShaderFunction(v2f i): SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                col.a = pow(col.a, _Gamma);
                return col;
            }
            ENDCG
            
        }
    }
}