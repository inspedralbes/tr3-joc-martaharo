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
    
    [SerializeField] private string urlLocal = "http://localhost:3000/api/auth"; 
    [SerializeField] private string urlServidor = "http://204.168.209.55:8080/api/auth";

    [Header("Formulari de Login")]
    public TMP_InputField campUsuari;
    public TMP_InputField campContrasenya;
    public TextMeshProUGUI missatgeError;

    public static string token { get; private set; }
    public static string username { get; private set; }

    public void FerLogin() { ValidarIExecutar("/login"); }
    public void FerRegistre() { ValidarIExecutar("/register"); }

    private void ValidarIExecutar(string endpoint)
    {
        string u = campUsuari.text.Trim();
        string p = campContrasenya.text.Trim();

        if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p)) {
            MostrarError("Falten dades!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ExecutarPeticio(u, p, endpoint));
    }

    IEnumerator ExecutarPeticio(string u, string p, string ep)
    {
        string baseUrl = usarServidorRemot ? urlServidor : urlLocal;
        string fullUrl = baseUrl.TrimEnd('/') + ep;

        MostrarInfo("Connectant...");
        
        string jsonData = JsonUtility.ToJson(new LoginRequest { username = u, password = p });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest www = new UnityWebRequest(fullUrl, "POST");
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.timeout = 15;

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string raw = www.downloadHandler.text;
            // VALIDACIÓN MANUAL SIN TRY-CATCH PARA EVITAR ERRORES DE COMPILACIÓN
            if (!string.IsNullOrEmpty(raw) && raw.Contains("token"))
            {
                LoginResposta res = JsonUtility.FromJson<LoginResposta>(raw);
                token = res.token;
                username = res.username;

                MostrarExit("Correcte! Entrant...");
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene("Menu");
            }
            else
            {
                MostrarError("Error en resposta del servidor");
            }
        }
        else
        {
            if (www.responseCode == 401) MostrarError("Usuari/Pass incorrectes");
            else if (www.responseCode == 409) MostrarError("L'usuari ja existeix");
            else MostrarError("Error de xarxa: " + (www.responseCode > 0 ? www.responseCode.ToString() : "Off"));
        }
        
        www.Dispose();
    }

    void MostrarError(string m) { if (missatgeError) { missatgeError.color = Color.red; missatgeError.text = m; } }
    void MostrarExit(string m) { if (missatgeError) { missatgeError.color = Color.green; missatgeError.text = m; } }
    void MostrarInfo(string m) { if (missatgeError) { missatgeError.color = Color.white; missatgeError.text = m; } }

    [System.Serializable] public class LoginRequest { public string username; public string password; }
    [System.Serializable] public class LoginResposta { public string token; public string username; }
}