Shader "Custom/RTAlphaGamma"    //名称路径
{
    Properties  //属性
    {
        // _MainTex ("Base (RGB)", 2D) = "white" { }
        _Gamma ("Gamma", Range(1, 5)) = 2.2
    }
    SubShader   //可以有多个，加载时选择支持的第一个
    {
        Pass    //渲染一次模型
        {
            Tags { "RenderType" = "Opaque" }    //系统应该在渲染非透明物体时调用我们
            LOD 200
            
            CGPROGRAM
            
            #pragma vertex VertexShaderFunction //定义顶点着色器
            #pragma fragment PixelShaderFunction    //定义片断着色器
            
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float _Gamma;
            
            struct v2f
            {
                float4 position: SV_POSITION;   //顶点位置
                float2 uv: TEXCOORD0;   //第一UV坐标
            };
            
            v2f VertexShaderFunction(appdata_img i)
            {
                v2f o;
                o.position = UnityObjectToClipPos(i.vertex);    //将顶点从模型坐标转换到裁剪坐标，矩阵转换
                o.uv = i.texcoord;
                return o;
            }
            
            float4 PixelShaderFunction(v2f i): SV_Target    //SV_Target系统值，表示该函数返回的是用于下一个阶段输出的颜色值
            {
                float4 col = tex2D(_MainTex, i.uv); //2D纹理采样
                col.a = pow(col.a, _Gamma);
                return col;
            }
            ENDCG
            
        }
    }
}