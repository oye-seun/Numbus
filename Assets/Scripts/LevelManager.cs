using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen; // Reference to your loading screen UI
    [SerializeField] private UnityEngine.UI.Slider progressBar; // Optional loading bar
    
    private static LevelManager instance;
    
    public static LevelManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        // Singleton pattern to maintain one instance
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Load level by build index
    public void LoadLevel(int levelIndex)
    {
        StartCoroutine(LoadLevelAsync(levelIndex));
    }

    // Load level by scene name
    public void LoadLevel(string levelName)
    {
        StartCoroutine(LoadLevelAsync(levelName));
    }

    // Load next level
    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        // Check if next level exists
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            LoadLevel(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels available!");
            // Optionally return to main menu or handle end game
        }
    }

    private IEnumerator LoadLevelAsync(string levelName)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(levelName);
        
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (progressBar != null)
                progressBar.value = progress;
            
            yield return null;
        }

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    private IEnumerator LoadLevelAsync(int levelIndex)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(levelIndex);
        
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (progressBar != null)
                progressBar.value = progress;
            
            yield return null;
        }

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    // Get current level index
    public int GetCurrentLevelIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    // Get current level name
    public string GetCurrentLevelName()
    {
        return SceneManager.GetActiveScene().name;
    }
}
