using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System.Text;

public class AuthManager : MonoBehaviour
{
    [Header("Configuració de Connexió")]
    public bool usarServidorRemot = true; 
    
    // Cambiamos a Puerto 80 (estándar) para evitar bloqueos de Windows
    [SerializeField] private string urlLocal = "http://localhost:3000/api/auth"; 
    [SerializeField] private string urlServidor = "http://204.168.209.55/api/auth";

    [Header("Formulari de Login")]
    public TMP_InputField campUsuari;
    public TMP_InputField campContrasenya;
    public TextMeshProUGUI missatgeError;

    public static string token { get; private set; }
    public static string username { get; private set; }

    public void FerLogin() { _ = PeticioAsync("/login"); }
    public void FerRegistre() { _ = PeticioAsync("/register"); }

    async Task PeticioAsync(string endpoint)
    {
        string u = campUsuari.text.Trim();
        string p = campContrasenya.text.Trim();

        if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p)) {
            MostrarError("Escriu usuari i pass!");
            return;
        }

        MostrarInfo("Connectant...");

        string baseUrl = usarServidorRemot ? urlServidor : urlLocal;
        string fullUrl = baseUrl.TrimEnd('/') + endpoint;
        string jsonData = JsonUtility.ToJson(new LoginRequest { username = u, password = p });

        using (UnityWebRequest www = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 15; // Timeout real de 15 segundos

            var operation = www.SendWebRequest();

            // Esperar de forma asíncrona (más estable que corrutinas en builds)
            while (!operation.isDone) {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resRaw = www.downloadHandler.text;
                if (resRaw.Contains("token")) {
                    LoginResposta res = JsonUtility.FromJson<LoginResposta>(resRaw);
                    token = res.token;
                    username = res.username;
                    MostrarExit("¡Correcte!");
                    await Task.Delay(1000);
                    SceneManager.LoadScene("Menu");
                } else {
                    MostrarError("Error de dades");
                }
            }
            else
            {
                string desc = www.error;
                if (www.responseCode == 401) MostrarError("Usuari/Pass malament");
                else if (www.responseCode == 409) MostrarError("Ja existeix l'usuari");
                else MostrarError("Error de Xarxa: " + (www.responseCode > 0 ? www.responseCode.ToString() : "Timeout"));
            }
        }
    }

    void MostrarError(string m) { if (missatgeError) { missatgeError.color = Color.red; missatgeError.text = m; } }
    void MostrarExit(string m) { if (missatgeError) { missatgeError.color = Color.green; missatgeError.text = m; } }
    void MostrarInfo(string m) { if (missatgeError) { missatgeError.color = Color.white; missatgeError.text = m; } }

    [System.Serializable] public class LoginRequest { public string username; public string password; }
    [System.Serializable] public class LoginResposta { public string token; public string username; }
}