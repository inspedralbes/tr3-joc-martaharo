using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    [Header("Panells UI")]
    public GameObject panelGameOver;
    public GameObject panelVictoria;
    public GameObject panelEspera;
    public GameObject panelOpcions;

    [Header("Text Game Over")]
    public TextMeshProUGUI textGameOver;

    [Header("Text Victoria")]
    public TextMeshProUGUI textVictoria;

    [Header("Text Espera")]
    public TextMeshProUGUI textEspera;

    [Header("Botons")]
    public Button btnTornarJugar;
    public Button btnMenuPrincipal;
    public Button btnTancarOpcions;

    private GameManager gameManager;

    void Start()
    {
        gameManager = Object.FindFirstObjectByType<GameManager>();

        if (btnTornarJugar != null)
            btnTornarJugar.onClick.AddListener(TornarJugar);

        if (btnMenuPrincipal != null)
            btnMenuPrincipal.onClick.AddListener(AnarMenuPrincipal);

        if (btnTancarOpcions != null)
            btnTancarOpcions.onClick.AddListener(TancarOpcions);

        MostrarPanelEspera();
    }

    void MostrarPanelEspera()
    {
        if (panelEspera != null)
            panelEspera.SetActive(true);

        if (panelGameOver != null)
            panelGameOver.SetActive(false);

        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        if (MainMenuManager.isSinglePlayer)
        {
            if (textEspera != null)
                textEspera.text = "Començant partida...";
            Invoke("AmagarEspera", 2f);
        }
        else
        {
            if (textEspera != null)
                textEspera.text = "Esperant un altre jugador...";
        }
    }

    void AmagarEspera()
    {
        if (panelEspera != null)
            panelEspera.SetActive(false);
    }

    public void MostrarGameOver()
    {
        if (panelGameOver != null)
            panelGameOver.SetActive(true);

        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        if (textGameOver != null)
            textGameOver.text = "Game Over";
    }

    public void MostrarVictoria()
    {
        if (panelVictoria != null)
            panelVictoria.SetActive(true);

        if (panelGameOver != null)
            panelGameOver.SetActive(false);

        if (textVictoria != null)
            textVictoria.text = "Has guanyat!";
    }

    public void MostrarOpcions()
    {
        if (panelOpcions != null)
            panelOpcions.SetActive(true);
    }

    void TancarOpcions()
    {
        if (panelOpcions != null)
            panelOpcions.SetActive(false);
    }

    public void TornarJugar()
    {
        SceneManager.LoadScene("Joc");
    }

    public void AnarMenuPrincipal()
    {
        SceneManager.LoadScene("Menu");
    }

    public void SortirJoc()
    {
        Application.Quit();
    }
}
