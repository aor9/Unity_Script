using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

// Object Pooling을 수행하는 클래스
public class PoolManager
{
    class Pool
    {
        public GameObject Original { get; private set; }
        public Transform Root { get; set; }

        private Stack<Poolable> poolStack = new Stack<Poolable>();

        public void Init(GameObject original, int count = 5)
        {
            Original = original;
            Root = new GameObject().transform;
            Root.name = $"{original.name}_Root";

            if (original.GetComponent<Poolable>().isMercenary is true)
            {
                count = 2;
            }
            
            for (int i = 0; i < count; i++)
            {
                Push(Create());
            }
        }

        Poolable Create()
        {
            GameObject go = Object.Instantiate<GameObject>(Original);
            go.name = Original.name;
            return go.GetOrAddComponent<Poolable>();
        }

        public void Push(Poolable poolable)
        {
            if (poolable == null) return;

            poolable.transform.parent = Root;
            poolable.gameObject.SetActive(false);
            poolable.isUsing = false;
            
            poolStack.Push(poolable);
        }

        public Poolable Pop(Transform parent)
        {
            Poolable poolable;
            if (poolStack.Count > 0)
            {
                poolable = poolStack.Pop();
            }
            else
            {
                poolable = Create();
            }
            
            poolable.gameObject.SetActive(true);

            //DontDestroyOnLoad 해제 용도
            if (parent == null)
            {
                poolable.transform.parent = Managers.Scene.CurrentScene.transform;
            }
            
            poolable.transform.parent = parent;
            poolable.isUsing = true;

            return poolable;
        }
    }

    private Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();
    private Transform root;
    
    //초기화
    public void Init()
    {
        if (root == null)
        {
            root = new GameObject { name = "@Pool_Root" }.transform;
            Object.DontDestroyOnLoad(root);
        }
    }

    public void CreatePool(GameObject original, int count = 5)
    {
        Pool pool = new Pool();
        pool.Init(original, count);
        pool.Root.parent = root;
        
        _pool.Add(original.name, pool);
    }

    public void Push(Poolable poolable)
    {
        string name = poolable.gameObject.name;
        if (_pool.ContainsKey(name) == false)
        {
            GameObject.Destroy(poolable.gameObject);
            return;
        }
        
        _pool[name].Push(poolable);
    }

    public Poolable Pop(GameObject original, Transform parent = null)
    {
        if (_pool.ContainsKey(original.name) == false)
        {
            CreatePool(original);
        }
        return _pool[original.name].Pop(parent);
    }

    public GameObject GetOriginal(string name)
    {
        if (_pool.ContainsKey(name) == false)
        {
            return null;
        }
        return _pool[name].Original;
    }

    public void Clear()
    {
        foreach(Transform child in root)
            GameObject.Destroy(child.gameObject);

        _pool.Clear();
    }
    
}
