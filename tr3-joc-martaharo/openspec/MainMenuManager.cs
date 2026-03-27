// =================================================================================
// SCRIPT: MainMenuManager
// UBICACIÓ: Assets/_Project/Scripts/UI/
// DESCRIPCIÓ: Menú principal amb opcions Single Player i Multiplayer
// =================================================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    // URL del servidor Node.js
    private string urlServidor = "http://localhost:3000";

    [Header("Panells del Menú")]
    public GameObject panellPrincipal;
    public GameObject panellCrearSala;
    public GameObject panellUnirSala;

    [Header("Formulari Crear Sala")]
    public TMP_InputField campNomSala;
    public TextMeshProUGUI textCodiSala;
    public TextMeshProUGUI missatgeCrear;

    [Header("Formulari Unir-se a Sala")]
    public TMP_InputField campCodiSala;
    public TextMeshProUGUI missatgeUnir;

    // Variables estátiques per passar dades entre escenes
    public static string roomId { get; private set; }
    public static string roomCode { get; private set; }
    public static string nomSala { get; private set; }
    public static bool isSinglePlayer { get; private set; }

    void Start()
    {
        // Mostrar panell principal
        MostrarPanellPrincipal();
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
        
        campNomSala.text = "";
        textCodiSala.text = "";
        campCodiSala.text = "";
        missatgeCrear.text = "";
        missatgeUnir.text = "";
    }

    // Botó Crear - sense demanar nom
    public void ConfirmarCrearSala()
    {
        StartCoroutine(CrearSalaCoroutine());
    }

    IEnumerator CrearSalaCoroutine()
    {
        missatgeCrear.text = "Creant sala...";
        missatgeCrear.color = Color.white;

        using (UnityWebRequest www = new UnityWebRequest(urlServidor + "/api/sales/crear", "POST"))
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
            }
            else
            {
                Debug.LogError("Error creant sala: " + www.error);
                missatgeCrear.text = "Error al crear la sala.";
                missatgeCrear.color = Color.red;
            }
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

    // Botó Unir - amb validació de 5 lletres
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
                RoomListResponse sales = JsonUtility.FromJson<RoomListResponse>(resposta);

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
                    missatgeUnir.text = "Sala no trobada. Comprova el codi.";
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
                SceneManager.LoadScene("Joc");
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