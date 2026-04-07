using UnityEngine;
using SocketIOClient;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    private SocketIO client;
    private string serverUrl = "http://localhost:3000";
    
    private string currentRoomId;
    private string playerId;
    private bool isConnected = false;

    public System.Action<Vector2> OnPlayerMoved;
    public System.Action<string> OnPlayerDisconnected;
    public System.Action<Dictionary<string, Vector2>> OnPositionsSynced;
    public System.Action OnOpponentJoined;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(string roomId)
    {
        currentRoomId = roomId;
        playerId = "player_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        StartCoroutine(Connect());
    }

    IEnumerator Connect()
    {
        client = new SocketIO(serverUrl);

        client.On("playerMoved", (data) =>
        {
            var response = JsonUtility.FromJson<PlayerMoveData>(data.ToString());
            if (response.playerId != playerId)
            {
                OnPlayerMoved?.Invoke(new Vector2(response.x, response.y));
            }
        });

        client.On("syncPositions", (data) =>
        {
            var response = JsonUtility.FromJson<PositionsSyncData>(data.ToString());
            if (response.positions != null)
            {
                Dictionary<string, Vector2> converted = new Dictionary<string, Vector2>();
                foreach (var kvp in response.positions)
                {
                    converted[kvp.Key] = kvp.Value.ToVector2();
                }
                OnPositionsSynced?.Invoke(converted);
            }
        });

        client.On("jugadorDesconnectat", (data) =>
        {
            var response = JsonUtility.FromJson<PlayerDisconnectData>(data.ToString());
            OnPlayerDisconnected?.Invoke(response.playerId);
        });

        client.On("jugadorEntrat", (data) =>
        {
            OnOpponentJoined?.Invoke();
        });

        client.On("gameFinished", (data) =>
        {
            var response = JsonUtility.FromJson<GameFinishedData>(data.ToString());
            if (GameManager.Instance != null)
            {
                if (response.winnerName == AuthManager.username)
                {
                    GameManager.Instance.Victory();
                }
                else
                {
                    GameManager.Instance.Defeat();
                }
            }
        });

        yield return client.ConnectAsync();

        if (client.Connected)
        {
            isConnected = true;
            Debug.Log("NetworkManager: Connectat a Socket.io");

            client.EmitAsync("joinRoom", new
            {
                roomId = currentRoomId,
                playerId = playerId,
                playerName = AuthManager.username ?? "Unknown"
            });
        }
        else
        {
            Debug.LogError("NetworkManager: Error connectant a Socket.io");
        }
    }

    public void UpdatePosition(Vector2 position)
    {
        if (isConnected && client != null && client.Connected)
        {
            client.EmitAsync("updatePosition", new
            {
                roomId = currentRoomId,
                playerId = playerId,
                x = position.x,
                y = position.y
            });
        }
    }

    public void SendPlayerWon()
    {
        if (isConnected && client != null && client.Connected)
        {
            client.EmitAsync("gameFinished", new
            {
                roomId = currentRoomId,
                winnerId = playerId,
                winnerName = AuthManager.username
            });
        }
    }

    public void SendPlayerCaught()
    {
        if (isConnected && client != null && client.Connected)
        {
            client.EmitAsync("playerCaught", new
            {
                roomId = currentRoomId,
                playerId = playerId
            });
        }
    }

    public bool IsConnected() => isConnected;

    void OnDestroy()
    {
        if (client != null && client.Connected)
        {
            client.EmitAsync("jugadorDesconnectat", new { playerId = playerId });
            client.DisconnectAsync();
        }
    }

    [System.Serializable]
    public class PlayerMoveData
    {
        public string playerId;
        public float x;
        public float y;
    }

    [System.Serializable]
    public class PositionsSyncData
    {
        public Dictionary<string, PositionData> positions;
    }

    [System.Serializable]
    public class PositionData
    {
        public float x;
        public float y;

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }

    [System.Serializable]
    public class PlayerDisconnectData
    {
        public string playerId;
    }

    [System.Serializable]
    public class GameFinishedData
    {
        public string winnerId;
        public string winnerName;
    }
}
