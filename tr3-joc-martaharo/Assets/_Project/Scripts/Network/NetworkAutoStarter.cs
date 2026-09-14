using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetworkAutoStarter : MonoBehaviour
{
    
    public GameObject networkManagerPrefab;

    IEnumerator Start()
    {
        // Espera de seguretat perquè Unity carregui les físiques
        yield return new WaitForSeconds(0.1f);

        // Si el NetworkManager no existeix a l'escena, l'instanciem
        if (NetworkManager.Singleton == null)
        {
            if (networkManagerPrefab != null)
            {
                Instantiate(networkManagerPrefab);
                yield return null; // Esperem un frame per la inicialització del Singleton
            }
            else
            {
                Debug.LogError("NetworkAutoStarter: No s'ha assignat el prefab del NetworkManager");
                yield break;
            }
        }

        // Si el Manager ja existeix (o l'acabem de crear) però no està actiu...
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
        {
            // Iniciem com a Host per a proves directes
            NetworkManager.Singleton.StartHost();
            Debug.Log("NetworkAutoStarter: Host iniciat per a prova directa.");
        }
        else
        {
            Debug.Log("NetworkAutoStarter: Xarxa ja activa (venim del Lobby o ja és Host/Client)");
        }

        // En qualsevol cas, destruïm aquest objecte per netejar
        Destroy(gameObject);
    }
}