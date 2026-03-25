// =================================================================================
// SCRIPT: AuthManager
// UBICACIÓ: Assets/_Project/Scripts/Network/
// DESCRIPCIÓ: Gestió d'autenticació amb el servidor Node.js
// =================================================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AuthManager : MonoBehaviour
{
    // URL del servidor Node.js
    private string urlServidor = "http://localhost:3000";

    // Camps del formulari de login (assigna des de l'inspector d'Unity)
    [Header("Formulari de Login")]
    public UnityEngine.UI.InputField campUsuari;
    public UnityEngine.UI.InputField campContrasenya;
    public UnityEngine.UI.Text missatgeError;

    // Variables per emmagatzemar el token i usuari
    public static string token { get; private set; }
    public static string nomUsuari { get; private set; }

    // Mètode cridat pel botó "Login" del formulari
    public void FerLogin()
    {
        string usuari = campUsuari.text;
        string contrasenya = campContrasenya.text;

        // Validar camp usuari buit
        if (string.IsNullOrEmpty(usuari))
        {
            MostrarError("Escriu un nom d'usuari!");
            return;
        }

        // Validar camp contrasenya buit
        if (string.IsNullOrEmpty(contrasenya))
        {
            MostrarError("Escriu la contrasenya!");
            return;
        }

        StartCoroutine(LoginCoroutine(usuari, contrasenya));
    }

    // Corrutina per fer la petició de login al servidor
    IEnumerator LoginCoroutine(string usuari, string contrasenya)
    {
        MostrarError("Connectant...");

        // Crear l'objecte petició
        LoginRequest peticio = new LoginRequest();
        peticio.username = usuari;
        peticio.password = contrasenya;

        // Convertir a JSON amb JsonUtility
        string jsonData = JsonUtility.ToJson(peticio);

        using (UnityWebRequest www = new UnityWebRequest(urlServidor + "/api/login", "POST"))
        {
            byte[] jsonToSend = new UTF8Encoding(true).GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resposta = www.downloadHandler.text;
                
                // Deserialitzar resposta amb JsonUtility
                LoginResposta dadesResposta = JsonUtility.FromJson<LoginResposta>(resposta);

                // Guardar el token i el nom d'usuari
                token = dadesResposta.token;
                nomUsuari = dadesResposta.username;

                Debug.Log("Login correcte! Benvingut/da: " + nomUsuari);
                Debug.Log("Token rebut: " + token);

                // Canviar a l'escena del Lobby
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                Debug.LogError("Error de login: " + www.error);
                string errorMsg = "Error de connexió. Servidor actiu?";

                // Intentem llegir el missatge d'error del servidor
                if (!string.IsNullOrEmpty(www.downloadHandler.text))
                {
                    try
                    {
                        ErrorResposta errorResponse = JsonUtility.FromJson<ErrorResposta>(www.downloadHandler.text);
                        if (!string.IsNullOrEmpty(errorResponse.error))
                        {
                            errorMsg = errorResponse.error;
                        }
                    }
                    catch
                    {
                        // Si no podem deserialitzar, usem el missatge per defecte
                    }
                }

                if (www.responseCode == 401)
                {
                    errorMsg = "Usuari o contrasenya incorrectes.";
                }

                MostrarError(errorMsg);
            }
        }
    }

    // Mostrar missatge d'error a la interfície
    void MostrarError(string missatge)
    {
        if (missatgeError != null)
        {
            missatgeError.text = missatge;
        }
    }

    // Classe per a la petició de login
    [System.Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
    }

    // Classe per deserialitzar la resposta del servidor
    [System.Serializable]
    public class LoginResposta
    {
        public string token;
        public string username;
    }

    // Classe per deserialitzar errors
    [System.Serializable]
    public class ErrorResposta
    {
        public string error;
    }

    // Mètode per tancar sessió (opcional)
    public void TancarSessio()
    {
        token = null;
        nomUsuari = null;
        SceneManager.LoadScene("Login");
    }
}
