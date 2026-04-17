using UnityEngine;
using TMPro;

public class UI_Manager_IA : MonoBehaviour
{
    [Header("Panells")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;
    public GameObject statsPanel;

    [Header("Textos")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI statsText;

    [Header("Configuracio")]
    public bool showStats = true;

    private float startTime;

    void Awake()
    {
        startTime = Time.time;
        OcultarPanells();
    }

    void OcultarPanells()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(showStats);
    }

    public void MostrarResultat(string missatge)
    {
        float temps = Time.time - startTime;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (resultText != null)
            {
                resultText.text = missatge + "\nTemps: " + temps.ToString("F2") + "s";
            }
        }

        Debug.Log("[UI_IA] Resultat: " + missatge + " | Temps: " + temps.ToString("F2") + "s");
    }

    public void MostrarDefeat()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (resultText != null)
            {
                resultText.text = "HAS PERDUT!";
            }
        }

        Debug.Log("[UI_IA] Game Over!");
    }

    public void MostrarVictoria()
    {
        MostrarResultat("VICTÒRIA!");
    }

    public void TornarMenu()
    {
        MainMenuManager.isSinglePlayer = false;
        MainMenuManager.roomId = "";
        MainMenuManager.roomCode = "";
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void TornarJugar()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Joc_IA");
    }
}