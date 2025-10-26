Shader "ayy/PAL/PalMap"
{
    Properties
    {
        _SpriteSheetTex("SpriteSheet",2D) = "white" {}
        [Toggle(ENABLE_TILE_INFO)] _EnableTileInfo("EnableTileDebugInfo",Float) = 0
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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                //float2 uv1 : TEXCOORD1;
                float4 color : COLOR;
            };
            
            sampler2D _SpriteSheetTex;
            float _EnableTileInfo;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                //o.uv1 = v.uv1;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 col = tex2D(_SpriteSheetTex, uv);
                if (_EnableTileInfo > 0.5)
                {
                    col *= i.color;
                }
                return col;
            }
            ENDCG
        }
    }
}
