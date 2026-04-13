using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Reflection;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    [Header("Identitat i Estètica")]
    public string playerId;
    public Sprite spriteBlanco;

    [Header("Moviment")]
    public float speed = 5f;

    [Header("Sistema de Vida")]
    public NetworkVariable<int> vidaSincronizada = new NetworkVariable<int>(2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> estaInvulnerable = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private bool esInvulnerable = false;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private float inputX;
    private float inputY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        NetworkAnimator networkAnim = GetComponent<NetworkAnimator>();
        if (networkAnim != null)
        {
            var field = typeof(NetworkAnimator).GetField("m_Animator", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null) field.SetValue(networkAnim, anim);
        }
        
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer && spriteBlanco != null)
        {
            GetComponent<SpriteRenderer>().sprite = spriteBlanco;
        }

        if (IsOwner)
        {
            playerId = "Jugador Local (" + OwnerClientId + ")";
            
            if (rb != null)
            {
                rb.simulated = true;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            StartCoroutine(CorrutinaCamaraBuildFix());
        }
        else
        {
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.simulated = true;
            }
            DesactivarComponentsRemots();
        }
    }

    private void Update()
    {
        // FUERZA DE POSICIÓN: Asegurar Z = 0 siempre
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        if (!IsOwner) return;

        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        if (anim != null)
        {
            anim.SetFloat("VelocitatX", inputX);
            anim.SetFloat("VelocitatY", inputY);
            float magnitud = new Vector2(inputX, inputY).magnitude;
            anim.SetFloat("Velocitat", magnitud);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector2 moviment = new Vector2(inputX, inputY).normalized;
        if (rb != null) rb.linearVelocity = moviment * speed;
    }

    private System.Collections.IEnumerator CorrutinaCamaraBuildFix()
    {
        if (!IsOwner) yield break;

        SeguimentOcell scriptCamara = null;

        while (scriptCamara == null)
        {
            scriptCamara = Object.FindFirstObjectByType<SeguimentOcell>();

            if (scriptCamara == null && Camera.main != null)
            {
                scriptCamara = Camera.main.GetComponent<SeguimentOcell>();
            }

            if (scriptCamara == null) yield return new WaitForSeconds(0.1f);
        }

        scriptCamara.SetTarget(this.transform);
        Debug.Log("<color=cyan>[SISTEMA]</color> Cámara vinculada con éxito en el Build local.");
    }

    void DesactivarComponentsRemots()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null) cam.enabled = false;

        AudioListener listener = GetComponentInChildren<AudioListener>();
        if (listener != null) listener.enabled = false;
    }

    // --- MÉTODO PÚBLICO RECIBIR DAÑO (llamado por EnemyAI) ---
    public void RecibirDanyo()
    {
        Debug.Log($"[Player] RecibirDanyo() llamado. IsServer: {IsServer}");

        // Si NO es el servidor, el cliente envía petición RPC al servidor
        if (!IsServer)
        {
            Debug.Log("[Player] Enviando SolicitarDanyoServerRpc...");
            SolicitarDanyoServerRpc();
            return;
        }

        // Si ES el servidor, procesar directamente
        Debug.Log("[Player] Procesando daño en servidor...");
        ProcesarDanyo();
    }

    // --- RPC: CLIENTE PIDE DAÑO AL SERVIDOR ---
    [Rpc(SendTo.Server)]
    private void SolicitarDanyoServerRpc()
    {
        if (!IsServer) return;
        Debug.Log("[SERVER] Solicitud de daño recibida.");
        ProcesarDanyo();
    }

    // --- LÓGICA DE DAÑO (solo servidor) ---
    private void ProcesarDanyo()
    {
        if (esInvulnerable || estaInvulnerable.Value) return;

        esInvulnerable = true;
        estaInvulnerable.Value = true;
        vidaSincronizada.Value--;

        Debug.Log($"[SERVER] Daño procesado. Jugador: {OwnerClientId}. Vida restante: {vidaSincronizada.Value}");

        // Enviar efecto visual a TODOS
        MostrarEfectoRojoRpc();

        StartCoroutine(QuitarInvulnerabilidad());

        // Si vida <= 0, ejecutar respawn
        if (vidaSincronizada.Value <= 0)
        {
            MandarAlSpawn();
        }
    }

    // --- EFECTO VISUAL (todos ven el color rojo) ---
    [Rpc(SendTo.Everyone)]
    private void MostrarEfectoRojoRpc()
    {
        Debug.Log("[Player] Mostrando efecto rojo...");
        StartCoroutine(EfectoVisualDanyo());
    }

    private IEnumerator EfectoVisualDanyo()
    {
        if (sr != null) sr.color = Color.red;
        
        yield return new WaitForSeconds(1f);
        
        if (sr != null) sr.color = Color.white;
    }

    // --- RESPAWN CON TELEPORT INFALIBLE ---
    private void MandarAlSpawn()
    {
        if (!IsServer) return;

        GameObject spawnPoint = GameObject.FindWithTag("Respawn");
        
        if (spawnPoint == null)
        {
            Debug.LogError("[SERVER] ERROR: No se encontró el objeto con Tag 'Respawn'.");
            return;
        }

        Vector3 spawnPos = spawnPoint.transform.position;
        Quaternion spawnRot = spawnPoint.transform.rotation;

        // USO OBLIGATORIO DE .TELEPORT() - GetComponent<NetworkTransform>()
        NetworkTransform nt = GetComponent<NetworkTransform>();
        if (nt != null)
        {
            nt.Teleport(spawnPos, spawnRot, transform.localScale);
            Debug.Log($"[SERVER] Teleport ejecutado para jugador {OwnerClientId} en posición {spawnPos}");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // RESET DE VIDA A 2
        vidaSincronizada.Value = 2;
        estaInvulnerable.Value = false;
        esInvulnerable = false;

        Debug.Log($"[SERVER] Jugador {OwnerClientId} enviado al spawn en {spawnPos}. Vida reseteada a 2.");
    }

    private IEnumerator QuitarInvulnerabilidad()
    {
        yield return new WaitForSeconds(1f);
        esInvulnerable = false;
        if (IsServer)
        {
            estaInvulnerable.Value = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (estaInvulnerable != null && IsServer)
            estaInvulnerable.Value = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
