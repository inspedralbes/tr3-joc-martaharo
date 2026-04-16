using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;

public class GameManagerIA : MonoBehaviour
{
    public static GameManagerIA Instance { get; private set; }

    private bool gameFinished = false;
    private bool iaGameStarted = false;

    [Header("Panells UI")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject waitingPanel;

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

    public void Victory()
    {
        if (gameFinished) return;
        gameFinished = true;

        Debug.Log("Victoria! Has guanyat!");
        
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
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
}