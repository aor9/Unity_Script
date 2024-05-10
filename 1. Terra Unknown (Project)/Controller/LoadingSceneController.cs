using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = System.Random;

public class LoadingSceneController : MonoBehaviour
{
    protected static LoadingSceneController instance;
    public static LoadingSceneController Instance
    {
        get
        {
            if(instance == null)
            {
                var obj = FindObjectOfType<LoadingSceneController>();
                if(obj != null)
                {
                    instance = obj;
                }
                else
                {
                    instance = Instantiate(Managers.Resource.Load<LoadingSceneController>("Prefabs/UI/LoadingUI"));
                }
            }
            return instance;
        }
    }
    
    [SerializeField] 
    private CanvasGroup canvasGroup;
    [SerializeField]
    private Image progressBar;
    private string loadSceneName;
    private GameObject mapGameObject;
    
    private Tilemap tilemap;
    private List<Tile> floorTiles;

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        
    }

    public void LoadScene(string sceneName)
    { 
        gameObject.SetActive(true);
        Debug.Log(gameObject.name);
        SceneManager.sceneLoaded += OnSceneLoaded;
        loadSceneName = sceneName;
        
        GameObject overlayContainer = Managers.Resource.Instantiate("object/OverlayContainer");
        DontDestroyOnLoad(overlayContainer);
        
        StartCoroutine(LoadSceneProcess());
    }

    private IEnumerator LoadSceneProcess()
    {
        progressBar.fillAmount = 0f;
        yield return StartCoroutine(Fade(true));

        AsyncOperation op = SceneManager.LoadSceneAsync(loadSceneName);
        op.allowSceneActivation = false;

        DungeonInit();

        float timer = 0f;
        float fakeLoadingTime = 1.0f;
        while (!op.isDone)
        {
            yield return null;
            if (op.progress < 0.7f)
            {
                progressBar.fillAmount = op.progress;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                progressBar.fillAmount = Mathf.Lerp(0.7f, 1f, timer / fakeLoadingTime);
                if (progressBar.fillAmount >= 1.0f)
                {
                    op.allowSceneActivation = true;
                    mapGameObject.GetComponentInChildren<TilemapRenderer>().material.color = new Color(1, 1, 1, 1);
                    yield break;
                }
            }
        }
    }

    private void DungeonInit()
    {
        mapGameObject = Managers.Resource.Instantiate("object/Map");
        DontDestroyOnLoad(mapGameObject);
        mapGameObject.GetComponentInChildren<TilemapRenderer>().material.color = new Color(1, 1, 1, 0);
        
        // 맵의 바닥을 만드는 코드
        tilemap = mapGameObject.GetComponentInChildren<Tilemap>();
        
        floorTiles = new List<Tile>();
        for (int i = 1; i < 5; i++)
        {
            floorTiles.Add(Managers.Resource.Load<Tile>($"Img/map4/cave{i}"));
        }

        for (int i = 1; i < 5; i++)
        {
            floorTiles.Add(Managers.Resource.Load<Tile>($"Img/map4/dark_cave{i}"));
        }

        int pattern = 0;
        
        for (int i = -8; i < 7; i++)
        {
            for (int j = -8; j < 7; j++)
            {
                int random = UnityEngine.Random.Range(1, 10);
                Tile tile;
                
                if (random <= 4)
                {
                    if ((j + pattern) % 2 == 0)
                    {
                        tile = floorTiles[0];
                    }
                    else
                    {
                        tile = floorTiles[4];
                    }
                } 
                else if (random <= 5)
                {
                    if ((j + pattern) % 2 == 0)
                    {
                        tile = floorTiles[1];
                    }
                    else
                    {
                        tile = floorTiles[5];
                    }
                } 
                else if (random <= 8)
                {
                    if ((j + pattern) % 2 == 0)
                    {
                        tile = floorTiles[2];
                    }
                    else
                    {
                        tile = floorTiles[6];
                    }
                }
                else 
                {
                    if ((j + pattern) % 2 == 0)
                    {
                        tile = floorTiles[3];
                    }
                    else
                    {
                        tile = floorTiles[7];
                    }
                }
                
                tilemap.SetTile(new Vector3Int(i, j, 0), tile);
            }
            pattern++;
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name == loadSceneName)
        {
            StartCoroutine(Fade(false));
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    private IEnumerator Fade(bool isFadeIn) 
    {
        float timer = 0f;

        while(timer <= 1f)
        {
            yield return null;
            timer += Time.unscaledDeltaTime * 2f;
            canvasGroup.alpha = Mathf.Lerp(isFadeIn ? 0 : 1, isFadeIn ? 1 : 0, timer);
        }

        if(!isFadeIn)
        {
            gameObject.SetActive(false);
        }
    }
}
