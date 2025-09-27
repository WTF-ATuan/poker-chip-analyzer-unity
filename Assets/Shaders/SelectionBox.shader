Shader "Custom/SelectionBox"
{
    Properties
    {
        _Color ("Color", Color) = (0,1,0,1)
        _BorderWidth ("Border Width", Range(0.01, 0.1)) = 0.02
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
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
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _BorderWidth;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Create hollow rectangle effect
                float2 uv = i.uv;
                float border = _BorderWidth;
                
                // Check if pixel is within border area
                bool inBorder = (uv.x < border || uv.x > 1.0 - border || 
                               uv.y < border || uv.y > 1.0 - border);
                
                if (inBorder)
                {
                    return _Color;
                }
                else
                {
                    return fixed4(0, 0, 0, 0); // Transparent center
                }
            }
            ENDCG
        }
    }
}
