using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class ProceduralStructureCache {
        Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        public bool ContainsKey(string key) {
            return prefabs.ContainsKey(key);
        }

        public GameObject GetGameObject(string key) {
            if (ContainsKey(key)) {
                return prefabs[key];
            } else {
                return null;
            }
        }

        public GameObject InstantiateGameObject(string key, GameObject parent, string name) {
            GameObject prefab = GetGameObject(key);
            if (prefab == null) {
                return null;
            }
            #if UNITY_EDITOR
            GameObject instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            #else
            GameObject instance = GameObject.Instantiate(prefab);
            #endif
            instance.name = name;
            instance.transform.parent = parent.transform;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        public void AddPrefab(string key, GameObject gameObject) {
            if (key == null)
                throw new System.ArgumentNullException("key must not be null");
            if (gameObject == null)
                throw new System.ArgumentNullException("gameObject must not be null");
            #if UNITY_EDITOR
                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(string.Format("{0}/{1}", Application.dataPath, "_ProceduralStructures"));
                dirInfo.Create();
    
                MeshFilter[] meshfilters = gameObject.GetComponentsInChildren<MeshFilter>();
                int i = 0;
                foreach (MeshFilter mf in meshfilters) {
                    i++;
                    if (!UnityEditor.AssetDatabase.Contains(mf.sharedMesh)) {
                        UnityEditor.AssetDatabase.CreateAsset(mf.sharedMesh, "Assets/_ProceduralStructures/" + key + "-mesh" + i);
                    }
                }
                prefabs[key] = UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, "Assets/_ProceduralStructures/" + key + ".prefab", UnityEditor.InteractionMode.AutomatedAction);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            #else
                prefabs[key] = gameObject;
            #endif
        }
    }
}
