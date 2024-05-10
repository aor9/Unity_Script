using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// 싱글톤으로 구현된 클래스, 전체 Manager 코드들을 연결시켜준다.
public class Managers : MonoBehaviour
{
    private static Managers s_instance;
    public static Managers Instance
    {
        get { Init(); return s_instance; }
    }

    private PoolManager _pool = new PoolManager();
    private ResourceManager _resource = new ResourceManager();
    private SceneManagerEx _scene = new SceneManagerEx();
    private DataManager _data = new DataManager();
    private SoundManager _sound = new SoundManager();
    private UIManager _ui = new UIManager();
    
    public static PoolManager Pool
    {
        get { return Instance._pool; }
    }
    public static ResourceManager Resource
    {
        get { return Instance._resource; }
    }

    public static SceneManagerEx Scene
    {
        get { return Instance._scene; }
    }

    public static DataManager Data
    {
        get { return Instance._data; }
    }

    public static SoundManager Sound
    {
        get { return Instance._sound; }
    }
    
    public static UIManager UI
    {
        get { return Instance._ui; }
    }
    
    public Define.Scene previousSceneType;

    void Start()
    {
        Init();
    }
    
    void Update()
    {
        
    }

    static void Init()
    {
        GameObject go = GameObject.Find("@Managers");
        
        if (s_instance == null)
        {
            go = new GameObject { name = "@Managers" };
            go.AddComponent<Managers>();
        }

        DontDestroyOnLoad(go);
        s_instance = go.GetComponent<Managers>();
        
        s_instance._pool.Init();
    }

    public static void Clear()
    {
        Pool.Clear();
    }
}
