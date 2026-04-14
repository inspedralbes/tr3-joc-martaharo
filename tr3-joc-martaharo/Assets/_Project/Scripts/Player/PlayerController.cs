using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Reflection;
using System.Collections;

// =================================================================================
// SCRIPT: PlayerController
// UBICACIÓ: Assets/_Project/Scripts/Player/
// DESCRIPCIÓ: Control de moviment per a multijugador Unity 6 (Netcode).
//             Gestiona autoritat, colors, càmera, animacions i reaparició.
// =================================================================================

public class PlayerController : NetworkBehaviour
{
    [Header("Identitat i Estètica")]
    public string playerId;
    public Sprite spriteBlanco; // Sprite per al Client

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

        // Correció per al NetworkAnimator amb Reflection
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

        // LÒGICA DE COLOR: Si no som el servidor, canviem el sprite al blanc
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

    void Update()
    {
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

 // --- SISTEMA DE VIDA (Sincronizado) ---
   public void RecibirDanyo()
{
    if (!IsServer) return; 
    if (esInvulnerable || estaInvulnerable.Value) return;

    esInvulnerable = true;
    estaInvulnerable.Value = true;
    vidaSincronizada.Value--; 

    MostrarEfectoDanyoClientRpc();
    StartCoroutine(QuitarInvulnerabilidad());

    if (vidaSincronizada.Value <= 0)
    {
        Respawn();
        vidaSincronizada.Value = 2;
        estaInvulnerable.Value = false;
        esInvulnerable = false;
    }
}

private IEnumerator QuitarInvulnerabilidad()
{
    yield return new WaitForSeconds(1f);
    esInvulnerable = false;
    estaInvulnerable.Value = false;
}
    [ClientRpc]
    private void MostrarEfectoDanyoClientRpc()
    {
        StartCoroutine(EfectoVisualDanyo());
    }

    private IEnumerator EfectoVisualDanyo()
    {
        if (sr != null) sr.color = Color.red;
        
        yield return new WaitForSeconds(1f);
        
        if (sr != null) sr.color = Color.white;
    }

    public override void OnNetworkDespawn()
{
    // Solo el Servidor tiene permiso para cambiar el valor de una NetworkVariable
    if (IsServer && estaInvulnerable != null)
    {
        estaInvulnerable.Value = false;
    }
    
    base.OnNetworkDespawn();
}

    private System.Collections.IEnumerator InvulnerabilidadTemporal()
    {
        esInvulnerable = true;
        if (sr != null) sr.color = Color.red;
        yield return new WaitForSeconds(1f);
        if (sr != null) sr.color = Color.white;
        esInvulnerable = false;
    }

   // --- REAPARICIÓ (RESPAWN) ---
    public void Respawn() 
    {
        // Solo el servidor tiene permiso para mover objetos físicamente
        // y que ese movimiento se replique a todos.
        if (!IsServer) return; 

        GameObject spawnPoint = GameObject.FindWithTag("Respawn");
        Vector3 posicionDestino = (spawnPoint != null) ? spawnPoint.transform.position : Vector3.zero;

        // Al cambiar la posición en el Servidor, el NetworkTransform 
        // se encarga de avisar a los demás automáticamente.
        transform.position = posicionDestino;

        if (rb != null) 
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        Debug.Log($"[SERVER] Jugador {OwnerClientId} reaparecido en {posicionDestino}");
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}