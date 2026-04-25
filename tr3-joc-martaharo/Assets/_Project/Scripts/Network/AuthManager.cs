using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AuthManager : MonoBehaviour
{
    [Header("Configuració de Xarxa")]
    public bool usarServidorRemot = true; 
    [SerializeField] private string urlLocal = "http://localhost:3000/api/auth"; 
    [SerializeField] private string urlRemota = "http://204.168.209.55:3000/api/auth";

    [Header("Interfície UI")]
    public TMP_InputField campUsuari;
    public TMP_InputField campContrasenya;
    public TextMeshProUGUI missatgeError;

    public static string token;
    public static string username;

    public void FerLogin() { StartCoroutine(ExecutarPeticio("/login")); }
    public void FerRegistre() { StartCoroutine(ExecutarPeticio("/register")); }

    IEnumerator ExecutarPeticio(string endpoint)
    {
        string u = campUsuari.text.Trim();
        string p = campContrasenya.text.Trim();

        if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p)) {
            ActualitzarUI("Falten dades", Color.red);
            yield break;
        }

        string baseUrl = usarServidorRemot ? urlRemota : urlLocal;
        string fullUrl = baseUrl.TrimEnd('/') + endpoint;

        ActualitzarUI("Connectant...", Color.white);

        // USAMOS LA CLASE SERIALIZABLE (Importante para evitar Error 400)
        UserRequest requestData = new UserRequest { username = u, password = p };
        string json = JsonUtility.ToJson(requestData);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(fullUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 15;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resRaw = www.downloadHandler.text;
                if (resRaw.Contains("token")) {
                    LoginResposta res = JsonUtility.FromJson<LoginResposta>(resRaw);
                    token = res.token;
                    username = res.username;
                    ActualitzarUI("¡Dins!", Color.green);
                    yield return new WaitForSeconds(0.8f);
                    SceneManager.LoadScene("Menu");
                } else {
                    ActualitzarUI("Error de dades", Color.red);
                }
            }
            else
            {
                if (www.responseCode == 401) ActualitzarUI("Usuari o password incorrectes", Color.red);
                else if (www.responseCode == 404) ActualitzarUI("L'usuari no existeix", Color.red);
                else if (www.responseCode == 409) ActualitzarUI("L'usuari ja existeix", Color.red);
                else ActualitzarUI("Error: " + (www.responseCode > 0 ? www.responseCode.ToString() : www.error), Color.red);
            }
        }
    }

    void ActualitzarUI(string t, Color c) { if (missatgeError) { missatgeError.text = t; missatgeError.color = c; } }

    [System.Serializable] public class UserRequest { public string username; public string password; }
    [System.Serializable] public class LoginResposta { public string token; public string username; }
}