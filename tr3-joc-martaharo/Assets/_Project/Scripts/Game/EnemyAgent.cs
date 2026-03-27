using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;
using SocketIOClient;

public class EnemyAgent : Agent
{
    [Header("Configuració IA")]
    public float moveSpeed = 3f;
    public float chaseRadius = 10f;
    public Transform puntInici;
    public bool useMLAgents = true;

    [Header("Components")]
    private Rigidbody2D rb;
    private Transform targetPlayer;
    private string roomId;
    private SocketIO client;
    private bool isMultiplayer;
    private bool isHost = false;
    private GameManager gameManager;
    private Vector2 serverPosition;
    private bool positionFromServer = false;

    private bool mlAgentsAvailable = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        roomId = LobbyManager.roomId;
        if (string.IsNullOrEmpty(roomId))
            roomId = MainMenuManager.roomId;

        isMultiplayer = !MainMenuManager.isSinglePlayer;
        gameManager = Object.FindFirstObjectByType<GameManager>();

        if (puntInici == null)
        {
            GameObject puntIniciObj = GameObject.Find("PuntInici");
            if (puntIniciObj != null)
                puntInici = puntIniciObj.transform;
        }

        TryConnectMLAgents();
        
        if (!mlAgentsAvailable)
        {
            Debug.LogWarning("[EnemyAgent] ML-Agents no disponible. Usant IA tradicional.");
            StartCoroutine(ConnectSocket());
        }
    }

    void TryConnectMLAgents()
    {
        try
        {
            if (Academy.IsInitialized)
            {
                mlAgentsAvailable = true;
                Debug.Log("[EnemyAgent] ML-Agents inicialitzat correctament!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[EnemyAgent] ML-Agents no disponible: {e.Message}");
            mlAgentsAvailable = false;
        }
    }

    IEnumerator ConnectSocket()
    {
        client = new SocketIO("http://localhost:3000");

        client.On("enemyMovedFromServer", (data) =>
        {
            var posData = JsonUtility.FromJson<EnemyServerPosition>(data.ToString());
            serverPosition = new Vector2(posData.x, posData.y);
            positionFromServer = true;
        });

        client.On("doRespawn", (data) =>
        {
            FerRespawn();
        });

        yield return client.ConnectAsync();

        if (client.Connected)
        {
            client.EmitAsync("registerEnemy", new { roomId = roomId });

            if (isMultiplayer)
                isHost = true;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (targetPlayer != null)
        {
            sensor.AddObservation(transform.position);
            sensor.AddObservation(targetPlayer.position);
            
            float distance = Vector2.Distance(transform.position, targetPlayer.position);
            sensor.AddObservation(distance);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!mlAgentsAvailable || !useMLAgents)
            return;

        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        Vector2 direction = new Vector2(moveX, moveY).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (!mlAgentsAvailable || !useMLAgents)
            return;

        var continuousActionsOut = actionsOut.ContinuousActions;
        
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (GameObject p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = p.transform;
            }
        }

        if (nearest != null)
        {
            Vector2 direction = (nearest.position - transform.position).normalized;
            continuousActionsOut[0] = direction.x;
            continuousActionsOut[1] = direction.y;
        }
    }

    void Update()
    {
        if (!mlAgentsAvailable || !useMLAgents)
        {
            CalcularIATradicional();
        }
    }

    void CalcularIATradicional()
    {
        if (!isMultiplayer || (isMultiplayer && isHost))
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
    }

    void FixedUpdate()
    {
        if (!mlAgentsAvailable || !useMLAgents)
        {
            if (isMultiplayer && !isHost && positionFromServer)
            {
                transform.position = Vector2.Lerp(transform.position, serverPosition, Time.fixedDeltaTime * 5f);
                return;
            }

            if (targetPlayer != null)
            {
                Vector2 direction = (targetPlayer.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;

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
                    if (client != null && client.Connected)
                    {
                        client.EmitAsync("playerCaught", new
                        {
                            roomId = roomId,
                            playerId = player.playerId
                        });
                    }
                    StartCoroutine(RespawnPlayer(collision.gameObject));
                }
                else
                {
                    if (gameManager != null)
                        gameManager.GameOver();

                    if (mlAgentsAvailable)
                        EndEpisode();
                }
            }
        }
    }

    IEnumerator RespawnPlayer(GameObject player)
    {
        player.SetActive(false);
        yield return new WaitForSeconds(1f);

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.Respawn();
        else if (puntInici != null)
            player.transform.position = puntInici.position;
        else
            player.transform.position = Vector3.zero;

        player.SetActive(true);
        Debug.Log("[EnemyAgent] Jugador fet respawn");
    }

    public void FerRespawn()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            PlayerController pc = p.GetComponent<PlayerController>();
            if (pc != null)
                pc.Respawn();
        }
    }

    public void SetTarget(Transform target)
    {
        targetPlayer = target;
    }

    [System.Serializable]
    public class EnemyServerPosition
    {
        public float x;
        public float y;
    }
}
