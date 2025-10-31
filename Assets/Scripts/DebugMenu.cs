using System;
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
        [SerializeField] private TMP_Dropdown _dropdownPalette;
        [SerializeField] private GameObject _paletteTextureHolder;
        
        [Header("Map")]
        [SerializeField] private TMP_Dropdown _dropdownMap;
        [SerializeField] private Button _btnToggleMapTileInfo;
        [SerializeField] private GameObject _mapSpriteFramePrefab;
        
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
        [SerializeField] private Button _btnLoadDefaultGame;
        
        [Header("Pos")]
        [SerializeField] private TMP_InputField _inputFieldPos;
        [SerializeField] private Button _btnSetPos;
        
        private Texture2D[] _spriteFrames;
        
        private MapService _mapService = null;
        private PaletteService _paletteService = null;
        private SpriteService _spriteService = null;
        private ViewportService _viewportService = null;
        private PALGameplayService _gameplayService = null;
        
        void Start()
        {
            _mapService = PalGame.GetInstance().GetService<MapService>();
            _paletteService = PalGame.GetInstance().GetService<PaletteService>();
            _spriteService = PalGame.GetInstance().GetService<SpriteService>();
            _viewportService = PalGame.GetInstance().GetService<ViewportService>();
            _gameplayService = PalGame.GetInstance().GetService<PALGameplayService>();
            
            InitDebugPalette();
            InitDebugMap();
            InitDebugPlayerSprite();
            InitForSpawnSprite();
            _btnLoadDefaultGame.onClick.AddListener(LoadDefaultGame);
            _btnSetPos.onClick.AddListener(SetTestPos);
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
            
            if (Input.GetKeyDown(KeyCode.L))
            {
                int viewportPixelX = 1152;
                int viewportPixelY = 832;
                _viewportService.GetViewport().RefreshCoord(viewportPixelX, viewportPixelY);
                if(_spritePresenter != null)
                {
                    int spriteOffsetPixelX = 149;
                    int spriteOffsetPixelY = 66;
                    int pixelX = viewportPixelX + spriteOffsetPixelX;
                    int pixelY = viewportPixelY + spriteOffsetPixelY;
                    _spritePresenter.SetPixelPos(pixelX, pixelY);
                }
            }
        }

        private void InitDebugPalette()
        {
            _dropdownPalette.onValueChanged.AddListener(OnClickPalette);
            _dropdownPalette.options.Clear();
            int cnt = _paletteService.GetPaletteCount();
            for (int i = 0;i < cnt;i++)
            {
                _dropdownPalette.options.Add(new TMP_Dropdown.OptionData($"palette_[{i}]"));
            }
            var mat = _paletteTextureHolder.GetComponent<MeshRenderer>().material;
            mat.SetTexture(Shader.PropertyToID("_Texture2D"), _paletteService.GetPaletteTexture());
        }

        private void InitDebugMap()
        {
            _btnToggleMapTileInfo.onClick.AddListener(OnClickToggleMapTileInfo);
            _dropdownMap.onValueChanged.AddListener(OnClickSwitchMap);
            _dropdownMap.options.Clear();
            int mapCnt = _mapService.GetMapWrapper().GetMapCount();
            for (int i = 0; i < mapCnt; i++)
            {
                _dropdownMap.options.Add(new TMP_Dropdown.OptionData($"map_{i}"));
            }
        }

        private void InitDebugPlayerSprite()
        {
            _btnLoadSprite.onClick.AddListener(LoadAllSprites);
            _dropdownSprite.onValueChanged.AddListener(OnClickSwitchSprite);
        }

        private void InitForSpawnSprite()
        {
            
        }

        private void OnClickPalette(int index)
        {
            Debug.Log($"Load Palette {index}");
            _paletteService.LoadPalette(index,false);
        }

        private void OnClickToggleMapTileInfo()
        {
            if (_mapPresenter != null)
            {
                _mapPresenter.ToggleDebugTileInfo();
            }
        }

        private void OnClickSwitchMap(int mapIndex)
        {
            LoadMapWithSingleDrawCall(mapIndex);
        }

        private void LoadMapWithSingleDrawCall(int mapIndex)
        {
            // if (_mapPresenter == null)
            // {
            //     _mapPresenter = GameObject.Instantiate(_mapPresenterPrefab).GetComponent<MapPresenter>();
            // }
            // else
            // {
            //     _mapPresenter.Unload();
            // }

            _mapPresenter = _gameplayService.GetMapPresenter();
            _mapPresenter.Unload();
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
        
        private void LoadDefaultGame()
        {
            Debug.Log("Load Default Game");
            PalGame.GetInstance().GetService<LoadGameService>().LoadDefaultGame();
        }

        private void SetTestPos()
        {
            try
            {
                string str = _inputFieldPos.text;
                string[] strs = str.Split(",");
                int viewportX = int.Parse(strs[0]);
                int viewportY = int.Parse(strs[1]);
                int partyOffsetX = int.Parse(strs[2]);
                int partyOffsetY = int.Parse(strs[3]);
                
                _viewportService.GetViewport().RefreshCoord(viewportX, viewportY);
                if (_spritePresenter != null)
                {
                    int px = viewportX + partyOffsetX;
                    int py = viewportY + partyOffsetY;
                    _spritePresenter.SetPixelPos(px, py);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}

