using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

// =================================================================================
// SCRIPT: PlayerController
// UBICACIÓ: Assets/_Project/Scripts/Player/
// DESCRIPCIÓ: Control de moviment simplificat per Unity Netcode (Sense dependències)
// =================================================================================

public class PlayerController : NetworkBehaviour
{
    // Velocitat del moviment del jugador (Preservada)
    public float velocitat = 5f;

    // ID del jugador (S'omple amb el nom per defecte)
    public string playerId;

    // Sincronització del nom en xarxa
    public NetworkVariable<FixedString32Bytes> playerNameSync = new NetworkVariable<FixedString32Bytes>(
        readPerm: NetworkVariableReadPermission.Everyone, 
        writePerm: NetworkVariableWritePermission.Owner
    );

    // Punt d'inici per al respawn
    public Transform puntInici;

    // Referència al component Rigidbody2D per al moviment (Preservada)
    private Rigidbody2D rb;

    // Referència a l'Animator per controlar les animacions de moviment
    private Animator animator;

    // Variables per emmagatzemar l'entrada del jugador
    private float inputX;
    private float inputY;

    // Text MeshPro per mostrar el nom d'usuari
    private TextMeshPro nomUsuariText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Si sóc el propietari, configuro el nom per defecte
        if (IsOwner)
        {
            playerNameSync.Value = "Jugador " + OwnerClientId;
            
            Debug.Log("Sóc el propietari d'aquest objecte (" + OwnerClientId + "). Configurant càmera.");
            
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.SetParent(transform);
                mainCamera.transform.localPosition = new Vector3(0, 0, -10);
                Debug.Log("Càmera configurada per seguir el propietari.");
            }
        }
        
        // Subscripció als canvis de nom per a la UI
        playerNameSync.OnValueChanged += (oldValue, newValue) => {
            UpdatePlayerNameUI(newValue.ToString());
        };

        // Inicialització visual i de la UI
        CrearTextNomUsuari();
        UpdatePlayerNameUI(playerNameSync.Value.ToString());

        // Spawn: Configurar posició inicial a Vector3.zero en iniciar-se el joc
        transform.position = Vector3.zero;
    }

    void Start()
    {
        // Obtenir el component Rigidbody2D d'aquest objecte (Preservada)
        rb = GetComponent<Rigidbody2D>();
        
        // Obtenir el component Animator per a les animacions
        animator = GetComponent<Animator>();
        
        // Configurar posició inicial a Vector3.zero en iniciar-se
        if (IsServer && !IsOwner)
        {
            transform.position = Vector3.zero;
        }
    }

    /// <summary>
    /// Crear un objecte de text TMP a sobre del jugador amb el seu nom d'usuari.
    /// </summary>
    void CrearTextNomUsuari()
    {
        nomUsuariText = GetComponentInChildren<TextMeshPro>();

        if (nomUsuariText == null)
        {
            GameObject textObject = new GameObject("NomUsuari");
            textObject.transform.SetParent(transform);
            textObject.transform.localPosition = new Vector3(0, 1.5f, 0);

            nomUsuariText = textObject.AddComponent<TextMeshPro>();
            nomUsuariText.fontSize = 3;
            nomUsuariText.alignment = TextAlignmentOptions.Center;
            nomUsuariText.color = Color.white;
        }
    }

    /// <summary>
    /// Mètode per actualitzar la UI del nom i la variable playerId.
    /// </summary>
    void UpdatePlayerNameUI(string newName)
    {
        playerId = newName;
        if (nomUsuariText != null)
        {
            nomUsuariText.text = newName;
        }
    }

    void Update()
    {
        // Seguretat Multijugador: Només el propietari controla el seu personatge
        if (!IsOwner) return;

        // Lectura de tecles (Preservada)
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        // Moviment i Animació: Activar el paràmetre isMoving segons el moviment
        bool isMoving = inputX != 0 || inputY != 0;
        if (animator != null)
        {
            animator.SetBool("isMoving", isMoving);
        }
    }

    void FixedUpdate()
    {
        // Moviment: També protegit per IsOwner per a la física
        if (!IsOwner) return;

        // Moviment del jugador usant Rigidbody2D i velocitat (Preservada)
        Vector2 moviment = new Vector2(inputX, inputY).normalized * velocitat;
        
        // Assegurem l'ús de linearVelocity per a versions noves de Unity
        rb.linearVelocity = moviment;
    }

    public void Respawn()
    {
        if (!IsOwner) return;

        if (puntInici != null)
        {
            transform.position = puntInici.position;
        }
        else
        {
            transform.position = Vector3.zero;
        }
        Debug.Log("Jugador tornant al punt d'inici (respawn)");
    }
}
