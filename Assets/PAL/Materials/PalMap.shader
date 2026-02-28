Shader "ayy/PAL/PalMap"
{
    Properties
    {
        _SpriteSheetTex("SpriteSheet",2D) = "white" {}
        [Toggle(ENABLE_TILE_INFO)] _EnableTileInfo("EnableTileDebugInfo",Float) = 0
        // 默认关闭,换成 sdlpal 里, 看 sprite 和哪些 tiles 相交
        // 再把这些相交的 tiles, 按照逻辑, 动态调整 tiles 相关顶点的 z值
        // 用这样的方式, 来解决 tiles 和 sprite 覆盖的问题
        //[Toggle(ENABLE_DEPTH_Z)] _EnableDepthZ("EnableDepthZ",Float) = 1    // 是否开启, 每个tile 的顶点坐标,使用逻辑 height,来当作 z值.  
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
                float4 color : COLOR;   // rgb 用于区分标记, tile 是否可以走; a用于存储,实际逻辑上的 z值
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            
            sampler2D _SpriteSheetTex;
            float _EnableTileInfo;
            // float _EnableDepthZ;

            v2f vert (appdata v)
            {
                v2f o;
                float4 localPos = v.vertex;
                // if (_EnableDepthZ > 0.5)      // 开启了 logic Z 的遮挡功能, 则不适用原本的z,而是使用 color.a 来当作 z值
                // {
                //     localPos.z = localPos.z - v.color.a;
                // }
                o.vertex = UnityObjectToClipPos(localPos);
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
                    //col.rgb *= i.color.rgb;
                    col.rgb += i.color.rgb;
                }

                //return float4(_EnableDepthZ,0.0,0.0,1.0);
                return col;
            }
            ENDCG
        }
    }
}
