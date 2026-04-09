using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Reflection;

// =================================================================================
// SCRIPT: PlayerController
// UBICACIÓ: Assets/_Project/Scripts/Player/
// DESCRIPCIÓ: Control de moviment per a multijugador EXCLUSIU Netcode.
//             Gestiona autoritat, càmera, audio i animacions.
// =================================================================================

public class PlayerController : NetworkBehaviour
{
    [Header("Identitat del Jugador")]
    public string playerId;
    public int playerNumber;

    [Header("Moviment")]
    public float speed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private float inputX;
    private float inputY;

    private void Awake()
    {
        // 1. Cerca de components
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // 2. Correció per al NetworkAnimator amb Reflection 
        // (Evita el bloqueig del Inspector i errors de compilació)
        NetworkAnimator networkAnim = GetComponent<NetworkAnimator>();
        if (networkAnim != null)
        {
            var field = typeof(NetworkAnimator).GetField("m_Animator", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(networkAnim, anim);
            }
        }
        
        // 3. Configuració Física Antigravetat
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
            playerId = "Jugador Local (" + OwnerClientId + ")";
            playerNumber = (int)OwnerClientId;
            
            // Propietari: Físiques dinàmiques
            if (rb != null)
            {
                rb.simulated = true;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            ConfigurarCamera();
        }
        else
        {
            // Remot: Físiques desactivades (simulació) per evitar conflictes amb NetworkTransform
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.simulated = false; 
            }
            
            // Desactivar components de càmera/audio que puguin venir en el prefab
            DesactivarComponentsRemots();
        }
    }

    void ConfigurarCamera()
    {
        // Iniciem la corrutina de configuració per permetre reintents si la càmera triga a carregar
        StartCoroutine(ConfiguracioCameraCoroutine());
    }

    private System.Collections.IEnumerator ConfiguracioCameraCoroutine()
    {
        // 1. Prevenció d'errors de Shutdown
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) yield break;
        if (!IsOwner) yield break;

        int intents = 0;
        int maxIntents = 3;
        bool exit = false;

        while (intents < maxIntents && !exit)
        {
            // 2. Búsqueda Directa per Nom
            GameObject camObj = GameObject.Find("Main Camera");

            if (camObj != null)
            {
                // 3. Triple verificació (Tipus i String)
                Component cameraScript = camObj.GetComponent("SeguimentOcell");

                if (cameraScript != null)
                {
                    // Èxit: Configuració del target i audio
                    ((SeguimentOcell)cameraScript).playerTarget = transform;
                    
                    AudioListener[] allListeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
                    foreach (var l in allListeners) l.enabled = false;

                    AudioListener localListener = camObj.GetComponent<AudioListener>();
                    if (localListener != null) localListener.enabled = true;

                    Debug.Log($"[PlayerController] TRIPLE VERIFICACIÓ ÈXIT: Connectat a '{camObj.name}' al intent {intents + 1}.");
                    exit = true;
                }
                else
                {
                    Debug.LogWarning($"[PlayerController] Intent {intents + 1}: Objecte '{camObj.name}' trobat però 'SeguimentOcell' encara no és visible.");
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerController] Intent {intents + 1}: 'Main Camera' encara no existeix a l'escena Joc.");
            }

            if (!exit)
            {
                intents++;
                yield return new WaitForSeconds(0.1f); // Esperem 0.1s abans del següent intent
            }
        }

        if (!exit)
        {
            Debug.LogError("[PlayerController] ERROR FINAL: Després de 3 intents, l'objecte 'Main Camera' o el script 'SegumientOcell' segueixen invisibles.");
        }
    }

    void DesactivarComponentsRemots()
    {
        // Ens assegurem que els ocells que no són nostres no tinguin càmeres ni listeners actius
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null) cam.enabled = false;

        AudioListener listener = GetComponentInChildren<AudioListener>();
        if (listener != null) listener.enabled = false;
    }

    void Update()
    {
        if (!IsOwner) return;

        // Captura d'inputs
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        // Actualització del Animator per al Blend Tree 2D
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

        // Moviment físic (Unity 6 friendly)
        Vector2 moviment = new Vector2(inputX, inputY).normalized;
        
        if (rb != null)
        {
            rb.linearVelocity = moviment * speed;
        }
    }

    public void Respawn()
    {
        if (!IsOwner) return;
        transform.position = Vector3.zero;
    }
}