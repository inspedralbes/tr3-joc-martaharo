using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Mètode públic per anar a l'escena individual
    public void AnarIndividual()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Mètode públic per anar a l'escena multiplayer
    public void AnarMultiplayer()
    {
        SceneManager.LoadScene("Lobby");
    }
}
