// =================================================================================
// SCRIPT: GoalZone
// UBICACIÓ: Assets/_Project/Scripts/Game/
// DESCRIPCIÓ: Detecta quan els jugadors entren a la zona d'objectiu i guarda puntuació
// =================================================================================

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SocketIOClient;

public class GoalZone : MonoBehaviour
{
    // URL del servidor Node.js
    private string urlServidor = "http://localhost:3000";

    // Llista de jugadors que estan dins de la zona
    private List<string> jugadorsDins = new List<string>();

    // Referència al client Socket.io
    private SocketIO client;

    // Puntuació per guanyar la partida
    private int puntuacioVictoria = 100;

    // Control de si ja s'ha guanyat
    private bool jaGuanyat = false;

    // Control de si la meta està bloquejada
    private bool metaBloquejada = false;
    private string guanyador = null;

    // Referència a la sala
    private string roomId;

    void Start()
    {
        roomId = LobbyManager.roomId;
        if (string.IsNullOrEmpty(roomId))
            roomId = MainMenuManager.roomId;

        // Iniciar connexió Socket.io
        StartCoroutine(ConnectarSocket());
    }

    IEnumerator ConnectarSocket()
    {
        client = new SocketIO(urlServidor);

        // Escoltar l'estat de la meta des del servidor
        client.On("goalStatus", (data) =>
        {
            var status = JsonUtility.FromJson<GoalStatusData>(data.ToString());
            metaBloquejada = status.blocked;
            guanyador = status.winner;

            if (metaBloquejada && guanyador != AuthManager.nomUsuari)
            {
                Debug.Log("La meta està bloquejada. Has perdut!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Defeat();
                }
            }
        });

        // Escoltar quan algú guanya
        client.On("gameFinished", (data) =>
        {
            var result = JsonUtility.FromJson<GameFinishedClientData>(data.ToString());
            if (result.winnerName == AuthManager.nomUsuari)
            {
                // Hem guanyat nosaltres
                metaBloquejada = true;
            }
            else
            {
                // Ha guanyat l'altre
                metaBloquejada = true;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Defeat();
                }
            }
        });

        yield return client.ConnectAsync();
    }

    /// <summary>
    /// Quan un collider entra a la zona de l'objectiu.
    /// </summary>
    void OnTriggerEnter2D(Collider2D altre)
    {
        // Si la meta està bloquejada, no es pot guanyar
        if (metaBloquejada || jaGuanyat) return;

        // Comprovar si és un jugador (té el tag "Player" o "Meta")
        if (altre.CompareTag("Player") || altre.CompareTag("Meta"))
        {
            // Obtenir el playerId del jugador
            PlayerController jugador = altre.GetComponent<PlayerController>();
            
            if (jugador != null)
            {
                string playerId = jugador.playerId;

                if (!jugadorsDins.Contains(playerId))
                {
                    jugadorsDins.Add(playerId);
                    Debug.Log("Jugador entrant a la zona d'objectiu: " + playerId + 
                              " (" + jugadorsDins.Count + "/2)");

                    // En mode individual: quan un jugador arriba, guanya directament
                    if (MainMenuManager.isSinglePlayer)
                    {
                        GestionaraVictoria();
                    }
                    // En multijugador: el primer que arriba guanya
                    else if (!MainMenuManager.isSinglePlayer)
                    {
                        NotificarGuanyador();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Quan un collider surt de la zona de l'objectiu.
    /// </summary>
    void OnTriggerExit2D(Collider2D altre)
    {
        if (altre.CompareTag("Player") || altre.CompareTag("Finish"))
        {
            PlayerController jugador = altre.GetComponent<PlayerController>();
            
            if (jugador != null && jugadorsDins.Contains(jugador.playerId))
            {
                jugadorsDins.Remove(jugador.playerId);
                Debug.Log("Jugador sortint de la zona d'objectiu: " + jugador.playerId);
            }
        }
    }

    /// <summary>
    /// Notifica al servidor que un jugador ha guanyat (multijugador)
    /// </summary>
    void NotificarGuanyador()
    {
        if (client != null && client.Connected)
        {
            string roomId = LobbyManager.roomId;
            if (string.IsNullOrEmpty(roomId))
                roomId = MainMenuManager.roomId;

            client.EmitAsync("gameFinished", new
            {
                roomId = roomId,
                winnerId = AuthManager.nomUsuari,
                winnerName = AuthManager.nomUsuari,
                puntuacio = puntuacioVictoria
            });

            Debug.Log("Guanyador notificat: " + AuthManager.nomUsuari);
        }
    }

    /// <summary>
    /// Gestionar l'esdeveniment de victòria quan tots dos jugadors són dins.
    /// </summary>
    void GestionaraVictoria()
    {
        if (jaGuanyat) return;
        jaGuanyat = true;

        Debug.Log("VICTÒRIA! Has guanyat!");
        
        // Notificar al GameManager per mostrar pantalla de victòria
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Victory();
        }

        // Enviar la puntuació al servidor
        if (AuthManager.nomUsuari != null)
        {
            StartCoroutine(EnviarPuntuacio(AuthManager.nomUsuari, puntuacioVictoria));
        }

        // Notificar als jugadors via Socket.io
        if (client != null && client.Connected)
        {
            string roomId = LobbyManager.roomId;
            if (string.IsNullOrEmpty(roomId))
                roomId = MainMenuManager.roomId;

            client.EmitAsync("gameFinished", new
            {
                roomId = roomId,
                winnerId = AuthManager.nomUsuari,
                winnerName = AuthManager.nomUsuari,
                puntuacio = puntuacioVictoria
            });
        }

        Debug.Log("Puntuació guardada: " + puntuacioVictoria);
    }

    /// <summary>
    /// Enviar la puntuació al servidor per guardar-la al rànquing.
    /// </summary>
    IEnumerator EnviarPuntuacio(string username, int puntuacio)
    {
        // Crear objecte petició
        RankingRequest peticio = new RankingRequest();
        peticio.username = username;
        peticio.puntuacio = puntuacio;
        peticio.tipus = MainMenuManager.isSinglePlayer ? "SINGLE" : "MULTIPLAYER";

        string jsonData = JsonUtility.ToJson(peticio);

        using (UnityWebRequest www = new UnityWebRequest(urlServidor + "/api/rankings", "POST"))
        {
            byte[] jsonToSend = new UTF8Encoding(true).GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Puntuació enviada correctament al servidor!");
            }
            else
            {
                Debug.LogError("Error enviant puntuació: " + www.error);
            }
        }
    }

    // Classe per a la petició de ranking
    [System.Serializable]
    public class RankingRequest
    {
        public string username;
        public int puntuacio;
        public string tipus;
    }

    [System.Serializable]
    public class GoalStatusData
    {
        public bool blocked;
        public string winner;
    }

    [System.Serializable]
    public class GameFinishedClientData
    {
        public string winnerId;
        public string winnerName;
    }
}
