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
    [SerializeField] private string urlServidor = "http://204.168.209.55:3000/api/auth";

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
        if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p)) { MostrarError("Falten dades!"); return; }

        StopAllCoroutines();
        StartCoroutine(PeticioFinal(u, p, endpoint));
    }

    // Clase interna para saltar cualquier restricción de certificado en Builds
    public class BypassCertificate : CertificateHandler {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }

    IEnumerator PeticioFinal(string u, string p, string ep)
    {
        string baseUrl = usarServidorRemot ? urlServidor : urlLocal;
        string fullUrl = baseUrl.TrimEnd('/') + ep;

        MostrarInfo("Connectant a " + fullUrl + "...");
        
        string jsonData = JsonUtility.ToJson(new LoginRequest { username = u, password = p });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest www = new UnityWebRequest(fullUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            
            // CONFIGURACIÓN DE EMERGENCIA
            www.certificateHandler = new BypassCertificate();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"); // Engañar a firewalls
            www.timeout = 15;

            var op = www.SendWebRequest();
            float timer = 0;

            while (!op.isDone)
            {
                timer += Time.deltaTime;
                MostrarInfo($"Esperant resposta... {timer:F1}s");
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resRaw = www.downloadHandler.text;
                if (resRaw.Contains("token") || resRaw.Contains("success\":true")) {
                    LoginResposta res = JsonUtility.FromJson<LoginResposta>(resRaw);
                    token = res.token;
                    username = res.username;
                    MostrarExit("¡Dins! Carregant...");
                    yield return new WaitForSeconds(0.8f);
                    SceneManager.LoadScene("Menu");
                } else {
                    MostrarError("Error del servidor: " + resRaw);
                }
            }
            else
            {
                // Si llegamos aquí con un responseCode > 0, es que hay conexión pero error de lógica
                if (www.responseCode == 401 || www.responseCode == 404) MostrarError("Usuari/Pass incorrectes");
                else if (www.responseCode == 409) MostrarError("L'usuari ja existeix");
                else MostrarError("Error Xarxa: " + (www.responseCode > 0 ? www.responseCode.ToString() : www.error));
                
                Debug.LogWarning("Respuesta fallida: " + www.downloadHandler.text);
            }
        }
    }

    void MostrarError(string m) { if (missatgeError) { missatgeError.color = Color.red; missatgeError.text = m; } }
    void MostrarExit(string m) { if (missatgeError) { missatgeError.color = Color.green; missatgeError.text = m; } }
    void MostrarInfo(string m) { if (missatgeError) { missatgeError.color = Color.white; missatgeError.text = m; } }

    [System.Serializable] public class LoginRequest { public string username; public string password; }
    [System.Serializable] public class LoginResposta { public string token; public string username; public string success; }
}