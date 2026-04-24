using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

// Clase para saltar validaciones de certificado (util en algunos builds)
public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}

public class AuthManager : MonoBehaviour
{
    [Header("Configuració de Connexió")]
    public bool usarServidorRemot = true; 
    
    private string urlLocal = "http://localhost/api/auth"; 
    private string urlServidor = "http://204.168.209.55:8080/api/auth";

    [Header("Formulari de Login")]
    public TMP_InputField campUsuari;
    public TMP_InputField campContrasenya;
    public TextMeshProUGUI missatgeError;

    public static string token { get; private set; }
    public static string username { get; private set; }

    public void FerLogin()
    {
        string usuari = campUsuari.text;
        string contrasenya = campContrasenya.text;

        if (string.IsNullOrEmpty(usuari) || string.IsNullOrEmpty(contrasenya))
        {
            MostrarError("Escriu usuari i contrasenya!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(LoginCoroutine(usuari, contrasenya));
    }

    IEnumerator LoginCoroutine(string usuari, string contrasenya)
    {
        string baseUrl = usarServidorRemot ? urlServidor : urlLocal;
        string fullUrl = baseUrl.TrimEnd('/') + "/login";

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            MostrarError("Sense conexión a internet!");
            yield break;
        }

        MostrarInfo("Connectant a: " + fullUrl);
        
        LoginRequest peticio = new LoginRequest { username = usuari, password = contrasenya };
        string jsonData = JsonUtility.ToJson(peticio);

        using (UnityWebRequest www = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.certificateHandler = new BypassCertificate();
            
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("User-Agent", "UnityGame/1.0");

            // Timeout manual de 15 segundos
            www.timeout = 15;
            
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                LoginResposta dades = JsonUtility.FromJson<LoginResposta>(www.downloadHandler.text);
                token = dades.token;
                username = dades.username;

                MostrarExit("Benvingut/da " + username + "!");
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene("Menu");
            }
            else
            {
                Debug.LogError($"Error Login: {www.error} | Code: {www.responseCode}");
                if (www.responseCode == 401) MostrarError("Usuari o contrasenya malament");
                else MostrarError("Error de xarxa: " + (www.responseCode > 0 ? www.responseCode.ToString() : "Timeout"));
            }
        }
    }

    public void FerRegistre()
    {
        string usuari = campUsuari.text;
        string contrasenya = campContrasenya.text;

        if (string.IsNullOrEmpty(usuari) || contrasenya.Length < 4)
        {
            MostrarError("Usuari buit o pass curta!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(RegistreCoroutine(usuari, contrasenya));
    }

    IEnumerator RegistreCoroutine(string usuari, string contrasenya)
    {
        string baseUrl = usarServidorRemot ? urlServidor : urlLocal;
        string fullUrl = baseUrl.TrimEnd('/') + "/register";

        MostrarInfo("Creant compte a: " + fullUrl);
        
        LoginRequest peticio = new LoginRequest { username = usuari, password = contrasenya };
        string jsonData = JsonUtility.ToJson(peticio);

        using (UnityWebRequest www = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.certificateHandler = new BypassCertificate();

            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");

            www.timeout = 15;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                LoginResposta dades = JsonUtility.FromJson<LoginResposta>(www.downloadHandler.text);
                token = dades.token;
                username = dades.username;

                MostrarExit("Compte creat. Entrant...");
                yield return new WaitForSeconds(1.5f);
                SceneManager.LoadScene("Menu");
            }
            else
            {
                if (www.responseCode == 409) MostrarError("L'usuari ja existeix");
                else MostrarError("Error de xarxa: " + (www.responseCode > 0 ? www.responseCode.ToString() : "Error"));
            }
        }
    }

    void MostrarError(string missatge)
    {
        if (missatgeError != null)
        {
            missatgeError.color = Color.red;
            missatgeError.text = missatge;
        }
    }

    void MostrarExit(string missatge)
    {
        if (missatgeError != null)
        {
            missatgeError.color = Color.green;
            missatgeError.text = missatge;
        }
    }

    void MostrarInfo(string missatge)
    {
        if (missatgeError != null)
        {
            missatgeError.color = Color.white;
            missatgeError.text = missatge;
        }
    }

    public void TancarSessio()
    {
        token = null;
        username = null;
        SceneManager.LoadScene("Login");
    }

    [System.Serializable]
    public class LoginRequest { public string username; public string password; }

    [System.Serializable]
    public class LoginResposta { public string token; public string username; }
}