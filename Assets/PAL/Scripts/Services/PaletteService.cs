using ayy.pal.core;
using UnityEngine;

namespace ayy.pal
{
    public class PaletteService : Service,IInitializable,IDestroyable
    {
        private Palette _palette = null;
        private Texture2D _texture = null;      // 16 x 16. 左下角[0], 右上角 [255]
        private PaletteColor[] _paletteColors = null;

        private static int PALETTE_COLOR_COUNT = 256;
        private static int PALETTE_TEXTURE_SIDE_SIZE = 16;
        
        public void Init()
        {
            _palette = new Palette();
            _palette.Load();
            _texture = CreatePaletteTexture();
        }

        public void Destroy()
        {

        }

        public void LoadPalette(int index,bool isNight)
        {
            _paletteColors = _palette.GetPalette(index, isNight);
            for (int i = 0;i < Palette.PALETTE_COLOR_COUNT;i++)
            {
                int x = i % PALETTE_TEXTURE_SIDE_SIZE;
                int y = i / PALETTE_TEXTURE_SIDE_SIZE;
                _texture.SetPixel(x,y,_paletteColors[i].ConvertToColor());
            }
            _texture.Apply();
        }

        public int GetPaletteCount()
        {
            return _palette.GetPaletteCount();
        }

        public Texture2D GetPaletteTexture()
        {
            return _texture;
        }

        public PaletteColor[] GetPaletteColors()
        {
            return _paletteColors;
        }

        public Color[] GetPaletteColorsInUnity()
        {
            Color[] colors = new Color[PALETTE_COLOR_COUNT];
            for(int i = 0;i < _paletteColors.Length;i++)
            {
                colors[i] = _paletteColors[i].ConvertToColor();
            }
            return colors;
        }

        private Texture2D CreatePaletteTexture()
        {
            var tex = new Texture2D(PALETTE_TEXTURE_SIDE_SIZE, PALETTE_TEXTURE_SIDE_SIZE);
            tex.filterMode = FilterMode.Point;
            return tex;
        }
    }
}

