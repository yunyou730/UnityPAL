using System.IO;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace ayy.pal.core
{
    public class Palette
    {
        public static int PALETTE_COLOR_COUNT = 256;
        private MKFLoader _patMKF = null;

        public void Load()
        {
            _patMKF = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "PAT.MKF"));
            _patMKF.Load();   
        }

        /*
         * Purpose:
         *  Get the specified palette in pat.mkf file.
         *
         * Parameters:
         *  [IN] iPaletteNum - number of the palette.
         *  [IN] fNight - whether use the night palette or not.
         *
         * Return value:
         *  Pointer to the palette.NULL if failed
         *
         */
        public PaletteColor[] GetPalette(int paletteIndex,bool isNightColor)
        {
            byte[] buf = _patMKF.ReadChunk(paletteIndex);
            if (buf == null || buf.Length == 0)
            {
                return null;
            }

            PaletteColor[] paletteColors = new PaletteColor[PALETTE_COLOR_COUNT];
            bool hasNight = !(buf.Length <= 256 * 3); // is palette has night colors
            if (!hasNight)
            {
                isNightColor = false;
            }
            
            for (int i = 0; i < PALETTE_COLOR_COUNT; i++)
            {
                var col = new PaletteColor();
                col.r = buf[isNightColor ? 256 * 3 : 0 + i * 3] << 2;
                col.g = buf[isNightColor ? 256 * 3 : 0 + i * 3 + 1] << 2;
                col.b = buf[isNightColor ? 256 * 3 : 0 + i * 3 + 2] << 2;
                paletteColors[i] = col;
            }
            return paletteColors;
        }
        
        public int GetPaletteCount()
        {
            int ret = _patMKF.GetChunkCount();
            return ret;
        }

        // public Texture2D CreateDebugTexture()
        // {
        //     var tex = new Texture2D(16, 16);
        //     tex.filterMode = FilterMode.Point;
        //     for (int x = 0; x < 16; x++)
        //     {
        //         for (int y = 0; y < 16; y++)
        //         {
        //             tex.SetPixel(x, y, new Color(0, 0, 0, 0));
        //         }
        //     }
        //     return tex;
        // }
    }

    public class PaletteColor
    {
        public int r, g, b;

        public Color ConvertToColor()
        {
            return new Color(r/255.0f, g/255.0f, b/255.0f,1.0f);
        }
    }

}

