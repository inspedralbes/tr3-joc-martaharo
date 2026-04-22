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
    [Header("Configuración del Servidor")]
    public string urlServidor = "http://204.168.209.55";
    public string puertoServidor = "3000";

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
    public static string roomId { get; set; }
    public static string roomCode { get; set; }
    public static string nomSala { get; private set; }
    public static bool isSinglePlayer { get; set; }
    public static string playerNumber { get; private set; }
    
    public static bool isHost;

    private SocketIO socketClient;

    void Start()
    {
        MostrarPanellPrincipal();
        ConnectarSocketPerErrors();
    }
    
    async void ConnectarSocketPerErrors()
    {
        string urlLimpia = urlServidor.Trim().TrimEnd('/');
        if (!string.IsNullOrEmpty(puertoServidor))
        {
            urlLimpia = urlLimpia + ":" + puertoServidor;
        }
        socketClient = new SocketIO(urlLimpia);
        
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

    public void JugarSol()
    {
        isSinglePlayer = true;
        roomId = "";
        roomCode = "";
        SceneManager.LoadScene("Joc_IA");
    }

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

        // URL de Seguridad: Trim y eliminar '/' final
        string urlBase = urlServidor.Trim().TrimEnd('/');
        if (!string.IsNullOrEmpty(puertoServidor))
        {
            urlBase = urlBase + ":" + puertoServidor;
        }
        string url = urlBase + "/api/rooms";

        // Debug de Headers
        Debug.Log("===========================================");
        Debug.Log("[MainMenuManager] Enviando POST a: " + url);
        Debug.Log("[MainMenuManager] urlServidor original: " + urlServidor);
        Debug.Log("[MainMenuManager] urlBase limpia: " + urlBase);
        
        if (AuthManager.token != null && AuthManager.token != "")
        {
            Debug.Log("[MainMenuManager] Token: PRESENTE");
        }
        else
        {
            Debug.Log("[MainMenuManager] Token: NO ESTÁ (se envía sin Authorization)");
        }
        Debug.Log("===========================================");

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = new UTF8Encoding(true).GetBytes("{}");
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // Login Opcional: Si NO hay token, envía sin header Authorization
            if (AuthManager.token != null && AuthManager.token != "")
            {
                www.SetRequestHeader("Authorization", "Bearer " + AuthManager.token);
            }

            yield return www.SendWebRequest();

            string respuesta = www.downloadHandler.text;
            long codigo = www.responseCode;

            Debug.Log("[MainMenuManager] HTTP Código: " + codigo);
            Debug.Log("[MainMenuManager] Respuesta: " + respuesta.Substring(0, Mathf.Min(200, respuesta.Length)));

            // Detección de HTML mejorada
            if (respuesta.Contains("<!DOCTYPE") || respuesta.Contains("<html") || respuesta.Contains("Cannot POST") || respuesta.Contains("Cannot"))
            {
                Debug.LogError("===========================================");
                Debug.LogError("ERROR: El Nginx no está redirigiendo al Backend. Revisa el default.conf");
                Debug.LogError("URL intentada: " + url);
                Debug.LogError("==============================");
                missatgeCrear.text = "Error: Nginx no conecta con Backend";
                missatgeCrear.color = Color.red;
                yield break;
            }

            if (www.result == UnityWebRequest.Result.Success && codigo >= 200 && codigo < 300)
            {
                RoomResponse dades = JsonUtility.FromJson<RoomResponse>(respuesta);
                
                if (dades != null && !string.IsNullOrEmpty(dades.roomId))
                {
                    roomId = dades.roomId;
                    roomCode = dades.roomCode;
                    isSinglePlayer = false;
                    isHost = true;

                    textCodiSala.text = "Codi: " + roomCode;
                    missatgeCrear.text = "Sala creada! Comparteix el codi amb el teu company.";
                    missatgeCrear.color = Color.green;

                    Debug.Log("[MainMenuManager] ✅ Sala creada: " + roomCode);
                    yield return new WaitForSeconds(1f);
                    SceneManager.LoadScene("Lobby");
                }
                else
                {
                    Debug.LogError("[MainMenuManager] Error: JSON inválido del servidor");
                    missatgeCrear.text = "Error al crear la sala";
                    missatgeCrear.color = Color.red;
                }
            }
            else
            {
                Debug.LogError("[MainMenuManager] Error HTTP: " + codigo + " - " + respuesta);
                missatgeCrear.text = "Error: Código " + codigo;
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
            Debug.Log("Codi copiat: " + roomCode);
        }
    }

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

        string urlBase = urlServidor.Trim().TrimEnd('/');
        if (!string.IsNullOrEmpty(puertoServidor))
        {
            urlBase = urlBase + ":" + puertoServidor;
        }
        string url = urlBase + "/api/rooms";

        Debug.Log("[MainMenuManager] Buscando sala con código: " + codi);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            if (AuthManager.token != null && AuthManager.token != "")
            {
                www.SetRequestHeader("Authorization", "Bearer " + AuthManager.token);
            }

            yield return www.SendWebRequest();

            string respuesta = www.downloadHandler.text;

            if (respuesta.Contains("<!DOCTYPE") || respuesta.Contains("<html"))
            {
                Debug.LogError("ERROR: El servidor respondió con HTML en /api/rooms");
                missatgeUnir.text = "Error de connexió";
                missatgeUnir.color = Color.red;
                yield break;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonFix = "{\"rooms\":" + respuesta + "}";
                RoomsWrapper wrapper = JsonUtility.FromJson<RoomsWrapper>(jsonFix);

                if (wrapper != null && wrapper.rooms != null)
                {
                    foreach (SalaInfo sala in wrapper.rooms)
                    {
                        if (sala.codi_sala == codi)
                        {
                            yield return UnirSalaPerId(sala.id, sala.codi_sala);
                            yield break;
                        }
                    }
                }

                missatgeUnir.text = "Aquesta sala no existeix";
                missatgeUnir.color = Color.red;
            }
            else
            {
                Debug.LogError("[MainMenuManager] Error HTTP: " + www.responseCode);
                missatgeUnir.text = "Error cercant sales";
                missatgeUnir.color = Color.red;
            }
        }
    }

    IEnumerator UnirSalaPerId(string idSala, string codi)
    {
        string urlBase = urlServidor.Trim().TrimEnd('/');
        if (!string.IsNullOrEmpty(puertoServidor))
        {
            urlBase = urlBase + ":" + puertoServidor;
        }
        string url = urlBase + "/api/rooms/" + idSala + "/join";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = new UTF8Encoding(true).GetBytes("{}");
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            if (AuthManager.token != null && AuthManager.token != "")
            {
                www.SetRequestHeader("Authorization", "Bearer " + AuthManager.token);
            }

            yield return www.SendWebRequest();

            string respuesta = www.downloadHandler.text;

            if (respuesta.Contains("<!DOCTYPE") || respuesta.Contains("<html"))
            {
                Debug.LogError("ERROR: No se pudo unir - HTML recibido");
                missatgeUnir.text = "Error de ruta";
                missatgeUnir.color = Color.red;
                yield break;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                roomId = idSala;
                roomCode = codi;
                isSinglePlayer = false;
                isHost = false;

                Debug.Log("[MainMenuManager] ✅ Unit a la sala: " + idSala);
                missatgeUnir.text = "Unit a la sala! Començant...";
                missatgeUnir.color = Color.green;

                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                Debug.LogError("[MainMenuManager] Error HTTP: " + www.responseCode);
                missatgeUnir.text = "Error al unir-se";
                missatgeUnir.color = Color.red;
            }
        }
    }

    public void Tornar()
    {
        MostrarPanellPrincipal();
    }

    // Classes [System.Serializable] dentro de la clase principal
    [System.Serializable]
    public class RoomResponse
    {
        public string roomId;
        public string roomCode;
    }

    [System.Serializable]
    public class SalaInfo
    {
        public string id;
        public string nom_sala;
        public string codi_sala;
    }

    [System.Serializable]
    public class RoomsWrapper
    {
        public SalaInfo[] rooms;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string error;
    }
}