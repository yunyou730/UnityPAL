using System.IO;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace ayy.pal
{
    public class Palette
    {
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

            PaletteColor[] paletteColors = new PaletteColor[256];
            bool hasNight = !(buf.Length <= 256 * 3); // is palette has night colors
            if (!hasNight)
            {
                isNightColor = false;
            }
            
            for (int i = 0; i < 256; i++)
            {
                var col = new PaletteColor();
                col.r = buf[isNightColor ? 256 * 3 : 0 + i * 3] << 2;
                col.g = buf[isNightColor ? 256 * 3 : 0 + i * 3 + 1] << 2;
                col.b = buf[isNightColor ? 256 * 3 : 0 + i * 3 + 2] << 2;
                paletteColors[i] = col;
            }
            return paletteColors;
        }
    }

    public class PaletteColor
    {
        public int r, g, b, a;
    }

}

