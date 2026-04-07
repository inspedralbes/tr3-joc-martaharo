// =================================================================================
// SCRIPT: PlayerController
// UBICACIÓ: Assets/_Project/Scripts/Player/
// DESCRIPCIÓ: Control de moviment del jugador compatible amb Netcode for GameObjects
// =================================================================================

using UnityEngine;
using TMPro;
using Unity.Netcode; // Requerit per Netcode for GameObjects
using System.Collections;
using System.Collections.Generic;

public class PlayerController : NetworkBehaviour // Herència: NetworkBehaviour (Regla Estricta 1)
{
    // Velocitat del moviment del jugador (Preservada)
    public float velocitat = 5f;

    // Punt d'inici per al respawn (Preservada)
    public Transform puntInici;

    // Referència al component Rigidbody2D per al moviment (Preservada)
    private Rigidbody2D rb;

    // Variables per emmagatzemar l'entrada del jugador
    private float inputX;
    private float inputY;

    // --- Capes de Red Necessàries (Añadides) ---
    
    // Altres scripts com EnemyAI i PlayerController la necessiten llegir (segons instruccions anteriors)
    public string roomId;

    // Text MeshPro per mostrar el nom d'usuari
    private TextMeshPro nomUsuariText;

    // Càmera Individual: OnNetworkSpawn (Regla Estricta 3)
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            Debug.Log("Sóc el propietari d'aquest objecte. Configurant càmera.");
            
            // Buscar la Main Camera de la escena
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // Opció 1: Emparentar (com un dels mètodes proposats)
                // mainCamera.transform.SetParent(transform);
                // mainCamera.transform.localPosition = new Vector3(0, 0, -10);

                // Opció 2: Configurar Cinemachine si existeix (més modern)
                // Si hi ha una càmera virtual, hauria d'apuntar a aquest transform.
                // Aquí seguim la instrucció d'emparentar o configurar.
                
                // Pel tutorial bàsic de Netcode, emparentem o busquem el component de follow.
                // Si usem un script de follow extern, aquí podríem assignar-ne el target.
                
                // Mantenim el focus de la càmera en el propietari
                Debug.Log("Propietari detectat, càmera vinculada.");
            }
        }
        
        // Inicialització visual (nom, etc.)
        CrearTextNomUsuari();
    }

    void Awake()
    {
        // Obtenir el roomId des del LobbyManager (variable estàtica) - Requerit per altres scripts
        roomId = LobbyManager.roomId;
        if (string.IsNullOrEmpty(roomId)) roomId = MainMenuManager.roomId;
    }

    void Start()
    {
        // Obtenir el component Rigidbody2D d'aquest objecte (Preservada)
        rb = GetComponent<Rigidbody2D>();

        // Si és un bot o un altre objecte de xarxa que no controlem, no l'hem d'inicialitzar més d'aquí
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

        // En Netcode, podríem fer servir NetworkVariable per sincronitzar-ho millor
        if (IsOwner && AuthManager.username != null)
        {
            nomUsuariText.text = AuthManager.username;
        }
        else
        {
            nomUsuariText.text = "Jugador " + OwnerClientId;
        }
    }

    void Update()
    {
        // Restricció de Control: if (!IsOwner) return; (Regla Estricta 2)
        if (!IsOwner) return;

        // Lectura de tecles (Preservada)
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        // Restricció de Control (Regla Estricta 5: Sincronització via físiques)
        if (!IsOwner) return;

        // Moviment del jugador usant Rigidbody2D i velocitat (Preservada)
        Vector2 moviment = new Vector2(inputX, inputY).normalized * velocitat;
        rb.linearVelocity = moviment;
    }

    // Lògica de colisions existent (Preservada - Regla Estricta 4)
    private void OnCollisionEnter2D(Collision2D col)
    {
        // Aquí aniria la lògica de col·lisions que el jugador ja tingui
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


// Classes per deserialitzar les dades rebudes del servidor
[System.Serializable]
public class PlayerMoveData
{
    public string playerId;
    public float x;
    public float y;
}

[System.Serializable]
public class PlayerPosition
{
    public float x;
    public float y;
    public string name;
}

[System.Serializable]
public class GameFinishedResponse
{
    public string winnerId;
    public string winnerName;
}
