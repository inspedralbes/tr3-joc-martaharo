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
        Debug.Log(">>> EXECUTANT ACCIÓ <<<");
        Debug.Log(">>> BOTÓN LOGIN CLICADO <<<");
        
        string usuari = campUsuari.text;
        string contrasenya = campContrasenya.text;

        if (string.IsNullOrEmpty(usuari))
        {
            MostrarError("Escriu un nom d'usuari!");
            return;
        }

        if (string.IsNullOrEmpty(contrasenya))
        {
            MostrarError("Escriu la contrasenya!");
            return;
        }

        StartCoroutine(LoginCoroutine(usuari, contrasenya));
    }

    IEnumerator LoginCoroutine(string usuari, string contrasenya)
    {
        MostrarError("Connectant...");
        
        string baseUrl = usarServidorRemot ? urlServidor : urlLocal;
        string fullUrl = baseUrl.TrimEnd('/') + "/login";
        
        Debug.Log(">>> INTENTANT LOGIN A: " + fullUrl);

        // Definició correcta del jsonData abans del seu ús
        LoginRequest peticio = new LoginRequest();
        peticio.username = usuari;
        peticio.password = contrasenya;
        string jsonData = JsonUtility.ToJson(peticio);

        using (UnityWebRequest www = new UnityWebRequest(fullUrl, "POST"))
        {
            www.timeout = 60;
            www.useHttpContinue = false;
            
            byte[] jsonToSend = new UTF8Encoding(true).GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json"); // Afegit per a compatibilitat total

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resposta = www.downloadHandler.text;
                LoginResposta dadesResposta = JsonUtility.FromJson<LoginResposta>(resposta);

                token = dadesResposta.token;
                username = dadesResposta.username;

                Debug.Log("Login correcte! Benvingut/da: " + username);

                MostrarExit("Sessió iniciada. Benvingut/da!");
                SceneManager.LoadScene("Menu");
            }
            else
            {
                string respostaError = www.downloadHandler.text;
                Debug.LogError($">>> ERROR TÉCNIC LOGIN: URL={fullUrl} | Codi={www.responseCode} | Error={www.error} | Reachability={Application.internetReachability}");
                Debug.LogError($">>> COS DE LA RESPOSTA: {respostaError}");
                
                string errorMsg;
                if (www.responseCode == 401)
                {
                    errorMsg = "Usuari o contrasenya incorrectes";
                }
                else
                {
                    errorMsg = "Error de connexió amb el servidor";
                }

                MostrarError(errorMsg);
            }
        }
    }

    public void FerRegistre()
    {
        Debug.Log(">>> EXECUTANT ACCIÓ <<<");
        
        string usuari = campUsuari.text;
        string contrasenya = campContrasenya.text;

        if (string.IsNullOrEmpty(usuari))
        {
            MostrarError("Escriu un nom d'usuari!");
            return;
        }

        if (string.IsNullOrEmpty(contrasenya))
        {
            MostrarError("Escriu la contrasenya!");
            return;
        }

        if (contrasenya.Length < 4)
        {
            MostrarError("La contrasenya ha de tenir 4 caràcters mínim.");
            return;
        }

        StartCoroutine(RegistreCoroutine(usuari, contrasenya));
    }

    IEnumerator RegistreCoroutine(string usuari, string contrasenya)
    {
        MostrarError("Creant compte...");
        
        string baseUrl = usarServidorRemot ? urlServidor : urlLocal;
        string fullUrl = baseUrl.TrimEnd('/') + "/register";
        
        Debug.Log(">>> INTENTANT REGISTRE A: " + fullUrl);

        // Definició correcta del jsonData abans del seu ús
        LoginRequest peticio = new LoginRequest();
        peticio.username = usuari;
        peticio.password = contrasenya;
        string jsonData = JsonUtility.ToJson(peticio);

        using (UnityWebRequest www = new UnityWebRequest(fullUrl, "POST"))
        {
            www.timeout = 60;
            www.useHttpContinue = false;
            
            byte[] jsonToSend = new UTF8Encoding(true).GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resposta = www.downloadHandler.text;
                LoginResposta dadesResposta = JsonUtility.FromJson<LoginResposta>(resposta);

                token = dadesResposta.token;
                username = dadesResposta.username;

                Debug.Log("Registre correcte! Benvingut/da: " + username);
                
                MostrarExit("Usuari creat correctament!");
                
                campUsuari.text = "";
                campContrasenya.text = "";
            }
            else
            {
                string respostaError = www.downloadHandler.text;
                Debug.LogError($">>> ERROR TÉCNIC REGISTRE: URL={fullUrl} | Codi={www.responseCode} | Error={www.error} | Reachability={Application.internetReachability}");
                Debug.LogError($">>> COS DE LA RESPOSTA: {respostaError}");
                
                string errorMsg;
                if (www.responseCode == 409)
                {
                    errorMsg = "Aquest usuari ja existeix";
                }
                else
                {
                    errorMsg = "Error de connexió amb el servidor";
                }

                MostrarError(errorMsg);
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

    public void TancarSessio()
    {
        token = null;
        username = null;
        SceneManager.LoadScene("Login");
    }

    [System.Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    public class LoginResposta
    {
        public string token;
        public string username;
    }
}