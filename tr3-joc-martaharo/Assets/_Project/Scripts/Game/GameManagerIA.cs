using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManagerIA : MonoBehaviour
{
    public static GameManagerIA Instance { get; private set; }

    private bool gameFinished = false;
    private bool iaGameStarted = false;

    [Header("Panells UI")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;
    public GameObject waitingPanel;
    public GameObject resultPanel;

    [Header("UI Texte")]
    public TextMeshProUGUI winnerText;

    [Header("Personatges")]
    public GameObject player;
    public GameObject enemy;
    public BirdAgentIA birdAgent;
    public Transform puntInici;

    private Vector3 playerSpawnPos;
    private Vector3 enemySpawnPos;
    private Vector3 birdSpawnPos;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (puntInici != null)
        {
            playerSpawnPos = puntInici.position + Vector3.left * 1.5f;
            birdSpawnPos = puntInici.position;
            enemySpawnPos = puntInici.position + Vector3.right * 2f;
        }
        else
        {
            if (player != null) playerSpawnPos = player.transform.position;
            if (enemy != null) enemySpawnPos = enemy.transform.position;
            if (birdAgent != null) birdSpawnPos = birdAgent.transform.position;
        }

        ReiniciarPosicions();

        if (waitingPanel != null)
            waitingPanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        StartCoroutine(StartGameAfterDelay());
    }

    public void ReiniciarPosicions()
    {
        if (player != null)
        {
            player.transform.position = playerSpawnPos;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        if (enemy != null)
        {
            EnemyLocal el = enemy.GetComponent<EnemyLocal>();
            if (el != null) el.ResetPosition();
            else
            {
                enemy.transform.position = enemySpawnPos;
                Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }

        if (birdAgent != null)
        {
            birdAgent.transform.position = birdSpawnPos;
            birdAgent.Respawn();
        }

        gameFinished = false;
    }

    public void SpawnBird()
    {
        if (birdAgent != null)
        {
            birdAgent.transform.position = birdSpawnPos;
            birdAgent.Respawn();
        }
    }

    IEnumerator StartGameAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        iaGameStarted = true;
    }

    public void Victory(string winner = "Jugador")
    {
        if (gameFinished) return;
        gameFinished = true;

        Debug.Log("Victoria! Ha guanyat: " + winner);
        
        Time.timeScale = 0f;
        CrearCanvasVictòria(winner);
    }

    private void CrearCanvasVictòria(string winner)
    {
        GameObject canvasObj = new GameObject("Canvas_Victoria");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject bgPanel = new GameObject("Panel_Fons");
        bgPanel.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgPanel.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.85f);

        RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("Text_Guanyador");
        textObj.transform.SetParent(bgPanel.transform, false);
        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = winner == "IA" ? "HA GUANYAT LA IA!" : "HAS GUANYAT!";
        tmpText.fontSize = 72;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.6f);
        textRect.anchorMax = new Vector2(1f, 0.8f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        GameObject btnJugar = CrearBoto(bgPanel.transform, "Btn_TornarJugar", "Tornar a jugar", 200f, -100f);
        btnJugar.GetComponent<Button>().onClick.AddListener(TornarJugar);

        GameObject btnSortir = CrearBoto(bgPanel.transform, "Btn_Sortir", "Sortir", 200f, -200f);
        btnSortir.GetComponent<Button>().onClick.AddListener(SortirDelJoc);
    }

    private GameObject CrearBoto(Transform parent, string nom, string text, float ample, float yPos)
    {
        GameObject btnObj = new GameObject(nom);
        btnObj.transform.SetParent(parent, false);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = Color.green;
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0f, 0.8f, 0f);
        colors.highlightedColor = new Color(0f, 1f, 0f);
        colors.pressedColor = new Color(0f, 0.6f, 0f);
        btn.colors = colors;

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(ample, 60f);
        btnRect.anchoredPosition = new Vector2(0f, yPos);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = 36;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.black;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return btnObj;
    }

    public void GameOver()
    {
        if (gameFinished) return;
        gameFinished = true;

        Debug.Log("Game Over!");
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void DamagePlayerAndEnemy()
    {
        if (player != null)
        {
            SpriteRenderer playerSr = player.GetComponent<SpriteRenderer>();
            if (playerSr != null)
            {
                playerSr.color = Color.red;
                Invoke(nameof(ResetPlayerColor), 0.2f);
            }
            RespawnPlayer();
        }

        if (enemy != null && birdAgent != null)
        {
            SpriteRenderer enemySr = enemy.GetComponent<SpriteRenderer>();
            if (enemySr != null)
            {
                enemySr.color = Color.red;
                Invoke(nameof(ResetEnemyColor), 0.2f);
            }
            birdAgent.Respawn();
        }
    }

    private void ResetPlayerColor()
    {
        if (player != null)
        {
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.white;
        }
    }

    private void ResetEnemyColor()
    {
        if (enemy != null)
        {
            SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.white;
        }
    }

    private void RespawnPlayer()
    {
        if (player != null)
        {
            player.transform.position = playerSpawnPos;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void TornarMenu()
    {
        MainMenuManager.isSinglePlayer = false;
        MainMenuManager.roomId = "";
        MainMenuManager.roomCode = "";
        SceneManager.LoadScene("Menu");
    }

    public void TornarJugar()
    {
        SceneManager.LoadScene("Joc_IA");
    }

    public bool IsGameStarted()
    {
        return iaGameStarted;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SortirDelJoc();
        }
    }

    public void SortirDelJoc()
    {
        Debug.Log("Sortint del joc...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}