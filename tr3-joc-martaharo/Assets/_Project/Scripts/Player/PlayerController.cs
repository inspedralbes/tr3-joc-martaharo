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
    public int vida = 2;
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
                rb.simulated = false; 
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

    // --- SISTEMA DE VIDA ---
    public void RecibirDanyo()
    {
        if (esInvulnerable) return;

        vida--;

        if (vida <= 0)
        {
            Respawn();
            vida = 2;
            return;
        }

        StartCoroutine(InvulnerabilidadTemporal());
    }

    private System.Collections.IEnumerator InvulnerabilidadTemporal()
    {
        esInvulnerable = true;
        if (sr != null) sr.color = Color.red;
        yield return new WaitForSeconds(1.2f);
        if (sr != null) sr.color = Color.white;
        esInvulnerable = false;
    }

    // --- REAPARICIÓ (RESPAWN) ---
    public void Respawn() 
    {
        GameObject spawnPoint = GameObject.FindWithTag("Respawn");
        if (spawnPoint != null)
            transform.position = spawnPoint.transform.position;
        else
            transform.position = Vector2.zero;

        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}