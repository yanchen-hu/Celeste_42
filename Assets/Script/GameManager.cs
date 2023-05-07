using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int NumberCollectionGet;
    public int Count;
    public int DeathNum;
    private bool loadingNextScene;
    public GameObject Donuts;
    public GameObject Skl;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(Instance!= null && Instance!=this)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this);
        NumberCollectionGet = 0;
        Count = 0;
        DeathNum = 0;
        loadingNextScene = false;
        HandleUpdateCollectionUI();
    }

    public void HandlePlayerReachDestination()
    {
        Count++;
    }

    // Update is called once per frame
    void Update()
    {
        if(Count == 2&&!loadingNextScene)
        {
            LoadNextLevel(3.2f);
            loadingNextScene = true;
            Count = 0;
            //load next level
        }
    }

    public void HandleCollectItems()
    {
        NumberCollectionGet++;
        HandleUpdateCollectionUI();
    }

    public void HandleUpdateCollectionUI()
    {
        if (NumberCollectionGet != 0)
        {
            Donuts.SetActive(true);
            Donuts.GetComponentInChildren<Text>().text = "X " + NumberCollectionGet;
        }
        else
        {
            Donuts.SetActive(false);
        }
    }

    public void HandleUpdateDeathUI()
    {
        Skl.GetComponentInChildren<Text>().text = "X " + DeathNum;
    }

    public void ReloadScene(float sec)
    {
        StartCoroutine(WaitAndReloadScene(sec));
    }

    public void LoadNextLevel(float sec)
    {
        StartCoroutine(WaitAndLoadNextScene(sec));
    }

    IEnumerator WaitAndReloadScene(float sec)
    {
        yield return new WaitForSeconds(sec);
        Count = 0;
        DeathNum++;
        HandleUpdateDeathUI();
        loadingNextScene = false;
        NumberCollectionGet = 0;
        HandleUpdateCollectionUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator WaitAndLoadNextScene(float sec)
    {
        this.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(sec);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Count = 0;
        loadingNextScene = false;
    }

}
