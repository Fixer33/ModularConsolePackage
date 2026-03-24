using System.Collections.Generic;
using ModularConsole.Contracts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ModularConsole.Modules
{
    public class SceneHierarchyModule : PersistentModule
    {
        public override string ModuleName => "Scenes hierarchy";

        private readonly TreeView _treeView = new() { name = "container" };

        public SceneHierarchyModule()
        {
            _treeView.makeItem = () =>
            {
                var container = new VisualElement();
                container.name = "tree-item-container";
                container.style.flexDirection = FlexDirection.Row;
                container.style.alignItems = Align.Center;
                container.style.height = 30; // Consistent height
                container.style.paddingLeft = 5;

                var toggle = new Toggle { name = "active-toggle" };
                toggle.AddToClassList("logFilterToggle");
                toggle.style.marginRight = 5;
                toggle.style.marginLeft = 0;
                toggle.style.marginTop = 0;
                toggle.style.marginBottom = 0;

                var label = new Label { name = "name-label" };
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.flexGrow = 1;
                label.style.height = Length.Percent(100);
                label.style.unityTextAlign = TextAnchor.MiddleLeft;

                container.Add(toggle);
                container.Add(label);

                container.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    container.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                });
                container.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    container.style.backgroundColor = new Color(0, 0, 0, 0);
                });

                return container;
            };

            _treeView.bindItem = (VisualElement element, int index) =>
            {
                var item = _treeView.GetItemDataForIndex<ISceneTreeViewItem>(index);
                var toggle = element.Q<Toggle>("active-toggle");
                var label = element.Q<Label>("name-label");

                label.text = item.Name;

                if (item.GameObject != null)
                {
                    toggle.style.display = DisplayStyle.Flex;
                    toggle.UnregisterCallback<ChangeEvent<bool>>(OnToggleChanged);
                    toggle.value = item.GameObject.activeSelf;
                    toggle.RegisterCallback<ChangeEvent<bool>>(OnToggleChanged);
                    toggle.userData = item.GameObject;
                }
                else
                {
                    toggle.style.display = DisplayStyle.None;
                }
            };
        }

        private void OnToggleChanged(ChangeEvent<bool> evt)
        {
            if (evt.target is Toggle toggle && toggle.userData is GameObject gameObject)
            {
                gameObject.SetActive(evt.newValue);
            }
        }

        ~SceneHierarchyModule()
        {
        }

        public override void ConstructUI(VisualElement root)
        {
            var styleSheet = Resources.Load<StyleSheet>("ConsoleModule");
            if (styleSheet != null)
                root.styleSheets.Add(styleSheet);

            int counter = 0;

            TreeViewItemData<ISceneTreeViewItem> ConstructElement<T>(string name, GameObject[] children, GameObject currentGameObject = null)
                where T : NamedSceneTreeViewItem, new()
            {
                List<TreeViewItemData<ISceneTreeViewItem>> childrenElements = new();
                foreach (var gameObject in children)
                {
                    if (gameObject == null) continue;

                    GameObject[] childrenObjects = new GameObject[gameObject.transform.childCount];
                    int childCounter = 0;
                    foreach (Transform child in gameObject.transform)
                    {
                        childrenObjects[childCounter++] = child.gameObject;
                    }

                    childrenElements.Add(ConstructElement<ObjectRecord>(gameObject.name, childrenObjects, gameObject));
                }

                T item = currentGameObject != null ? new ObjectRecord(currentGameObject) as T : new T();
                item.SetName(name);
                return new TreeViewItemData<ISceneTreeViewItem>(counter++, item, childrenElements);
            }

            root.Add(_treeView);

            List<TreeViewItemData<ISceneTreeViewItem>> items = new();
            // Get dont destroy on load objects
            var buf = new GameObject();
            Object.DontDestroyOnLoad(buf);
            items.Add(ConstructElement<SceneRecord>("Don't destroy on load", buf.scene.GetRootGameObjects()));
            Object.Destroy(buf);

            // Add other scenes objects
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    items.Add(ConstructElement<SceneRecord>(scene.name, scene.GetRootGameObjects()));
                }
            }

            _treeView.SetRootItems(items);
        }

        private interface ISceneTreeViewItem
        {
            public string Name { get; }
            public GameObject GameObject { get; }
        }

        private abstract class NamedSceneTreeViewItem : ISceneTreeViewItem
        {
            public string Name { get; private set; }
            public virtual GameObject GameObject => null;

            public NamedSceneTreeViewItem()
            {
            }

            public void SetName(string name) => Name = name;
        }

        private class SceneRecord : NamedSceneTreeViewItem
        {
        }

        private class ObjectRecord : NamedSceneTreeViewItem
        {
            public override GameObject GameObject { get; }

            public ObjectRecord(GameObject gameObject)
            {
                GameObject = gameObject;
            }

            public ObjectRecord()
            {
            }
        }
    }
}