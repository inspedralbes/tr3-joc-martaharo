using UnityEngine;
using UnityEngine.Networking; // Obligatori per a peticions HTTP
using System.Collections;

public class AuthManager : MonoBehaviour
{
    private string urlServidor = "http://localhost:3000/login"; 

    public void IniciarSessio()
    {
        StartCoroutine(FerLogin("Mxrta22", "1234"));
    }

    IEnumerator FerLogin(string usuari, string pass)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", usuari);
        form.AddField("password", pass);

        using (UnityWebRequest www = UnityWebRequest.Post(urlServidor, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Login OK: " + www.downloadHandler.text);
            }
            else
            {
                Debug.Log("Error de connexió (el servidor està encès?): " + www.error);
            }
        }
    }
}