using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using SocketIOClient;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private SocketIO client;
    private string roomId;
    private bool isMultiplayer;
    private bool gameFinished = false;

    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject waitingPanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        roomId = LobbyManager.roomId;
        if (string.IsNullOrEmpty(roomId))
            roomId = MainMenuManager.roomId;

        isMultiplayer = !MainMenuManager.isSinglePlayer;

        if (isMultiplayer)
        {
            StartCoroutine(ConnectSocket());
        }

        if (waitingPanel != null)
            waitingPanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    IEnumerator ConnectSocket()
    {
        client = new SocketIO("http://localhost:3000");

        client.On("gameFinished", (data) =>
        {
            GameFinishedResponse response = JsonUtility.FromJson<GameFinishedResponse>(data.ToString());
            if (response.winnerId == AuthManager.nomUsuari)
            {
                Victory();
            }
            else
            {
                Defeat();
            }
        });

        client.On("playerWon", (data) =>
        {
            PlayerWinResponse response = JsonUtility.FromJson<PlayerWinResponse>(data.ToString());
            if (response.winnerName == AuthManager.nomUsuari)
            {
                Victory();
            }
            else
            {
                Defeat();
            }
        });

        yield return client.ConnectAsync();

        if (client.Connected)
        {
            client.EmitAsync("joinRoom", new
            {
                roomId = roomId,
                playerId = "gameManager",
                playerName = "GameManager"
            });
        }
    }

    public void Victory()
    {
        if (gameFinished) return;
        gameFinished = true;

        Debug.Log("Has guanyat!");
        
        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (client != null && client.Connected)
        {
            client.EmitAsync("gameFinished", new
            {
                roomId = roomId,
                winnerId = AuthManager.nomUsuari,
                winnerName = AuthManager.nomUsuari
            });
        }
    }

    public void GameOver()
    {
        if (gameFinished) return;
        gameFinished = true;

        Debug.Log("Game Over!");
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (client != null && client.Connected)
        {
            client.EmitAsync("gameFinished", new
            {
                roomId = roomId,
                winnerId = "enemy",
                winnerName = "Enemy"
            });
        }
    }

    public void Defeat()
    {
        if (gameFinished) return;
        gameFinished = true;

        Debug.Log("Has perdut!");
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void TornarMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void TornarJugar()
    {
        SceneManager.LoadScene("Joc");
    }

    [System.Serializable]
    public class GameFinishedResponse
    {
        public string winnerId;
        public string winnerName;
    }

    [System.Serializable]
    public class PlayerWinResponse
    {
        public string winnerName;
    }
}
