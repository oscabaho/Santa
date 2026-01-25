using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Santa.Core.Pooling
{
    public class PoolService : MonoBehaviour, IPoolService
    {
        private readonly Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>(32);
        private Transform _root;

        private void Awake()
        {
            // Use a child object as pool root to keep hierarchy clean
            var rootGo = new GameObject("[PoolRoot]");
            rootGo.transform.SetParent(transform);
            _root = rootGo.transform;
        }

        public async UniTask PrewarmAsync(string key, GameObject prefab, int count)
        {
            if (string.IsNullOrEmpty(key) || prefab == null || count <= 0) return;
            if (!_pools.TryGetValue(key, out var q))
            {
                q = new Queue<GameObject>(count);
                _pools[key] = q;
            }

            // Instantiate "count" instances and enqueue
            for (int i = 0; i < count; i++)
            {
                var go = Object.Instantiate(prefab, _root);
                go.SetActive(false);
                q.Enqueue(go);

                // Spread work across frames for mobile (avoid spikes)
                if ((i & 3) == 3) // every 4 creations yield
                {
                    await UniTask.Yield();
                }
            }
        }

        public GameObject Get(string key, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            Queue<GameObject> q;
            GameObject instance = null;

            if (!string.IsNullOrEmpty(key) && _pools.TryGetValue(key, out q) && q.Count > 0)
            {
                instance = q.Dequeue();
            }

            if (instance == null)
            {
                // Fallback: instantiate if pool empty or key not registered
                instance = Object.Instantiate(prefab, position, rotation, parent != null ? parent : _root);
            }
            else
            {
                var t = instance.transform;
                t.SetPositionAndRotation(position, rotation);
                t.SetParent(parent != null ? parent : _root, worldPositionStays: true);
                instance.SetActive(true);
            }

            return instance;
        }

        public void Return(string key, GameObject instance)
        {
            if (instance == null) return;
            if (string.IsNullOrEmpty(key))
            {
                // If no key, just disable and keep under root
                instance.SetActive(false);
                instance.transform.SetParent(_root, worldPositionStays: false);
                return;
            }

            if (!_pools.TryGetValue(key, out var q))
            {
                q = new Queue<GameObject>();
                _pools[key] = q;
            }

            instance.SetActive(false);
            instance.transform.SetParent(_root, worldPositionStays: false);
            q.Enqueue(instance);
        }
    }
}
