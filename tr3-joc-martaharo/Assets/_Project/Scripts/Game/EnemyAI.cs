using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocketIOClient;

public class EnemyAI : MonoBehaviour
{
    [Header("Configuració de moviment")]
    public float speed = 3f;
    public float chaseRadius = 10f;
    public bool seguirJugadorMesProper = true;

    [Header("Punt d'inici (Respawn)")]
    public Transform puntInici;

    private Rigidbody2D rb;
    private Transform targetPlayer;
    private string roomId;
    private SocketIO client;
    private bool isMultiplayer;
    private bool isHost = false;
    private GameManager gameManager;
    private Vector2 serverPosition;
    private bool positionFromServer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        roomId = LobbyManager.roomId;
        if (string.IsNullOrEmpty(roomId))
            roomId = MainMenuManager.roomId;

        isMultiplayer = !MainMenuManager.isSinglePlayer;
        gameManager = Object.FindFirstObjectByType<GameManager>();

        // Buscar automàticament el puntInici si no està assignat
        if (puntInici == null)
        {
            GameObject puntIniciObj = GameObject.Find("PuntInici");
            if (puntIniciObj != null)
            {
                puntInici = puntIniciObj.transform;
            }
        }

        StartCoroutine(ConnectSocket());
    }

    IEnumerator ConnectSocket()
    {
        client = new SocketIO("http://localhost:3000");

        // Rebre posició de l'enemic des del servidor (multijugador)
        client.On("enemyMovedFromServer", (data) =>
        {
            var posData = JsonUtility.FromJson<EnemyServerPosition>(data.ToString());
            serverPosition = new Vector2(posData.x, posData.y);
            positionFromServer = true;
        });

        // Rebre ordre de fer respawn des del servidor
        client.On("doRespawn", (data) =>
        {
            var respawnData = JsonUtility.FromJson<RespawnData>(data.ToString());
            FerRespawn();
        });

        yield return client.ConnectAsync();

        if (client.Connected)
        {
            // Unir-se a la sala com a observador de l'enemic
            client.EmitAsync("registerEnemy", new
            {
                roomId = roomId
            });

            // En multiplayer, el primer jugador és l'"host" que calcula l'IA
            if (isMultiplayer)
            {
                isHost = true;
            }
        }
    }

    void Update()
    {
        // En mode individual: IA local
        // En mode multijugador: només l'host calcula la IA
        if (!isMultiplayer || (isMultiplayer && isHost))
        {
            CalcularIAPersonal();
        }
    }

    void CalcularIAPersonal()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform nearestPlayer = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject player in players)
        {
            if (player.activeInHierarchy)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearestPlayer = player.transform;
                }
            }
        }

        if (nearestPlayer != null && nearestDistance <= chaseRadius)
        {
            targetPlayer = nearestPlayer;
        }
        else
        {
            targetPlayer = null;
        }
    }

    void FixedUpdate()
    {
        // En multijugador: rebre posició del servidor
        if (isMultiplayer && !isHost && positionFromServer)
        {
            transform.position = Vector2.Lerp(transform.position, serverPosition, Time.fixedDeltaTime * 5f);
            return;
        }

        // L'host mou l'enemic localment
        if (targetPlayer != null)
        {
            Vector2 direction = (targetPlayer.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;

            // Enviar posició al servidor per sincronitzar
            if (client != null && client.Connected && isHost)
            {
                client.EmitAsync("enemyMoved", new
                {
                    roomId = roomId,
                    x = transform.position.x,
                    y = transform.position.y
                });
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                if (isMultiplayer)
                {
                    // Notificar al servidor que hem atrapat un jugador
                    if (client != null && client.Connected)
                    {
                        client.EmitAsync("playerCaught", new
                        {
                            roomId = roomId,
                            playerId = player.playerId
                        });
                    }

                    // Fer respawn del jugador atrapat
                    StartCoroutine(RespawnPlayer(collision.gameObject));
                }
                else
                {
                    if (gameManager != null)
                    {
                        gameManager.GameOver();
                    }
                }
            }
        }
    }

    IEnumerator RespawnPlayer(GameObject player)
    {
        player.SetActive(false);
        yield return new WaitForSeconds(1f);

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.Respawn();
        }
        else if (puntInici != null)
        {
            player.transform.position = puntInici.position;
        }
        else
        {
            player.transform.position = Vector3.zero;
        }

        player.SetActive(true);
        Debug.Log("Jugador tornant al punt d'inici (respawn)");
    }

    public void FerRespawn()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.Respawn();
            }
        }
    }

    [System.Serializable]
    public class EnemyServerPosition
    {
        public float x;
        public float y;
    }

    [System.Serializable]
    public class RespawnData
    {
        public string playerId;
    }
}
