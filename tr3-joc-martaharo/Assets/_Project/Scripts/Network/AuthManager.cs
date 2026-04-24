using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AuthManager : MonoBehaviour
{
    [Header("Configuració de Connexió")]
    public bool usarServidorRemot = true; 
    
    [SerializeField] private string urlLocal = "http://localhost/api/auth"; 
    [SerializeField] private string urlServidor = "http://204.168.209.55:8080/api/auth";

    [Header("Formulari de Login")]
    public TMP_InputField campUsuari;
    public TMP_InputField campContrasenya;
    public TextMeshProUGUI missatgeError;

    // Token y username accesibles globalmente
    public static string token { get; private set; }
    public static string username { get; private set; }

    public void FerLogin()
    {
        string u = campUsuari.text.Trim();
        string p = campContrasenya.text.Trim();

        if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
        {
            MostrarError("Falten dades!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ExecutarPeticio(u, p, "/login"));
    }

    public void FerRegistre()
    {
        string u = campUsuari.text.Trim();
        string p = campContrasenya.text.Trim();

        if (string.IsNullOrEmpty(u) || p.Length < 4)
        {
            MostrarError("Usuari buit o pass curta!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ExecutarPeticio(u, p, "/register"));
    }

    IEnumerator ExecutarPeticio(string usuari, string contrasenya, string endpoint)
    {
        string baseUrl = usarServidorRemot ? urlServidor : urlLocal;
        string fullUrl = baseUrl.TrimEnd('/') + endpoint;

        MostrarInfo("Connectant...");
        
        LoginRequest dadesPeticio = new LoginRequest { username = usuari, password = contrasenya };
        string jsonData = JsonUtility.ToJson(dadesPeticio);

        // CREACIÓN DE LA PETICIÓN (MÉTODO MÁS ROBUSTO PARA BUILDS)
        UnityWebRequest www = new UnityWebRequest(fullUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.timeout = 10; // 10 segundos máximo

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            try {
                LoginResposta resposta = JsonUtility.FromJson<LoginResposta>(www.downloadHandler.text);
                token = resposta.token;
                username = resposta.username;

                MostrarExit("Correcte! Entrant...");
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene("Menu");
            } catch {
                MostrarError("Error processant resposta del servidor");
            }
        }
        else
        {
            // MANEJO DE ERRORES ESPECÍFICOS
            if (www.responseCode == 401) MostrarError("Usuari o password incorrectes");
            else if (www.responseCode == 409) MostrarError("L'usuari ja existeix");
            else if (www.result == UnityWebRequest.Result.ConnectionError) MostrarError("Error de xarxa: Comprova el Firewall");
            else MostrarError("Error: " + (www.responseCode > 0 ? "Servidor Down (" + www.responseCode + ")" : "Timeout/No Internet"));
        }
        
        www.Dispose();
    }

    void MostrarError(string missatge)
    {
        if (missatgeError != null) {
            missatgeError.color = Color.red;
            missatgeError.text = missatge;
        }
    }

    void MostrarExit(string missatge)
    {
        if (missatgeError != null) {
            missatgeError.color = Color.green;
            missatgeError.text = missatge;
        }
    }

    void MostrarInfo(string missatge)
    {
        if (missatgeError != null) {
            missatgeError.color = Color.white;
            missatgeError.text = missatge;
        }
    }

    [System.Serializable]
    public class LoginRequest { public string username; public string password; }

    [System.Serializable]
    public class LoginResposta { public string token; public string username; }
}