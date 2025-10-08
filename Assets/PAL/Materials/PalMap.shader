Shader "ayy/PAL/PalMap"
{
    Properties
    {
        _SpriteSheetTex("SpriteSheet",2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZTest Off
        LOD 100
        
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
                //float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                //float2 uv1 : TEXCOORD1;
            };
            sampler2D _SpriteSheetTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                //o.uv1 = v.uv1;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                //uv = uv / 16.0f;
                float4 col = tex2D(_SpriteSheetTex, uv);

                // int frameIndex = (int)i.uv1.x;
                //
                // float2 uvOffset = float2(0,0);
                // float ox = frameIndex % 16;
                // float oy = frameIndex / 16;
                
                
                return col;

                
                return float4(i.uv,0.0,1.0);
            }
            ENDCG
        }
    }
}
