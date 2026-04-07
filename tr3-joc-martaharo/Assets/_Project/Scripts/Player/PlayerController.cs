// =================================================================================
// SCRIPT: PlayerController
// UBICACIÓ: Assets/_Project/Scripts/Player/
// DESCRIPCIÓ: Control de moviment del jugador i comunicació Socket.io
// =================================================================================

using UnityEngine;
using TMPro;
using SocketIOClient;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // Velocitat del moviment del jugador
    public float velocitat = 5f;

    // Punt d'inici per al respawn
    public Transform puntInici;

    // Referència al component Rigidbody2D per al moviment
    private Rigidbody2D rb;

    // Variables per emmagatzemar l'entrada del jugador
    private float inputX;
    private float inputY;

    // Variables per a la connexió Socket.io
    private SocketIO client;
    private bool clientConnectat = false;

    // Identificador del jugador i de la sala
    public string playerId;
    public string roomId;

    // Referència a l'altre jugador (l'oponent)
    public GameObject opponentPlayer;
    private bool primerCop = true;

    // Text MeshPro per mostrar el nom d'usuari
    private TextMeshPro nomUsuariText;

    // Velocitat d'interpolació per a l'oponent
    public float lerpSpeed = 5f;
    private Vector3 targetOpponentPosition;

    void Awake()
    {
        // Obtenir el roomId des del LobbyManager (variable estàtica)
        roomId = LobbyManager.roomId;

        if (string.IsNullOrEmpty(roomId))
        {
            roomId = "sala1";
            Debug.LogWarning("No s'ha trobat roomId, utilitzant per defecte: sala1");
        }
        else
        {
            Debug.Log("RoomId rebut del Lobby: " + roomId);
        }
    }

    void Start()
    {
        // Obtenir el component Rigidbody2D d'aquest objecte
        rb = GetComponent<Rigidbody2D>();

        // Configurar l'identificador del jugador
        playerId = "player_" + GetInstanceID();

        // Crear el text amb el nom d'usuari a sobre del jugador
        CrearTextNomUsuari();

        // Iniciar la connexió Socket.io
        StartCoroutine(ConnectarSocket());

        // Inicialitzar posició objectiu
        targetOpponentPosition = Vector3.zero;
    }

    /// <summary>
    /// Crear un objecte de text TMP a sobre del jugador amb el seu nom d'usuari.
    /// </summary>
    void CrearTextNomUsuari()
    {
        // Buscar si ja existeix un fill amb el component TextMeshPro
        nomUsuariText = GetComponentInChildren<TextMeshPro>();

        if (nomUsuariText == null)
        {
            // Crear un nou GameObject per al text
            GameObject textObject = new GameObject("NomUsuari");
            textObject.transform.SetParent(transform);
            textObject.transform.localPosition = new Vector3(0, 1.5f, 0);

            // Afegir el component TextMeshPro
            nomUsuariText = textObject.AddComponent<TextMeshPro>();

            // Configurar l'estil del text
            nomUsuariText.fontSize = 3;
            nomUsuariText.alignment = TextAlignmentOptions.Center;
            nomUsuariText.color = Color.white;
            nomUsuariText.outlineWidth = 0.2f;
            nomUsuariText.outlineColor = Color.black;
        }

        // Assignar el nom d'usuari (des d'AuthManager)
        if (AuthManager.username != null)
        {
            nomUsuariText.text = AuthManager.username;
        }
        else
        {
            nomUsuariText.text = playerId;
        }
    }

    // Corrutina per connectar-se al servidor Socket.io
    IEnumerator ConnectarSocket()
    {
        client = new SocketIO("http://localhost:3000");

        // Escoltar l'esdeveniment 'playerMoved' del servidor
        client.On("playerMoved", (data) =>
        {
            try
            {
                string jsonStr = data.ToString();
                PlayerMoveData response = JsonUtility.FromJson<PlayerMoveData>(jsonStr);

                if (response.playerId != playerId && opponentPlayer != null)
                {
                    targetOpponentPosition = new Vector3(response.x, response.y, 0);
                }
            }
            catch
            {
                // Si falla JsonUtility, ignorem l'esdeveniment
            }
        });

        // Escoltar l'esdeveniment 'syncPositions' per rebre totes les posicions existents
        client.On("syncPositions", (data) =>
        {
            if (primerCop)
            {
                primerCop = false;
                // Per a syncPositions, depenem de SocketIOClient que retorna JSON
                // Com JsonUtility no gestiona diccionaris, ignorem per ara
            }
        });

        // Escoltar l'esdeveniment de desconnexió de l'altre jugador
        client.On("jugadorDesconnectat", (data) =>
        {
            Debug.Log("L'altre jugador s'ha desconnectat!");
            
            // Desactivar l'oponent
            if (opponentPlayer != null)
            {
                opponentPlayer.SetActive(false);
            }
        });

        // Escoltar l'esdeveniment de victòria/derrota
        client.On("gameFinished", (data) =>
        {
            GameFinishedResponse response = JsonUtility.FromJson<GameFinishedResponse>(data.ToString());
            
            if (response.winnerName == AuthManager.username)
            {
                Debug.Log("Has guanyat!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Victory();
                }
            }
            else
            {
                Debug.Log("Has perdut!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Defeat();
                }
            }
        });

        // Escoltar quan un jugador és atrapat
        client.On("playerCaught", (data) =>
        {
            Debug.Log("Un jugador ha estat atrapat per l'enemic!");
        });

        yield return client.ConnectAsync();

        if (client.Connected)
        {
            clientConnectat = true;
            Debug.Log("Connectat al servidor Socket.io!");

            // Unir-se a la sala
            client.EmitAsync("joinRoom", new
            {
                roomId = roomId,
                playerId = playerId,
                playerName = AuthManager.username ?? playerId
            });
        }
        else
        {
            Debug.LogError("No s'ha pogut connectar al servidor Socket.io!");
        }
    }

    void Update()
    {
        // Obtenir l'entrada del teclat (Fletxes o WASD)
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        // Interpolar la posició de l'oponent per suavitzar el moviment
        if (opponentPlayer != null && targetOpponentPosition != Vector3.zero)
        {
            opponentPlayer.transform.position = Vector3.Lerp(
                opponentPlayer.transform.position, 
                targetOpponentPosition, 
                Time.deltaTime * lerpSpeed
            );
        }
    }

    void FixedUpdate()
    {
        // Moviment del jugador usant Rigidbody2D i velocitat
        Vector2 moviment = new Vector2(inputX, inputY).normalized * velocitat;
        rb.linearVelocity = moviment;

        // Si estem connectats i el jugador s'ha mogut, enviar la posició al servidor
        if (clientConnectat && (inputX != 0 || inputY != 0))
        {
            EnviarPosicio();
        }
    }

    // Enviar la posició actual al servidor
    void EnviarPosicio()
    {
        if (clientConnectat && client != null)
        {
            client.EmitAsync("updatePosition", new
            {
                roomId = roomId,
                playerId = playerId,
                x = transform.position.x,
                y = transform.position.y
            });
        }
    }

    void OnDestroy()
    {
        // Desconnectar quan es destrueixi l'objecte
        if (clientConnectat && client != null)
        {
            // Notificar als altres jugadors que ens desconnectem
            client.EmitAsync("jugadorDesconnectat", new { playerId = playerId });
            client.DisconnectAsync();
        }
    }

    public void Respawn()
    {
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
