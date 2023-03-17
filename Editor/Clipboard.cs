using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Tymski.Clipboard
{
    public class Clipboard : OdinEditorWindow
    {
        const string WINDOW_KEY = "Tools/Tymski/Clipboard";
        int historyLimit = 30;

        public List<ItemList> selectionHistoryByType = new();
        public List<Object> selectionHistory = new();

        [MenuItem(WINDOW_KEY)]
        static void OpenWindow()
        {
            GetWindow<Clipboard>().Show();
        }

        protected override void OnEnable()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
        }

        private void OnDisable()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
        }

        private void OnPrefabStageOpened(PrefabStage prefabStage)
        {
            AddToHistory(AssetDatabase.LoadAssetAtPath(prefabStage.assetPath, typeof(Object)));
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            AddToHistory(AssetDatabase.LoadAssetAtPath(scene.path, typeof(Object)));
        }

        void OnSelectionChange()
        {
            if (Selection.activeObject == null) return;
            AddToHistory(Selection.activeObject);
        }

        void AddToHistory(Object @object)
        {
            historyLimit *= 4;
            Add(selectionHistory, @object);
            historyLimit /= 4;

            string selectionType = @object.GetType().ToString();

            ItemList itemList = selectionHistoryByType.Find(itemList => itemList.name == selectionType);
            if (itemList == null)
            {
                itemList = new ItemList(selectionType);
                selectionHistoryByType.Add(itemList);
            }

            if (selectionType == "UnityEngine.GameObject" && (@object as GameObject).scene.path != null)
            {
                GameObject outermostPrefabInstanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(@object as GameObject);
                if (outermostPrefabInstanceRoot != null)
                {
                    GameObject prefabSourceRoot = PrefabUtility.GetCorrespondingObjectFromSource(outermostPrefabInstanceRoot);
                    if (prefabSourceRoot != null)
                    {
                        Add(itemList.objects, prefabSourceRoot);
                    }
                }
                return;
            }

            Add(itemList.objects, @object);
        }

        void Add<T>(List<T> list, T obj)
        {
            list.Remove(obj);
            list.Insert(0, obj);
            if (list.Count > historyLimit) list.RemoveAt(list.Count - 1);
            Clean(list);
        }

        void Clean<T>(List<T> list)
        {
            list.RemoveAll(item => item.ToString() == "null");
        }
    }

    [Serializable, InlineEditor]
    public class ItemList
    {
        [HideLabel] public string name;
        public List<Object> objects;

        public ItemList(string name)
        {
            this.name = name;
            objects = new List<Object>();
        }

        public ItemList() { }
    }

    public class Entry
    {
        public string path;
        public Object @object;
    }
}
