using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ayy.debugging
{
    public class DebugMenu : MonoBehaviour
    {
        [SerializeField] private Button _btnLoadPalette;
        [SerializeField] private TMP_Dropdown _dropdownPalette;
        [SerializeField] private GameObject _paletteTextureHolder;
        
        private ayy.pal.Palette _palette;
        private Texture2D _paletteTexture;
        
        void Start()
        {
            InitDebugPalette();
        }

        private void InitDebugPalette()
        {
            _btnLoadPalette.onClick.AddListener(OnClickLoadPalette);
            _dropdownPalette.onValueChanged.AddListener(OnClickPalette);
            _dropdownPalette.options.Clear();
            _paletteTexture = new Texture2D(16, 16);
            _paletteTexture.filterMode = FilterMode.Point;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    _paletteTexture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
            var mat = _paletteTextureHolder.GetComponent<MeshRenderer>().material;
            mat.SetTexture(Shader.PropertyToID("_Texture2D"), _paletteTexture);
        }

        private void OnClickLoadPalette()
        {
            Debug.Log("Load Palette");
            _palette = new ayy.pal.Palette();
            _dropdownPalette.options.Clear();
            int paletteCount = _palette.GetPaletteCount();
            for (int i = 0;i < paletteCount;i++)
            {
                _dropdownPalette.options.Add(new TMP_Dropdown.OptionData($"palette_[{i}]"));
            }
        }

        private void OnClickPalette(int index)
        {
            Debug.Log($"Load Palette {index}");
            ayy.pal.PaletteColor[] colors = _palette.GetPalette(index,false);
            for (int i = 0;i < ayy.pal.Palette.PALETTE_COLOR_COUNT;i++)
            {
                int x = i % 16;
                int y = i / 16;
                _paletteTexture.SetPixel(x,y,colors[i].ConvertToColor());
            }
            _paletteTexture.Apply();
        }
    }
}

