using System.IO;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace ayy.pal
{
    public class Palette
    {
        public static int PALETTE_COLOR_COUNT = 256;
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
            var palMKF = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "PAT.MKF"));
            palMKF.Load();
            byte[] buf = palMKF.ReadChunk(paletteIndex);
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
            var palMKF = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "PAT.MKF"));
            palMKF.Load();
            int ret = palMKF.GetChunkCount();
            return ret;
        }
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

