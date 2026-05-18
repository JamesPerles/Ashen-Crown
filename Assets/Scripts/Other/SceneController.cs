using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindGoal();
        FindRetryButton();
        FindWinButton();
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindGoal();
        FindRetryButton();
        FindWinButton();
    }
    void FindGoal()
    {
        GameObject goal = GameObject.FindGameObjectWithTag("Goal");
        if (goal == null) return;
        Collider2D col = goal.GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        if (goal.GetComponent<GoalTriggerListener>() == null)
            goal.AddComponent<GoalTriggerListener>();
    }
    void FindRetryButton()
    {
        GameObject buttonObj = GameObject.FindGameObjectWithTag("RetryButton");
        if (buttonObj == null) return;
        Button button = buttonObj.GetComponent<Button>();
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ReloadLastScene);
    }
    void FindWinButton()
    {
        GameObject buttonObj = GameObject.FindGameObjectWithTag("WinButton");
        if (buttonObj == null) return;
        Button button = buttonObj.GetComponent<Button>();
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(LoadFirstScene);
    }
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "GameOver" && Input.GetKeyDown(KeyCode.Space))
            ReloadLastScene();
    }
    public void GoToGameOver()
    {
        PlayerPrefs.SetInt("LastSceneIndex", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameOver");
    }
    public void LoadNextScene()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
    }
    public void ReloadLastScene()
    {
        int last = PlayerPrefs.GetInt("LastSceneIndex", 0);
        SceneManager.LoadScene(last);
    }
    public void LoadFirstScene()
    {
        SceneManager.LoadScene(0);
    }
    class GoalTriggerListener : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                SceneController.Instance.LoadNextScene();
        }
    }
}