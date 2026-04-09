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

        // LÒGICA DE COLOR: Si el jugador NO és el Host (ClientID != 0), el posem blanc.
        if (OwnerClientId != 0 && spriteBlanco != null && sr != null)
        {
            sr.sprite = spriteBlanco;
        }

        if (IsOwner)
        {
            playerId = "Jugador Local (" + OwnerClientId + ")";
            
            if (rb != null)
            {
                rb.simulated = true;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            // Només configurem la càmera si som el propietari
            ConfigurarCamera();
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

    void ConfigurarCamera()
    {
        StartCoroutine(ConfiguracioCameraCoroutine());
    }

    private IEnumerator ConfiguracioCameraCoroutine()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) yield break;
        if (!IsOwner) yield break;

        int intents = 0;
        int maxIntents = 3;
        bool exit = false;

        while (intents < maxIntents && !exit)
        {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null)
            {
                SeguimentOcell script = camObj.GetComponent<SeguimentOcell>();
                if (script == null) script = camObj.GetComponent("SeguimentOcell") as SeguimentOcell;

                if (script != null)
                {
                    script.playerTarget = transform;
                    
                    AudioListener[] all = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
                    foreach (var l in all) l.enabled = false;

                    AudioListener local = camObj.GetComponent<AudioListener>();
                    if (local != null) local.enabled = true;

                    Debug.Log($"[PlayerController] Càmera connectada correctament.");
                    exit = true;
                }
            }

            if (!exit)
            {
                intents++;
                yield return new WaitForSeconds(0.1f);
            }
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
        if (IsServer)
        {
            RespawnClientRpc();
        }
        else if (IsOwner)
        {
            ExecutarRespawn();
        }
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        if (IsOwner)
        {
            ExecutarRespawn();
        }
    }

    private void ExecutarRespawn()
    {
        transform.position = Vector3.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Debug.Log("[PlayerController] Jugador reaparegut a (0,0,0)");
    }

    private void OnDestroy()
    {
        if (IsOwner && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("[PlayerController] Port alliberat en morir o sortir.");
        }
    }
}