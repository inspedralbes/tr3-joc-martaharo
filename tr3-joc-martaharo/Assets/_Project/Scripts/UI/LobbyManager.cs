// =================================================================================
// SCRIPT: LobbyManager
// UBICACIÓ: Assets/_Project/Scripts/UI/
// DESCRIPCIÓ: Gestió del Lobby i creació/unió a sales de joc
// =================================================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using SocketIOClient;

public class LobbyManager : MonoBehaviour
{
    // URL del servidor Node.js
    private string urlServidor = "http://localhost:3000";

    [Header("Formulari de Sala")]
    public UnityEngine.UI.InputField campNomSala;
    public UnityEngine.UI.Text missatgeInfo;

    // Variables estátiques per passar dades entre escenes
    public static string roomId { get; private set; }
    public static string roomCode { get; private set; }
    public static string nomSala { get; private set; }
    public static bool isSinglePlayer { get; private set; }

    // Variables per a la connexió Socket.io
    private SocketIO client;
    private bool altreJugadorEntrat = false;

    void Start()
    {
        // Obtenir el token des de AuthManager
        if (string.IsNullOrEmpty(AuthManager.token))
        {
            MostrarInfo("Sessió expirada. Torna al login.");
            Invoke("TornarAlLogin", 2f);
            return;
        }

        // Iniciar la connexió Socket.io per escoltar quan l'altre jugador entrí
        StartCoroutine(ConnectarSocket());
    }

    IEnumerator ConnectarSocket()
    {
        client = new SocketIO(urlServidor);

        // Escoltar quan l'altre jugador s'uneix a la sala
        client.On("jugadorEntrat", (data) =>
        {
            altreJugadorEntrat = true;
            MostrarInfo("L'altre jugador ha entrat! Començant partida...");
            Debug.Log("L'altre jugador s'ha unit a la sala");
        });

        yield return client.ConnectAsync();

        if (client.Connected)
        {
            Debug.Log("Connectat al socket del Lobby");
        }
    }

    // Crear una nova sala
    public void CrearSala()
    {
        string nom = campNomSala.text;

        if (string.IsNullOrEmpty(nom))
        {
            MostrarInfo("Siusplau, escriu un nom per a la sala.");
            return;
        }

        StartCoroutine(CrearSalaCoroutine(nom));
    }

    IEnumerator CrearSalaCoroutine(string nomSala)
    {
        MostrarInfo("Creant sala...");

        // Crear objecte petició
        RoomRequest peticio = new RoomRequest();
        peticio.nom_sala = nomSala;

        string jsonData = JsonUtility.ToJson(peticio);

        using (UnityWebRequest www = new UnityWebRequest(urlServidor + "/api/rooms", "POST"))
        {
            byte[] jsonToSend = new UTF8Encoding(true).GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + AuthManager.token);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resposta = www.downloadHandler.text;
                RoomResposta dades = JsonUtility.FromJson<RoomResposta>(resposta);

                roomId = dades.roomId;
                roomCode = dades.roomCode;
                LobbyManager.nomSala = nomSala;

                Debug.Log("Sala creada: " + nomSala + " (ID: " + roomId + ")");
                MostrarInfo("Sala creada! Esperant altre jugador...");

                // Esperar que l'altre jugador entrí abans d'anar a l'escena de joc
                yield return new WaitUntil(() => altreJugadorEntrat);
                
                SceneManager.LoadScene("Joc");
            }
            else
            {
                Debug.LogError("Error creant sala: " + www.error);
                MostrarInfo("Error al crear la sala.");
            }
        }
    }

    // Unir-se a una sala existent (per nom)
    public void UnirSala()
    {
        string nom = campNomSala.text;

        if (string.IsNullOrEmpty(nom))
        {
            MostrarInfo("Siusplau, escriu el nom de la sala.");
            return;
        }

        StartCoroutine(UnirSalaCoroutine(nom));
    }

    IEnumerator UnirSalaCoroutine(string nomSala)
    {
        MostrarInfo("Buscant sala...");

        using (UnityWebRequest www = UnityWebRequest.Get(urlServidor + "/api/rooms"))
        {
            www.SetRequestHeader("Authorization", "Bearer " + AuthManager.token);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resposta = www.downloadHandler.text;
                
                // JsonUtility no pot deserialitzar arrays directament
                // Fem servir un wrapper
                RoomListResponse salesWrapper = JsonUtility.FromJson<RoomListResponse>(resposta);

                // Buscar la sala pel nom
                RoomInfo salaTrobada = null;
                if (salesWrapper != null && salesWrapper.rooms != null)
                {
                    foreach (RoomInfo room in salesWrapper.rooms)
                    {
                        if (room.nom_sala == nomSala)
                        {
                            salaTrobada = room;
                            break;
                        }
                    }
                }

                if (salaTrobada != null)
                {
                    yield return UnirSalaPerId(salaTrobada.id, nomSala);
                }
                else
                {
                    MostrarInfo("La sala no existeix.");
                }
            }
            else
            {
                MostrarInfo("Error carregant sales.");
            }
        }
    }

    IEnumerator UnirSalaPerId(string idSala, string nomSala)
    {
        using (UnityWebRequest www = new UnityWebRequest(urlServidor + "/api/rooms/" + idSala + "/join", "POST"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + AuthManager.token);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                roomId = idSala;
                LobbyManager.nomSala = nomSala;

                Debug.Log("Unit a la sala: " + idSala);
                MostrarInfo("Unit a la sala! Començant partida...");
                
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene("Joc");
            }
            else
            {
                Debug.LogError("Error unint a sala: " + www.error);
                MostrarInfo("Error al unir-se a la sala.");
            }
        }
    }

    // Tornar a l'escena de login
    public void TornarAlLogin()
    {
        SceneManager.LoadScene("Login");
    }

    // Mostrar missatge informatiu
    void MostrarInfo(string missatge)
    {
        if (missatgeInfo != null)
        {
            missatgeInfo.text = missatge;
        }
    }

    // Classes per a les peticions
    [System.Serializable]
    public class RoomRequest
    {
        public string nom_sala;
    }

    // Classes per deserialitzar les respostes
    [System.Serializable]
    public class RoomResposta
    {
        public string roomId;
        public string roomCode;
    }

    // Wrapper per a la llista de sales (JsonUtility no suporta arrays)
    [System.Serializable]
    public class RoomListResponse
    {
        public RoomInfo[] rooms;
    }

    [System.Serializable]
    public class RoomInfo
    {
        public string id;
        public string nom_sala;
        public string codi_sala;
        public int jugadors_actuals;
    }
}
