using UnityEngine;
using UnityEngine.UI;          // Per als botons de Canvas (UnityEngine.UI.Button)
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.UIElements;  // Per al UIDocument (UnityEngine.UIElements.Button)

// =================================================================================
// SCRIPT: GameUIManager
// DESCRIPCIÓ: Gestió de la UI de joc. Controla panells, botons i desconnexió.
//             CS0104 resolt: Button de Canvas = UnityEngine.UI.Button
//                            Button de UIDocument = UnityEngine.UIElements.Button
// =================================================================================

public class GameUIManager : MonoBehaviour
{
    [Header("Panells UI")]
    public GameObject panelGameOver;
    public GameObject panelVictoria;
    public GameObject panelEspera;
    public GameObject panelOpcions;

    [Header("UI Document (UIToolkit)")]
    private UIDocument uidocument;
    private VisualElement root;
    private VisualElement contenedorDesconexion;
    private UnityEngine.UIElements.Button botonVolver; // UIElements.Button per al UIDocument

    [Header("Text Game Over")]
    public TextMeshProUGUI textGameOver;

    [Header("Text Victoria")]
    public TextMeshProUGUI textVictoria;

    [Header("Text Espera")]
    public TextMeshProUGUI textEspera;

    [Header("Botons Canvas")]
    public UnityEngine.UI.Button btnTornarJugar;    // UI.Button per al Canvas
    public UnityEngine.UI.Button btnMenuPrincipal;  // UI.Button per al Canvas
    public UnityEngine.UI.Button btnTancarOpcions;  // UI.Button per al Canvas

    private GameManager gameManager;

    // -------------------------------------------------------------------------
    // CICLE DE VIDA
    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        // Guard de nulitat: evita NullReferenceException si NetworkManager no existeix
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDisable()
    {
        // Guard de nulitat: evita errors en tancar el joc mentre no hi ha sessió
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void Awake()
    {
        // Configuració del UIDocument (UIToolkit)
        uidocument = GetComponent<UIDocument>();
        if (uidocument != null)
        {
            root = uidocument.rootVisualElement;
            contenedorDesconexion = root.Q("contenedor-desconexion");
            botonVolver = root.Q<UnityEngine.UIElements.Button>("boton-volver");

            // El panell de desconnexió comença ocult
            if (contenedorDesconexion != null)
            {
                contenedorDesconexion.style.display = DisplayStyle.None;
            }
        }
    }

    private void Start()
    {
        gameManager = Object.FindFirstObjectByType<GameManager>();

        // Botó del UIDocument
        if (botonVolver != null)
        {
            botonVolver.clicked += VolverAlMenu;
        }

        // Botons del Canvas
        if (btnTornarJugar != null)
            btnTornarJugar.onClick.AddListener(TornarJugar);

        if (btnMenuPrincipal != null)
            btnMenuPrincipal.onClick.AddListener(AnarMenuPrincipal);

        if (btnTancarOpcions != null)
            btnTancarOpcions.onClick.AddListener(TancarOpcions);

        MostrarPanelEspera();
    }

    // -------------------------------------------------------------------------
    // DESCONNEXIÓ
    // -------------------------------------------------------------------------

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log("[GameUIManager] Cliente desconectado: " + clientId);

        // Mostrem el panell NOMÉS si el HOST ha tancat la partida i nosaltres som el client.
        // NetworkManager.ServerClientId és sempre 0 (l'ID del Host/Servidor).
        if (NetworkManager.Singleton == null) return;

        bool hostHaTancat = !NetworkManager.Singleton.IsServer
                            && clientId == NetworkManager.ServerClientId;

        if (hostHaTancat)
        {
            if (contenedorDesconexion != null)
            {
                contenedorDesconexion.style.display = DisplayStyle.Flex;
            }
            Debug.Log("[GameUIManager] El Host ha tancat la partida. Mostrant panell de desconnexió.");
        }
    }

    // -------------------------------------------------------------------------
    // NAVEGACIÓ
    // -------------------------------------------------------------------------

    public void VolverAlMenu()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        SceneManager.LoadScene("Menu");
    }

    public void AnarMenuPrincipal()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        SceneManager.LoadScene("Menu");
    }

    public void TornarJugar()
    {
        SceneManager.LoadScene("Joc");
    }

    public void SortirJoc()
    {
        Application.Quit();
    }

    // -------------------------------------------------------------------------
    // PANELLS
    // -------------------------------------------------------------------------

    private void MostrarPanelEspera()
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

    private void AmagarEspera()
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

    private void TancarOpcions()
    {
        if (panelOpcions != null)
            panelOpcions.SetActive(false);
    }
}
