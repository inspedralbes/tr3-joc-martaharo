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

            // Iniciem la corrutina robusta de càmera
            StartCoroutine(CorrutinaAutoCamara());
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

    private System.Collections.IEnumerator CorrutinaAutoCamara()
    {
        SeguimentOcell camara = null;
        // Intentar buscar la cámara durante 3 segundos máximo
        float tiempoInicio = Time.time;
        while (camara == null && Time.time - tiempoInicio < 3f)
        {
            camara = Object.FindFirstObjectByType<SeguimentOcell>();
            if (camara == null) yield return new WaitForSeconds(0.1f);
        }

        if (camara != null && IsOwner)
        {
            camara.SetTarget(this.transform);
            Debug.Log("<color=green>[EXITO]</color> Cámara vinculada en flujo real.");
        }
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

    // --- REAPARICIÓ (RESPAWN) ---
    public void Respawn() 
    {
        // Funció buida per evitar errors de compilació de la IA
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}