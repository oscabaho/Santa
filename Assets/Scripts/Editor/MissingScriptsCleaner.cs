#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Santa.Editor
{
    /// <summary>
    /// Utility to remove Missing (MonoBehaviour) scripts from the active scene or selected assets.
    /// Helps after refactors where components were removed or moved.
    /// </summary>
    public static class MissingScriptsCleaner
    {
        [MenuItem("Santa/Clean Missing Scripts In Scene", false, 50)]
        public static void CleanScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("MissingScriptsCleaner: No valid active scene.");
                return;
            }

            Undo.SetCurrentGroupName("Clean Missing Scripts In Scene");
            int undoGroup = Undo.GetCurrentGroup();

            int removedCount = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                removedCount += RemoveMissingInHierarchy(root);
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"MissingScriptsCleaner: Removed {removedCount} missing scripts from scene '{scene.name}'.");
        }

        [MenuItem("Santa/Clean Missing Scripts On Selected Prefabs", false, 51)]
        public static void CleanSelectedPrefabs()
        {
            var objs = Selection.objects;
            if (objs == null || objs.Length == 0)
            {
                Debug.LogWarning("MissingScriptsCleaner: No assets selected.");
                return;
            }

            int removedTotal = 0;
            foreach (var obj in objs)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null)
                {
                    continue; // not a prefab
                }

                Undo.SetCurrentGroupName($"Clean Missing Scripts: {go.name}");
                int undoGroup = Undo.GetCurrentGroup();

                int removed = RemoveMissingInHierarchy(go);
                if (removed > 0)
                {
                    EditorUtility.SetDirty(go);
                    AssetDatabase.SaveAssets();
                }

                Undo.CollapseUndoOperations(undoGroup);
                Debug.Log($"MissingScriptsCleaner: Removed {removed} missing scripts from prefab '{go.name}'.");
                removedTotal += removed;
            }

            Debug.Log($"MissingScriptsCleaner: Completed. Total removed across selection: {removedTotal}.");
        }

        private static int RemoveMissingInHierarchy(GameObject root)
        {
            int count = 0;
            var stack = new Stack<Transform>();
            stack.Push(root.transform);
            while (stack.Count > 0)
            {
                var t = stack.Pop();
                int before = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                if (before > 0)
                {
                    Undo.RegisterCompleteObjectUndo(t.gameObject, "Remove Missing Scripts");
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                    count += before;
                }

                for (int i = 0; i < t.childCount; i++)
                {
                    stack.Push(t.GetChild(i));
                }
            }
            return count;
        }
    }
}
#endif
