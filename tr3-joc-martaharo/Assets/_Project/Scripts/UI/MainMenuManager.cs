// =================================================================================
// SCRIPT: MainMenuManager
// UBICACIÓ: Assets/_Project/Scripts/UI/
// DESCRIPCIÓ: Menú principal amb opcions Single Player i Multiplayer
// =================================================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using System.Collections;
using System.Text;
using TMPro;
using SocketIOClient;

public class MainMenuManager : MonoBehaviour
{
    // URL del servidor Node.js
    private string urlServidor = "http://localhost:3000";

    [Header("Panells del Menú")]
    public GameObject panellPrincipal;
    public GameObject panellCrearSala;
    public GameObject panellUnirSala;

    [Header("Formulari Crear Sala")]
    public TextMeshProUGUI textCodiSala;
    public TextMeshProUGUI missatgeCrear;

    [Header("Formulari Unir-se a Sala")]
    public TMP_InputField campCodiSala;
    public TextMeshProUGUI missatgeUnir;
    public Label labelError;

    // Variables estátiques per passar dades entre escenes
    public static string roomId { get; private set; }
    public static string roomCode { get; private set; }
    public static string nomSala { get; private set; }
    public static bool isSinglePlayer { get; private set; }
    public static string playerNumber { get; private set; }

    // Socket.io client per escoltar errors
    private SocketIO socketClient;

    void Start()
    {
        // Mostrar panell principal
        MostrarPanellPrincipal();
        
        // Iniciar Socket.io per escoltar errors d'unió
        ConnectarSocketPerErrors();
    }
    
    async void ConnectarSocketPerErrors()
    {
        socketClient = new SocketIO(urlServidor);
        
        // Escoltar errors d'unió a sala
        socketClient.On("joinError", response => {
            string errorMsg = response.GetValue<string>();
            UnityMainThreadDispatcher.Instance.Enqueue(() => {
                if (labelError != null)
                {
                    if (errorMsg.Contains("full"))
                    {
                        labelError.text = "SALA PLENA";
                    }
                    else if (errorMsg.Contains("not found") || errorMsg.Contains("no trobada"))
                    {
                        labelError.text = "SALA NO TROBADA";
                    }
                    else
                    {
                        labelError.text = "CODI INCORRECTE";
                    }
                }
                else
                {
                    // Fallback a TMP
                    missatgeUnir.text = errorMsg.Contains("full") ? "SALA PLENA" : "CODI INCORRECTE";
                    missatgeUnir.color = Color.red;
                }
            });
        });
        
        await socketClient.ConnectAsync();
    }

    public void MostrarPanellPrincipal()
    {
        panellPrincipal.SetActive(true);
        panellCrearSala.SetActive(false);
        panellUnirSala.SetActive(false);
    }

    // Botó: Jugar Sol (Single Player)
    public void JugarSol()
    {
        StartCoroutine(CrearPartidaSinglePlayer());
    }

    IEnumerator CrearPartidaSinglePlayer()
    {
        isSinglePlayer = true;
        
        using (UnityWebRequest www = new UnityWebRequest(urlServidor + "/api/rooms/single", "POST"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + AuthManager.token);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resposta = www.downloadHandler.text;
                SinglePlayerRoomResponse dades = JsonUtility.FromJson<SinglePlayerRoomResponse>(resposta);

                roomId = dades.roomId;
                roomCode = dades.roomCode;
                nomSala = dades.room.nom_sala;

                Debug.Log("Partida Single Player creada: " + roomCode);
                
                // Carregar escena de joc directament
                SceneManager.LoadScene("Joc");
            }
            else
            {
                Debug.LogError("Error creant partida single player: " + www.error);
                missatgeCrear.text = "Error al crear la partida. Torna a intentar.";
                missatgeCrear.color = Color.red;
            }
        }
    }

    // Botó: Crear Sala Multiplayer
    // Mostra els dos panells (Crear i Unir-se) simultàniament
    public void CrearSalaMultiplayer()
    {
        panellPrincipal.SetActive(false);
        panellCrearSala.SetActive(true);
        panellUnirSala.SetActive(true);
        
        textCodiSala.text = "";
        campCodiSala.text = "";
        missatgeCrear.text = "";
        missatgeUnir.text = "";
    }

    public void ConfirmarCrearSala()
    {
        StartCoroutine(CrearSalaCoroutine());
    }

    IEnumerator CrearSalaCoroutine()
    {
        missatgeCrear.text = "Creant sala...";
        missatgeCrear.color = Color.white;

        using (UnityWebRequest www = new UnityWebRequest(urlServidor + "/api/rooms", "POST"))
        {
            byte[] jsonToSend = new UTF8Encoding(true).GetBytes("{}");
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + AuthManager.token);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resposta = www.downloadHandler.text;
                RoomResponse dades = JsonUtility.FromJson<RoomResponse>(resposta);

                roomId = dades.roomId;
                roomCode = dades.roomCode;
                isSinglePlayer = false;

                textCodiSala.text = "Codi: " + roomCode;
                missatgeCrear.text = "Sala creada! Comparteix el codi amb el teu company.";
                missatgeCrear.color = Color.green;

                Debug.Log("Sala multiplayer creada: " + roomCode);

                yield return new WaitForSeconds(2f);
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                Debug.LogError("Error creant sala: " + www.error);
                missatgeCrear.text = "Error al crear la sala.";
                missatgeCrear.color = Color.red;
            }
        }
    }

    public void CopiarCodiAlPortapapers()
    {
        if (!string.IsNullOrEmpty(roomCode))
        {
            GUIUtility.systemCopyBuffer = roomCode;
            missatgeCrear.text = "Codi copiat!";
            missatgeCrear.color = Color.yellow;
            Debug.Log("Codi copiat al portapapers: " + roomCode);
        }
    }

    IEnumerator RestaurarMissatgeCrear()
    {
        yield return new WaitForSeconds(1f);
        if (!string.IsNullOrEmpty(roomCode))
        {
            missatgeCrear.text = "Sala creada! Comparteix el codi amb el teu company.";
            missatgeCrear.color = Color.green;
        }
    }

    // Botó: Unir-se a Sala
    public void UnirSalaMultiplayer()
    {
        panellPrincipal.SetActive(false);
        panellUnirSala.SetActive(true);
        campCodiSala.text = "";
        missatgeUnir.text = "";
    }

    public void ConfirmarUnirSala()
    {
        string codi = campCodiSala.text.ToUpper();

        if (string.IsNullOrEmpty(codi))
        {
            missatgeUnir.text = "Escriu el codi de la sala.";
            missatgeUnir.color = Color.red;
            return;
        }

        if (codi.Length != 5)
        {
            missatgeUnir.text = "El codi ha de tenir 5 lletres.";
            missatgeUnir.color = Color.red;
            return;
        }

        StartCoroutine(BuscarSalaPerCodi(codi));
    }

    IEnumerator BuscarSalaPerCodi(string codi)
    {
        missatgeUnir.text = "Buscant sala...";
        missatgeUnir.color = Color.white;

        using (UnityWebRequest www = UnityWebRequest.Get(urlServidor + "/api/rooms"))
        {
            www.SetRequestHeader("Authorization", "Bearer " + AuthManager.token);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resposta = www.downloadHandler.text;
                Debug.Log("Respuesta del servidor (Llista sales): " + resposta);

                RoomListResponse sales = null;
                try 
                {
                    sales = JsonUtility.FromJson<RoomListResponse>(resposta);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error al parsejar JSON de sales: " + e.Message);
                    missatgeUnir.text = "Error de format al servidor.";
                    missatgeUnir.color = Color.red;
                    yield break;
                }

                // Buscar la sala pel codi
                RoomItem salaTrobada = null;
                if (sales != null && sales.rooms != null)
                {
                    foreach (RoomItem room in sales.rooms)
                    {
                        if (room.codi_sala == codi)
                        {
                            salaTrobada = room;
                            break;
                        }
                    }
                }

                if (salaTrobada != null)
                {
                    yield return UnirSalaPerId(salaTrobada.id, salaTrobada.nom_sala);
                }
                else
                {
                    missatgeUnir.text = "Aquesta sala no existeix";
                    missatgeUnir.color = Color.red;
                }
            }
            else
            {
                missatgeUnir.text = "Error cercant sales.";
                missatgeUnir.color = Color.red;
            }
        }
    }

    IEnumerator UnirSalaPerId(string idSala, string nom)
    {
        using (UnityWebRequest www = new UnityWebRequest(urlServidor + "/api/rooms/" + idSala + "/join", "POST"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + AuthManager.token);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                roomId = idSala;
                roomCode = nom;
                nomSala = nom;
                isSinglePlayer = false;

                Debug.Log("Unit a la sala: " + idSala);
                missatgeUnir.text = "Unit a la sala! Començant...";
                missatgeUnir.color = Color.green;

                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                string errorResposta = www.downloadHandler.text;
                ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(errorResposta);
                missatgeUnir.text = error.error;
                missatgeUnir.color = Color.red;
            }
        }
    }

    // Botó: Tornar
    public void Tornar()
    {
        MostrarPanellPrincipal();
    }

    // Classes per a les peticions
    [System.Serializable]
    public class RoomRequest
    {
        public string nom_sala;
    }

    // Classes per deserialitzar respostes
    [System.Serializable]
    public class SinglePlayerRoomResponse
    {
        public string roomId;
        public string roomCode;
        public RoomData room;
    }

    [System.Serializable]
    public class RoomResponse
    {
        public string roomId;
        public string roomCode;
    }

    [System.Serializable]
    public class RoomData
    {
        public string nom_sala;
    }

    [System.Serializable]
    public class RoomListResponse
    {
        public RoomItem[] rooms;
    }

    [System.Serializable]
    public class RoomItem
    {
        public string id;
        public string nom_sala;
        public string codi_sala;
        public int jugadors_actuals;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string error;
    }
}
