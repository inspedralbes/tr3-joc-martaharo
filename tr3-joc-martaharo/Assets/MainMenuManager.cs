using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
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
                var dades = JsonConvert.DeserializeObject<SinglePlayerRoomResponse>(resposta);

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
    public void CrearSalaMultiplayer()
    {
        panellPrincipal.SetActive(false);
        panellCrearSala.SetActive(true);
        campNomSala.text = "";
        textCodiSala.text = "";
        missatgeCrear.text = "";
    }

    public void ConfirmarCrearSala()
    {
        string nom = campNomSala.text;

        if (string.IsNullOrEmpty(nom))
        {
            missatgeCrear.text = "Escriu un nom per a la sala.";
            missatgeCrear.color = Color.red;
            return;
        }

        StartCoroutine(CrearSalaCoroutine(nom));
    }

    IEnumerator CrearSalaCoroutine(string nom)
    {
        missatgeCrear.text = "Creant sala...";
        missatgeCrear.color = Color.white;

        string jsonData = JsonConvert.SerializeObject(new
        {
            nom_sala = nom
        });

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
                var dades = JsonConvert.DeserializeObject<RoomResponse>(resposta);

                roomId = dades.roomId;
                roomCode = dades.roomCode;
                nomSala = nom;
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

    public void ConfirmarUnirSala()
    {
        string codi = campCodiSala.text.ToUpper();

        if (string.IsNullOrEmpty(codi))
        {
            missatgeUnir.text = "Escriu el codi de la sala.";
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
                var sales = JsonConvert.DeserializeObject<RoomListResponse>(resposta);

                // Buscar la sala pel codi
                var salaTrobada = System.Array.Find(sales.rooms, s => s.codi_sala == codi);

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
                var error = JsonConvert.DeserializeObject<ErrorResponse>(errorResposta);
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
