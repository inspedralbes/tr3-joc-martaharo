using UnityEngine;
using SocketIOClient;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class PlayerController : MonoBehaviour
{
    // Velocitat del moviment del jugador
    public float velocitat = 5f;

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
    public string roomId = "sala1";

    // Referència a l'altre jugador (l'oponent)
    public GameObject opponentPlayer;
    private bool primerCop = true;

    void Start()
    {
        // Obtenir el component Rigidbody2D d'aquest objecte
        rb = GetComponent<Rigidbody2D>();

        // Configurar l'identificador del jugador (pots canviar-ho segons el teu sistema)
        playerId = "player_" + GetInstanceID();

        // Iniciar la connexió Socket.io
        StartCoroutine(ConnectarSocket());
    }

    // Corrutina per connectar-se al servidor Socket.io
    IEnumerator ConnectarSocket()
    {
        // CORRECCIÓ AQUÍ: Hem eliminat les claus i el punt i coma que sobraven
        client = new SocketIO("http://localhost:3000");

        // Escoltar l'esdeveniment 'playerMoved' del servidor
        client.On("playerMoved", (data) =>
        {
            // Rebre les dades de posició de l'oponent
            var response = JsonConvert.DeserializeObject<PlayerMoveData>(data.ToString());

            // Actualitzar la posició de l'oponent (si no és nosaltres)
            if (response.playerId != playerId && opponentPlayer != null)
            {
                opponentPlayer.transform.position = new Vector3(response.x, response.y, 0);
            }
        });

        // Escoltar l'esdeveniment 'syncPositions' per rebre totes les posicions existents
        client.On("syncPositions", (data) =>
        {
            if (primerCop)
            {
                primerCop = false;
                var positions = JsonConvert.DeserializeObject<Dictionary<string, PlayerPosition>>(data.ToString());
                
                foreach (var kvp in positions)
                {
                    if (kvp.Key != playerId && opponentPlayer != null)
                    {
                        opponentPlayer.transform.position = new Vector3(kvp.Value.x, kvp.Value.y, 0);
                    }
                }
            }
        });

        // Intentar connectar
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
                playerName = playerId
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
            client.DisconnectAsync();
        }
    }
}

// Classes per deserialitzar les dades rebudes del servidor
public class PlayerMoveData
{
    public string playerId { get; set; }
    public float x { get; set; }
    public float y { get; set; }
}

public class PlayerPosition
{
    public float x { get; set; }
    public float y { get; set; }
    public string name { get; set; }
}
