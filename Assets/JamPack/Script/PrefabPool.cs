using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPJamPack {
    public interface IPoolableObj {
        void Instantiate();
        void DeactivateObj(Transform collectionTransform);
        void Reinstantiate();
    }

    public class PrefabPool<T> where T: MonoBehaviour, IPoolableObj
    {
        public T Prefab;

        public delegate T PrefabInstantiateFunc();
        public PrefabInstantiateFunc InstantiateFunc;

        public List<T> AliveObjs, PoolObjs;
        public Transform PoolCollection;

        public PrefabPool(T prefab, bool createPoolCollection=false, string poolCollectionName="Pool Collection")
        {
            Prefab = prefab;
            InstantiateFunc = null;
            AliveObjs = new List<T>();
            PoolObjs = new List<T>();

            if (createPoolCollection)
            {
                GameObject obj = new GameObject(poolCollectionName);
                PoolCollection = obj.transform;
            }
        }

        public PrefabPool(PrefabInstantiateFunc instantiateFunc, bool createPoolCollection=false, string poolCollectionName="Pool Collection")
        {
            Prefab = null;
            InstantiateFunc = instantiateFunc;
            AliveObjs = new List<T>();
            PoolObjs = new List<T>();

            if (createPoolCollection)
            {
                GameObject obj = new GameObject(poolCollectionName);
                PoolCollection = obj.transform;
            }
        }

        public void ClearAliveObjs()
        {
            while (AliveObjs.Count > 0)
            {
                GameObject.Destroy(AliveObjs[0]);
                AliveObjs.RemoveAt(0);
            }
        }

        public void ClearPoolObjs()
        {
            while (PoolObjs.Count > 0)
            {
                GameObject.Destroy(PoolObjs[0]);
                PoolObjs.RemoveAt(0);
            }
        }

        public T Get()
        {
            T component;
            if (PoolObjs.Count > 0) {
                component = PoolObjs[0];
                PoolObjs.RemoveAt(0);

                component.Reinstantiate();
            }
            else {
                if (Prefab == null)
                    component = InstantiateFunc.Invoke();
                else
                    component = GameObject.Instantiate(Prefab);

                component.Instantiate();
            }

            AliveObjs.Add(component);

            return component;
        }

        public void Put(T component)
        {
            component.DeactivateObj(PoolCollection);
            PoolObjs.Add(component);
        }
    }
}