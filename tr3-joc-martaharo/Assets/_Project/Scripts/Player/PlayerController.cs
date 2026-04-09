using Unity.Netcode;
using UnityEngine;

// =================================================================================
// SCRIPT: PlayerController
// UBICACIÓ: Assets/_Project/Scripts/Player/
// DESCRIPCIÓ: Control de moviment per a multijugador EXCLUSIU Netcode
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
        // Cerca automàtica de components si no estan assignats
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        // Configuració Física Antigravetat per defecte
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
            
            // Configuració de Físiques definitiva per al Propietari
            if (rb != null)
            {
                rb.simulated = true;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            ConfigurarCamera();
        }
        else
        {
            // Substitució de isKinematic per Dynamic segons la teva petició
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.simulated = false; // Mantenim simulated = false per als remots per evitar conflictes
            }
        }
        
        // Eliminem transform.position = Vector3.zero per evitar salts visuals innecessaris o conflictes amb NetworkTransform
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
    }

    void FixedUpdate()
    {
        // Vital: Només el propietari mou el seu ocell físicament
        if (!IsOwner) return;

        // Vector de moviment normalitzat
        Vector2 moviment = new Vector2(inputX, inputY).normalized;
        
        if (rb != null)
        {
            // Aplicar velocitat lineal amb velocitat (Estàndard de Unity 6)
            rb.linearVelocity = moviment * speed;
        }
    }

    public void Respawn()
    {
        if (!IsOwner) return;
        transform.position = Vector3.zero;
    }
}