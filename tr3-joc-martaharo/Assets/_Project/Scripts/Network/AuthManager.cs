using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class AuthManager : MonoBehaviour
{
    private string baseUrl = "http://localhost:8080/api/auth";

    [Header("Formulari de Login")]
    public InputField campUsuari;
    public InputField campContrasenya;
    public Text missatgeError;

    public static string token { get; private set; }
    public static string nomUsuari { get; private set; }

    public void FerLogin()
    {
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

        LoginRequest peticio = new LoginRequest();
        peticio.username = usuari;
        peticio.password = contrasenya;

        string jsonData = JsonUtility.ToJson(peticio);

        using (UnityWebRequest www = new UnityWebRequest(baseUrl + "/login", "POST"))
        {
            byte[] jsonToSend = new UTF8Encoding(true).GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resposta = www.downloadHandler.text;
                LoginResposta dadesResposta = JsonUtility.FromJson<LoginResposta>(resposta);

                token = dadesResposta.token;
                nomUsuari = dadesResposta.username;

                Debug.Log("Login correcte! Benvingut/da: " + nomUsuari);

                MostrarExit("Sessió iniciada. Benvingut/da!");
                Invoke("CanviarAMenu", 1.5f);
            }
            else
            {
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

        LoginRequest peticio = new LoginRequest();
        peticio.username = usuari;
        peticio.password = contrasenya;

        string jsonData = JsonUtility.ToJson(peticio);

        using (UnityWebRequest www = new UnityWebRequest(baseUrl + "/register", "POST"))
        {
            byte[] jsonToSend = new UTF8Encoding(true).GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string resposta = www.downloadHandler.text;
                LoginResposta dadesResposta = JsonUtility.FromJson<LoginResposta>(resposta);

                token = dadesResposta.token;
                nomUsuari = dadesResposta.username;

                Debug.Log("Registre correcte! Benvingut/da: " + nomUsuari);
                
                MostrarExit("Usuari creat correctament!");
                
                campUsuari.text = "";
                campContrasenya.text = "";
            }
            else
            {
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
        nomUsuari = null;
        SceneManager.LoadScene("Login");
    }

    void CanviarAMenu()
    {
        SceneManager.LoadScene("Menu");
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