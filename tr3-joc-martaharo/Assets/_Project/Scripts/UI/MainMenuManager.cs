using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class MainMenuManager : MonoBehaviour
{
    [Header("Los Tus Paneles (Sin cambios)")]
    public GameObject PanellPrincipal; 
    public GameObject PanellCrear;     
    public GameObject PanellUnir;      

    [Header("UI Textos")]
    public TextMeshProUGUI TextCodi;
    public TMP_InputField InputCodigo;

    public static string roomId;
    public static string roomCode;
    public static bool isSinglePlayer = false;
    public static bool isHost = false;

    void Start() {
        if(PanellPrincipal) PanellPrincipal.SetActive(true);
        if(PanellCrear) PanellCrear.SetActive(false);
        if(PanellUnir) PanellUnir.SetActive(false);
    }

    public void SeleccionarIndividual() {
        isSinglePlayer = true;
        isHost = true;
        SceneManager.LoadScene("Joc_IA");
    }

    public void ActivarPanelesMultiplayer() {
        // Encendemos tus dos paneles
        if(PanellCrear) PanellCrear.SetActive(true);
        if(PanellUnir) PanellUnir.SetActive(true);
        
        // APAGAMOS el principal para que no moleste
        if(PanellPrincipal) PanellPrincipal.SetActive(false);
    }

    public void BotonCrearDefinitivo() {
        isHost = true;
        isSinglePlayer = false;
        StartCoroutine(CrearSalaCoroutine());
    }

    public void BotonUnirDefinitivo() {
        isHost = false;
        isSinglePlayer = false;
        string codi = InputCodigo.text.Trim().ToUpper();
        if(!string.IsNullOrEmpty(codi)) StartCoroutine(UnirSalaCoroutine(codi));
    }

    public void TornarInici() {
        Start(); // Vuelve a poner todo como al principio
    }

    IEnumerator CrearSalaCoroutine() {
        using (UnityWebRequest www = new UnityWebRequest("http://204.168.209.55:3000/api/rooms", "POST")) {
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success) {
                RoomResponse data = JsonUtility.FromJson<RoomResponse>(www.downloadHandler.text);
                roomCode = data.roomCode;
                roomId = data.roomId;
                TextCodi.text = "CODI: " + roomCode;
                if (SocketNetworkManager.Instance != null) SocketNetworkManager.Instance.Initialize(roomId);
                yield return new WaitForSeconds(2.5f);
                SceneManager.LoadScene("Lobby");
            }
        }
    }

    IEnumerator UnirSalaCoroutine(string codi) {
        string json = "{\"roomCode\":\"" + codi + "\"}";
        using (UnityWebRequest www = new UnityWebRequest("http://204.168.209.55:3000/api/rooms/join", "POST")) {
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success) {
                RoomResponse data = JsonUtility.FromJson<RoomResponse>(www.downloadHandler.text);
                roomId = data.roomId;
                if (SocketNetworkManager.Instance != null) SocketNetworkManager.Instance.Initialize(roomId);
                SceneManager.LoadScene("Lobby");
            }
        }
    }

    [System.Serializable] public class RoomResponse { public string roomId; public string roomCode; }
}