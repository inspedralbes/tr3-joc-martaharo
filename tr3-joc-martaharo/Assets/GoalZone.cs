using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SocketIOClient;

/// <summary>
/// Script per detectar quan els dos jugadors entren a la zona d'objectiu.
/// Quan ambdós jugadors hi són, s'envia la puntuació al servidor.
/// </summary>
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

    void Start()
    {
        // Iniciar connexió Socket.io
        StartCoroutine(ConnectarSocket());
    }

    IEnumerator ConnectarSocket()
    {
        client = new SocketIO(urlServidor);
        yield return client.ConnectAsync();
    }

    /// <summary>
    /// Quan un collider entra a la zona de l'objectiu.
    /// </summary>
    void OnTriggerEnter2D(Collider2D altre)
    {
        // Comprovar si és un jugador (té el tag "Player")
        if (altre.CompareTag("Player"))
        {
            // Obtenir el playerId del jugador
            PlayerController jugador = altre.GetComponent<PlayerController>();
            
            if (jugador != null && !jugadorsDins.Contains(jugador.playerId))
            {
                jugadorsDins.Add(jugador.playerId);
                Debug.Log("Jugador entrant a la zona d'objectiu: " + jugador.playerId + 
                          " (" + jugadorsDins.Count + "/2)");

                // Comprovar si tots dos jugadors són dins
                if (jugadorsDins.Count >= 2)
                {
                    GestionaraVictoria();
                }
            }
        }
    }

    /// <summary>
    /// Quan un collider surt de la zona de l'objectiu.
    /// </summary>
    void OnTriggerExit2D(Collider2D altre)
    {
        if (altre.CompareTag("Player"))
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
    /// Gestionar l'esdeveniment de victòria quan tots dos jugadors són dins.
    /// </summary>
    void GestionaraVictoria()
    {
        Debug.Log("VICTÒRIA! Tots dos jugadors han arrivat a la zona d'objectiu!");
        
        // Enviar la puntuació al servidor
        if (AuthManager.nomUsuari != null)
        {
            StartCoroutine(EnviarPuntuacio(AuthManager.nomUsuari, puntuacioVictoria));
        }

        // Notificar als jugadors via Socket.io
        if (client != null && client.Connected)
        {
            client.EmitAsync("partidaAcabada", new
            {
                puntuacio = puntuacioVictoria
            });
        }

        // Mostrar missatge de victòria (pots personalitzar-ho)
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
}
