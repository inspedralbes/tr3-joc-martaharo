// =================================================================================
// SCRIPT: EnemyNetworkSync
// UBICACIÓ: Assets/_Project/Scripts/Network/
// DESCRIPCIÓ: Sincronització de l'enemic des del servidor amb interpolació
// =================================================================================

using UnityEngine;
using SocketIOClient;
using System.Collections;

public class EnemyNetworkSync : MonoBehaviour
{
    // URL del servidor Node.js
    private string urlServidor = "http://localhost:3000";

    [Header("Configuració")]
    public float lerpSpeed = 5f;
    public bool isLocalPlayer = false;

    // Variables per a la interpolació
    private Vector3 targetPosition;
    private Vector3 lastReceivedPosition;
    private float networkLatency = 0.1f;

    // Connexió Socket.io
    private SocketIO client;
    private string currentRoomId;

    void Start()
    {
        // Obtenir l'ID de la sala
        currentRoomId = LobbyManager.roomId;

        if (string.IsNullOrEmpty(currentRoomId))
        {
            currentRoomId = MainMenuManager.roomId;
        }

        // Iniciar connexió Socket.io
        StartCoroutine(ConnectarSocket());
    }

    IEnumerator ConnectarSocket()
    {
        client = new SocketIO(urlServidor);

        // Escoltar actualitzacions de l'enemic des del servidor
        client.On("enemyMoved", (data) =>
        {
            var posData = JsonUtility.FromJson<EnemyPositionData>(data.ToString());
            lastReceivedPosition = new Vector3(posData.x, posData.y, 0);
        });

        yield return client.ConnectAsync();

        if (client.Connected && !string.IsNullOrEmpty(currentRoomId))
        {
            // Unir-se a la sala
            client.EmitAsync("joinRoom", new
            {
                roomId = currentRoomId,
                playerId = "enemy_" + Random.Range(1000, 9999),
                playerName = "Enemy"
            });
        }
    }

    void Update()
    {
        // Interpolació suaument cap a la posició rebuda del servidor
        if (lastReceivedPosition != Vector3.zero)
        {
            transform.position = Vector3.Lerp(transform.position, lastReceivedPosition, Time.deltaTime * lerpSpeed);
        }
    }

    // Mètode per rebre posició de l'enemic (cridat des del servidor)
    public void OnEnemyMoved(float x, float y)
    {
        lastReceivedPosition = new Vector3(x, y, 0);
    }

    void OnDestroy()
    {
        if (client != null)
        {
            client.DisconnectAsync();
        }
    }

    [System.Serializable]
    public class EnemyPositionData
    {
        public float x;
        public float y;
    }
}
