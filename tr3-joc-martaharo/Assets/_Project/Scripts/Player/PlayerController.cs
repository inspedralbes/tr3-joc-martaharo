using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using SocketIOClient;

// =================================================================================
// SCRIPT: PlayerController
// UBICACIÓ: Assets/_Project/Scripts/Player/
// DESCRIPCIÓ: Control de moviment per a multijugador Socket.io + Netcode
// =================================================================================

public class PlayerController : NetworkBehaviour
{
    [Header("Identitat del Jugador")]
    public string playerId;
    public int playerNumber;

    [Header("Moviment")]
    public float speed = 5f;

    [Header("Configuració Física")]
    public bool gravityScaleZero = true;
    public bool freezeRotationZ = true;

    private Rigidbody2D rb;
    private Animator anim;
    private float inputX;
    private float inputY;

    private SocketIO client;
    private string roomId;
    private float syncInterval = 0.05f;
    private float lastSyncTime = 0f;

    private void Awake()
    {
        // Cerca automàtica de components si no estan assignats
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        
        // Configuració Física Antigravetat
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            playerId = "Jugador " + OwnerClientId;
            playerNumber = (int)OwnerClientId;
            
            ConfigurarCamera();
            ConnectarSocket();
        }

        transform.position = Vector3.zero;
    }

    void Start()
    {
        // Components ja buscats a l'Awake
    }

    void ConfigurarCamera()
    {
        if (!IsOwner) return;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.SetParent(transform);
            mainCamera.transform.localPosition = new Vector3(0, 0, -10);

            PlayerCameraFollow cameraFollow = mainCamera.GetComponent<PlayerCameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.Target = transform;
            }
        }
    }

    void ConnectarSocket()
    {
        client = new SocketIO("http://localhost:3000");

        client.OnConnected += (sender, e) => {
            Debug.Log("PlayerController connectat a Socket.io");
        };

        client.On("playerMoved", response => {
            Debug.Log("Posició rebuda del servidor");
        });

        client.ConnectAsync();
    }

    public void SetRoomId(string id)
    {
        roomId = id;
    }

    void Update()
    {
        if (!IsOwner) return;

        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        if (anim != null)
        {
            anim.SetFloat("VelocitatX", inputX);
            anim.SetFloat("VelocitatY", inputY);

            float magnitud = Mathf.Sqrt(inputX * inputX + inputY * inputY);
            anim.SetFloat("Velocitat", magnitud);
        }

        if (client != null && client.Connected && Time.time - lastSyncTime >= syncInterval)
        {
            float x = transform.position.x;
            float y = transform.position.y;
            client.EmitAsync("updatePosition", new { roomId = roomId, x = x, y = y });
            lastSyncTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector2 moviment = new Vector2(inputX, inputY).normalized * speed;
        if (rb != null)
        {
            rb.linearVelocity = moviment;
        }
    }

    public void UpdateRemotePosition(Vector2 newPos)
    {
        StartCoroutine(LerpPosition(newPos));
    }

    System.Collections.IEnumerator LerpPosition(Vector2 targetPos)
    {
        float t = 0f;
        Vector2 startPos = transform.position;
        float duration = 0.1f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
    }

    public void Respawn()
    {
        if (!IsOwner) return;
        transform.position = Vector3.zero;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        if (client != null)
        {
            client.DisconnectAsync();
        }
    }
}