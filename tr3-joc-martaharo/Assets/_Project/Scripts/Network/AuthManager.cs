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
    [SerializeField] private string urlServidor = "http://204.168.209.55/api/auth";

    [Header("Formulari de Login")]
    public TMP_InputField campUsuari;
    public TMP_InputField campContrasenya;
    public TextMeshProUGUI missatgeError;

    public static string token { get; private set; }
    public static string username { get; private set; }

    private void Start() {
        Debug.Log(">>> AuthManager Iniciat i a l'espera...");
        MostrarInfo("Esperant dades..."); 
    }

    public void FerLogin() { 
        Debug.Log(">>> BOTÓ LOGIN PREMUT!");
        ValidarIExecutar("/login"); 
    }
    
    public void FerRegistre() { 
        Debug.Log(">>> BOTÓ REGISTRE PREMUT!");
        ValidarIExecutar("/register"); 
    }

    private void ValidarIExecutar(string endpoint)
    {
        // Forzamos el mensaje antes de cualquier otra lógica
        if (missatgeError) {
            missatgeError.color = Color.yellow;
            missatgeError.text = "Processant clic...";
        }

        string u = campUsuari != null ? campUsuari.text.Trim() : "";
        string p = campContrasenya != null ? campContrasenya.text.Trim() : "";

        if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p)) {
            MostrarError("Falten dades!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(PeticioFinal(u, p, endpoint));
    }

    IEnumerator PeticioFinal(string u, string p, string ep)
    {
        string baseUrl = usarServidorRemot ? urlServidor : urlLocal;
        string fullUrl = baseUrl.TrimEnd('/') + ep;

        Debug.Log(">>> LANZANDO PETIICIÓN A: " + fullUrl);
        MostrarInfo("Intentant connectar a " + fullUrl);
        
        string jsonData = JsonUtility.ToJson(new LoginRequest { username = u, password = p });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest www = new UnityWebRequest(fullUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 20;

            var op = www.SendWebRequest();
            float inici = Time.realtimeSinceStartup;

            while (!op.isDone)
            {
                float transcorregut = Time.realtimeSinceStartup - inici;
                MostrarInfo($"Esperant resposta... {transcorregut:F1}s");
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                LoginResposta res = JsonUtility.FromJson<LoginResposta>(www.downloadHandler.text);
                token = res.token;
                username = res.username;
                MostrarExit("¡Correcte!");
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene("Menu");
            }
            else
            {
                Debug.LogError($">>> ERROR XARXA: {www.error}");
                MostrarError("Error: " + (www.responseCode > 0 ? www.responseCode.ToString() : www.error));
            }
        }
    }

    void MostrarError(string m) { if (missatgeError) { missatgeError.color = Color.red; missatgeError.text = m; } }
    void MostrarExit(string m) { if (missatgeError) { missatgeError.color = Color.green; missatgeError.text = m; } }
    void MostrarInfo(string m) { if (missatgeError) { missatgeError.color = Color.white; missatgeError.text = m; } }

    [System.Serializable] public class LoginRequest { public string username; public string password; }
    [System.Serializable] public class LoginResposta { public string token; public string username; }
}