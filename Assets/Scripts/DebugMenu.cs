using ayy.pal;
using ayy.pal.core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ayy.debugging
{
    public class DebugMenu : MonoBehaviour
    {
        [Header("Palette")]
        [SerializeField] private Button _btnLoadPalette;
        [SerializeField] private TMP_Dropdown _dropdownPalette;
        [SerializeField] private GameObject _paletteTextureHolder;
        
        [Header("Map")]
        [SerializeField] private Button _btnLoadMap;
        [SerializeField] private TMP_Dropdown _dropdownMap;
        [SerializeField] private GameObject _mapSpriteFramePrefab;
        [SerializeField] private bool _showMapBottomLayer = true;
        [SerializeField] private bool _showMapTopLayer = true;
        
        [Header("Sprite")]
        [SerializeField] private Button _btnLoadSprite;
        [SerializeField] private TMP_Dropdown _dropdownSprite;
        [SerializeField] private GameObject _spriteFramesHolder;
        [SerializeField] private GameObject _spriteSheetHolder;
        [SerializeField] private GameObject _spritePresenterPrefab = null;
        private SpritePresenter _spritePresenter = null;
        
        [Header("Map")]
        [SerializeField] private GameObject _mapPresenterPrefab;
        private MapPresenter _mapPresenter = null;
        
        [Header("Camera")]
        [SerializeField] private GameObject _cameraGO;
        [SerializeField,Range(0,20)] private float _cameraMoveSpeed = 5.0f;
        [SerializeField,Range(1,50)] private float _cameraOrthoSize = 5.0f;
        [SerializeField,Range(1,20)] private float _cameraOrthoChangeSpeed = 10.0f;
        
        [Header("GamePlay")]
        [SerializeField] private Button _btnStartGame;
        
        private Texture2D[] _spriteFrames;
        
        private MapService _mapService = null;
        private PaletteService _paletteService = null;
        private SpriteService _spriteService = null;
        
        
        void Start()
        {
            _mapService = PalGame.GetInstance().GetService<MapService>();
            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
            _spriteService = PalGame.GetInstance().GetService<SpriteService>();
            
            InitDebugPalette();
            InitDebugMap();
            InitDebugPlayerSprite();
            InitForSpawnSprite();
        }

        private void Update()
        {
            UpdateForMoveCamera();
            UpdateForSwitchMap();
            UpdateForSwitchSprite();
            UpdateForSwitchSpriteFrame();
        }

        private void UpdateForMoveCamera()
        {
            if (_cameraGO == null)
            {
                return;
            }
            
            //_cameraOrthoSize
            if (Input.mouseScrollDelta.y != 0)
            {
                _cameraOrthoSize -= (Input.mouseScrollDelta.y * Time.deltaTime * _cameraOrthoChangeSpeed);
            }
            float othoSize = Mathf.Clamp(_cameraOrthoSize, 1.0f, 50.0f);
            _cameraGO.GetComponent<Camera>().orthographicSize = othoSize;
            
            Vector2 dir = Vector2.zero;
            if (Input.GetKey(KeyCode.W))
            {
                dir += Vector2.up;
            }
            if (Input.GetKey(KeyCode.S))
            {
                dir += Vector2.down;
            }
            if (Input.GetKey(KeyCode.A))
            {
                dir += Vector2.left;
            }
            if (Input.GetKey(KeyCode.D))
            {
                dir += Vector2.right;
            }
            if (dir.magnitude > 0.0f)
            {
                dir = dir.normalized * Time.deltaTime * _cameraMoveSpeed;
                Vector3 pos = _cameraGO.transform.localPosition;
                pos.x += dir.x;
                pos.y += dir.y;
                _cameraGO.transform.localPosition = pos;
            }
        }

        private void UpdateForSwitchMap()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                int next = _dropdownMap.value + 1;
                if (next > _dropdownMap.options.Count - 1)
                {
                    next = 0;
                }
                _dropdownMap.value = next;
                _dropdownMap.onValueChanged.Invoke(next);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                int next = _dropdownMap.value - 1;
                if (next < 0)
                {
                    next = _dropdownMap.options.Count - 1;
                }
                _dropdownMap.value = next;
                _dropdownMap.onValueChanged.Invoke(next);
            }
        }

        private void UpdateForSwitchSprite()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                int next = _dropdownSprite.value + 1;
                if (next > _dropdownSprite.options.Count - 1)
                {
                    next = 0;
                }
                _dropdownSprite.value = next;
                _dropdownSprite.onValueChanged.Invoke(next);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                int next = _dropdownSprite.value - 1;
                if (next < 0)
                {
                    next = _dropdownSprite.options.Count - 1;
                }
                _dropdownSprite.value = next;
                _dropdownSprite.onValueChanged.Invoke(next);
            }
        }

        private void UpdateForSwitchSpriteFrame()
        {
            if (Input.GetKeyDown(KeyCode.P) && _spritePresenter != null)
            {
                _spritePresenter.SwitchNextFrame();
            }
        }

        private void InitDebugPalette()
        {
            _btnLoadPalette.onClick.AddListener(OnClickLoadPalette);
            _dropdownPalette.onValueChanged.AddListener(OnClickPalette);
            _dropdownPalette.options.Clear();
            
            var mat = _paletteTextureHolder.GetComponent<MeshRenderer>().material;
            mat.SetTexture(Shader.PropertyToID("_Texture2D"), _paletteService.GetPaletteTexture());
        }

        private void InitDebugMap()
        {
            _btnLoadMap.onClick.AddListener(OnClickLoadAllMaps);
            _dropdownMap.onValueChanged.AddListener(OnClickSwitchMap);
            _dropdownMap.options.Clear();
        }

        private void InitDebugPlayerSprite()
        {
            _btnLoadSprite.onClick.AddListener(LoadAllSprites);
            _dropdownSprite.onValueChanged.AddListener(OnClickSwitchSprite);
        }

        private void InitForSpawnSprite()
        {
            
        }

        private void OnClickLoadPalette()
        {
            Debug.Log("Load All Palettes");
            _dropdownPalette.options.Clear();
            int cnt = _paletteService.GetPaletteCount();
            for (int i = 0;i < cnt;i++)
            {
                _dropdownPalette.options.Add(new TMP_Dropdown.OptionData($"palette_[{i}]"));
            }
            _dropdownPalette.value = 0;
            _dropdownPalette.onValueChanged.Invoke(0);
        }

        private void OnClickPalette(int index)
        {
            Debug.Log($"Load Palette {index}");
            _paletteService.LoadPalette(index,false);
        }

        private void OnClickLoadAllMaps()
        {
            Debug.Log("Load All Maps");
            int mapCnt = _mapService.GetMapWrapper().GetMapCount();
            _dropdownMap.options.Clear();
            for (int i = 0; i < mapCnt; i++)
            {
                _dropdownMap.options.Add(new TMP_Dropdown.OptionData($"map_{i}"));
            }
            _dropdownMap.value = 0;
            _dropdownMap.onValueChanged.Invoke(0);
        }

        private void OnClickSwitchMap(int mapIndex)
        {
            LoadMapWithSingleDrawCall(mapIndex);
        }

        private void LoadMapWithSingleDrawCall(int mapIndex)
        {
            if (_mapPresenter == null)
            {
                _mapPresenter = GameObject.Instantiate(_mapPresenterPrefab).GetComponent<MapPresenter>();
            }
            else
            {
                _mapPresenter.Unload();
            }

            _mapPresenter.Load(mapIndex);
        }
        

        private void LoadAllSprites()
        {
            int cnt = _spriteService.GetSpriteCount();
            _dropdownSprite.ClearOptions();
            for (int i = 0;i < cnt;i++)
            {
                _dropdownSprite.options.Add(new TMP_Dropdown.OptionData($"sprite[{i}]"));
            }
        }
        
        private void OnClickSwitchSprite(int spriteIndex)
        {
            foreach (Transform child in _spriteFramesHolder.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            
            // @miao @test
            //LoadSprite(spriteIndex);
            LoadSprite2(spriteIndex);
            RefreshSpritePresenter(spriteIndex);
        }

        unsafe private void LoadSprite(int spriteIndex)
        {
            // 获取原始 sprite 数据 ,并解压缩
            byte[] sprite = _spriteService.GetMgoMKF().ReadChunk(spriteIndex);
            int decompressedSize = _spriteService.GetMgoMKF().GetDecompressedSize(spriteIndex);
            byte[] decompressedSprite = new byte[decompressedSize];
            fixed (byte* pChunkData = sprite)
            {
                fixed (byte* pDestData = decompressedSprite)
                {
                    Yj1Decompressor.YJ1_Decompress(pChunkData, pDestData,decompressedSize);
                }
            }
            
            // 拿到 sprite 数据,去创建 texture,并展示出来
            PaletteColor[] paletteColors = _paletteService.GetPaletteColors();
            SpriteTextureHelper.CreateSpriteFramesGameObjects(decompressedSprite, paletteColors,_mapSpriteFramePrefab,_spriteFramesHolder.transform);
        }
        
        private void LoadSprite2(int spriteIndex)
        {
            PALSprite sprite = _spriteService.GetSprite(spriteIndex);
            if (sprite.GetTexture() == null)
            {
                return;
            }

            var tex = sprite.GetTexture();
            var mat = _spriteSheetHolder.GetComponent<MeshRenderer>().material;
            mat.SetTexture(Shader.PropertyToID("_MainTex"),sprite.GetTexture());
            
            float sy = _spriteSheetHolder.transform.localScale.y;
            float sx = sy * tex.width / tex.height;
            _spriteSheetHolder.transform.localScale = new Vector3(sx,sy,1);
        }

        private void RefreshSpritePresenter(int spriteIndex)
        {
            if (_spritePresenter == null)
            {
                _spritePresenter = GameObject.Instantiate(_spritePresenterPrefab).GetComponent<SpritePresenter>();
            }
            _spritePresenter.SwitchSpriteFrame(spriteIndex,0);
        }
    }
}

