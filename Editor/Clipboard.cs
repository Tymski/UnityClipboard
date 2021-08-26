using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tymski.Clipboard
{
    public class Clipboard : OdinEditorWindow
    {
        const string WINDOW_KEY = "Tools/Tymski/Clipboard";
        public int historyLimit = 20;
        public string search = "";

        [ShowIf("@search.Length > 0")]
        public List<UnityEngine.Object> searchList = new List<UnityEngine.Object>();
        public List<ItemList> selectionHistoryByType = new List<ItemList>();
        public List<UnityEngine.Object> selectionHistory = new List<UnityEngine.Object>();

        [MenuItem(WINDOW_KEY)]
        static void OpenWindow()
        {
            GetWindow<Clipboard>().Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // var data = EditorPrefs.GetString(WINDOW_KEY, JsonUtility.ToJson(this, false));
            // JsonUtility.FromJsonOverwrite(data, this);
        }

        void OnSelectionChange()
        {
            if (Selection.activeObject == null) return;

            historyLimit *= 4;
            Add(selectionHistory, Selection.activeObject);
            historyLimit /= 4;

            string selectionType = Selection.activeObject.GetType().ToString();

            bool contains = false;
            foreach (ItemList item in selectionHistoryByType)
            {
                if (item.listName == selectionType) contains = true;
            }
            if (!contains)
            {
                selectionHistoryByType.Add(new ItemList(selectionType));
            }

            foreach (ItemList item in selectionHistoryByType)
            {
                if (item.listName != selectionType) continue;

                if (selectionType == "UnityEngine.GameObject" && Selection.activeGameObject.scene.path != null)
                {
                    if (PrefabUtility.GetOutermostPrefabInstanceRoot(Selection.activeGameObject) != null)
                        if (PrefabUtility.GetCorrespondingObjectFromSource(PrefabUtility.GetOutermostPrefabInstanceRoot(Selection.activeGameObject)) != null)
                            Add(item.objects, PrefabUtility.GetCorrespondingObjectFromSource(PrefabUtility.GetOutermostPrefabInstanceRoot(Selection.activeGameObject)));
                    continue;
                }

                Add(item.objects, Selection.activeObject);
            }
        }

        private void OnValidate()
        {
            searchList = new List<UnityEngine.Object>();
            for (int i = selectionHistory.Count - 1; i >= 0; i--)
            {
                UnityEngine.Object obj = selectionHistory[i];
                if (obj.name.ToLower().Contains(search.ToLower()))
                {
                    Add(searchList, selectionHistory[i]);
                }
            }
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

        // void OnDisable()
        // {
        //     var data = JsonUtility.ToJson(this, false);
        //     EditorPrefs.SetString(WINDOW_KEY, data);
        // }
    }

    [Serializable, InlineEditor]
    public class ItemList
    {
        [HideLabel] public string listName;
        public List<UnityEngine.Object> objects;

        public ItemList(string name)
        {
            listName = name;
            objects = new List<UnityEngine.Object>();
        }

        public ItemList() { }
    }
}
