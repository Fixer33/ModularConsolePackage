using System;
using System.IO;
using UnityEngine;

namespace ModularConsole.Samples
{
    public class ConsoleSequenceOpener : MonoBehaviour
    {
        #region Initialization
        private const string PREFAB_NAME_IS_RESOURCES = "ConsoleSequenceOpener";
        private const string MENU_ITEM_PATH = "Assets/Fixer33/Create console opener";
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateFromResources()
        {
            if (Application.isPlaying == false)
                return;
            
            var prefab = Resources.Load<ConsoleSequenceOpener>(Path.Combine("ModularConsole", PREFAB_NAME_IS_RESOURCES));
            if (prefab == false)
            {
                Debug.LogError("No prefab ConsoleSequenceOpener found in resources. Use " + MENU_ITEM_PATH);
                return;
            }

            var obj = Instantiate(prefab);
            obj.name = obj.name.Replace("(Clone)", " [Spawned from resources]");
            DontDestroyOnLoad(obj);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem(MENU_ITEM_PATH)]
        private static void CreatePrefabInResources()
        {
            if (UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources") == false)
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
            
            if (UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources/ModularConsole") == false)
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", "ModularConsole");

            string path = $"Assets/Resources/ModularConsole/{PREFAB_NAME_IS_RESOURCES}.prefab";
            if (UnityEditor.AssetDatabase.AssetPathExists(path))
                return;

            var obj = new GameObject("ConsoleSequenceOpener");
            var prefab = obj.AddComponent<ConsoleSequenceOpener>();
            UnityEditor.PrefabUtility.SaveAsPrefabAsset(obj, path);
            UnityEditor.AssetDatabase.SaveAssets();
        }
        
        [UnityEditor.MenuItem(MENU_ITEM_PATH, true)]
        private static bool CreatePrefabInResources_Validate()
        {
            return UnityEditor.AssetDatabase.AssetPathExists($"Assets/Resources/ModularConsole/{PREFAB_NAME_IS_RESOURCES}.prefab") == false;
        }
#endif
        #endregion
        
        [SerializeField] private KeyCode[] _sequence = { KeyCode.C, KeyCode.N, KeyCode.S, KeyCode.L };
        [SerializeField] private ScreenPosClickData[] _clickSequence = {
            new(0, 0, 100),
            new(0, 0, 100),
            new(1, 1, 100),
            new(1, 1, 100),
            new(0, 0, 100),
            new(0, 0, 100),
            new(0, 0, 100),
        };
        private int _requiredKeyIndex = 0;
        private int _requiredClickIndex = 0;
        private float _screenWidth, _screenHeight;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _screenHeight = Screen.height;
            _screenWidth = Screen.width;
        }

        private void Update()
        {
            if (ConsoleSystem.UI.IsVisible)
                return;
            
            if (Input.anyKeyDown && _sequence is { Length: > 0 })
            {
                if (Input.GetKeyDown(_sequence[_requiredKeyIndex]))
                {
                    if (++_requiredKeyIndex >= _sequence.Length)
                    {
                        _requiredKeyIndex = 0;
                        ConsoleSystem.UI.Show();
                    }
                }
                else
                {
                    _requiredKeyIndex = 0;
                }
            }

            if (Input.GetMouseButtonUp(0) && _clickSequence is { Length: > 0 })
            {
                var pos = Input.mousePosition;
                if (Mathf.Abs(pos.y - _clickSequence[_requiredClickIndex].YPosPosPercentage * _screenHeight) < _clickSequence[_requiredClickIndex].ClickRadius && 
                    Mathf.Abs(pos.x - _clickSequence[_requiredClickIndex].XPosPosPercentage * _screenWidth) < _clickSequence[_requiredClickIndex].ClickRadius)
                {
                    if (++_requiredClickIndex >= _clickSequence.Length)
                    {
                        _requiredClickIndex = 0;
                        ConsoleSystem.UI.Show();
                    }
                }
                else
                {
                    _requiredClickIndex = 0;
                }
            }
        }
        
        [Serializable]
        public struct ScreenPosClickData
        {
            [Range(0, 1f)] public float XPosPosPercentage;
            [Range(0, 1f)] public float YPosPosPercentage;
            public float ClickRadius;

            public ScreenPosClickData(float xPosPosPercentage, float yPosPosPercentage, float clickRadius)
            {
                XPosPosPercentage = xPosPosPercentage;
                YPosPosPercentage = yPosPosPercentage;
                ClickRadius = clickRadius;
            }
        }
    }
}