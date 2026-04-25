using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

// =================================================================================
// SCRIPT: GameUIManager (MINIMALISTA)
// DESCRIPCIÓ: Gestiona ÚNICAMENT la pantalla de desconnexió via UI Toolkit (neó).
//             Tot el Canvas (GameOver, Victòria, Espera) ha estat eliminat.
// =================================================================================

public class GameUIManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // VARIABLES PRIVADES — UI Toolkit
    // -------------------------------------------------------------------------

    private UIDocument uidocument;
    private VisualElement root;
    private VisualElement contenedorDesconexion;
    private Label labelMissatge;
    private Button btnTornarLobby;
    private Button btnSortirMenu;

    // -------------------------------------------------------------------------
    // CICLE DE VIDA
    // -------------------------------------------------------------------------

    private void Awake()
    {
        Debug.Log(">>> SCRIPT ACTIVAT A: " + gameObject.name);

        uidocument = GetComponent<UIDocument>();
        if (uidocument == null)
        {
            Debug.LogError(">>> ERROR CRÍTIC: No s'ha trobat cap UIDocument en aquest GameObject!");
            return;
        }

        root = uidocument.rootVisualElement;

        contenedorDesconexion = root.Q("contenedor-desconexion");

        if (contenedorDesconexion == null)
        {
            Debug.LogError(">>> ERROR CRÍTIC: No s'ha trobat 'contenedor-desconexion' al UXML. " +
                           "Revisa el nom al UI Builder!");
            return;
        }

        // Aplicar classes USS programàticament per activar els estils de neó
        contenedorDesconexion.AddToClassList("contenedor-desconexion");
        contenedorDesconexion.style.display = DisplayStyle.None;

        labelMissatge  = root.Q<Label>("label-missatge");
        btnTornarLobby = root.Q<Button>("btn-tornar-lobby");
        btnSortirMenu  = root.Q<Button>("btn-sortir-menu");

        if (btnTornarLobby != null)
        {
            btnTornarLobby.AddToClassList("boto-neon");
            btnTornarLobby.clicked += BotoTornarLobby;
        }

        if (btnSortirMenu != null)
        {
            btnSortirMenu.AddToClassList("boto-neon");
            btnSortirMenu.clicked += BotoSortirMenu;
        }

        if (labelMissatge != null)
        {
            labelMissatge.AddToClassList("missatge");
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    // -------------------------------------------------------------------------
    // UPDATE — PROVA LOCAL (Tecla K)
    // -------------------------------------------------------------------------

    private void Update()
    {
        // Prem K per simular una desconnexió i provar la UI sense xarxa
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("[PROVA] Tecla K premuda — simulant desconnexió del client 999");
            OnClientDisconnected(999);
        }
    }

    // -------------------------------------------------------------------------
    // DESCONNEXIÓ
    // -------------------------------------------------------------------------

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log("[DESCONNEXIO] Client desconnectat: " + clientId);

        // Forçar que el UIDocument estigui actiu
        if (uidocument != null)
            uidocument.enabled = true;

        // Mostrem el panell per a qualsevol desconnexió
        if (contenedorDesconexion != null)
        {
            contenedorDesconexion.BringToFront();
            contenedorDesconexion.style.display = DisplayStyle.Flex;
        }

        if (NetworkManager.Singleton == null)
        {
            // Mode de prova sense xarxa (p. ex. tecla K)
            if (labelMissatge != null)
                labelMissatge.text = "Desconnexió simulada (mode prova)";
            return;
        }

        // Intentem obtenir el nom real del jugador via el mapa del LobbyManager
        string nomJugador = null;
        if (LobbyManager.nomPerClientId != null &&
            LobbyManager.nomPerClientId.TryGetValue(clientId, out string nomTrobat))
        {
            nomJugador = nomTrobat;
        }

        // Determinem el motiu i construïm el missatge personalitzat
        bool elHostHaMarxat = !NetworkManager.Singleton.IsServer
                              && clientId == NetworkManager.ServerClientId;

        string missatge;
        if (elHostHaMarxat)
        {
            missatge = "El Host ha tancat la partida";
            Debug.Log("[DESCONNEXIO] El Host ha tancat la sala. Mostrant panell de desconnexió.");
        }
        else if (!string.IsNullOrEmpty(nomJugador))
        {
            // Tenim el nom real del jugador
            missatge = $"El jugador {nomJugador} ha marxat";
            Debug.Log($"[DESCONNEXIO] El jugador '{nomJugador}' (id: {clientId}) ha marxat.");
        }
        else
        {
            // No tenim el nom, usem l'id com a fallback
            missatge = $"El Jugador {clientId} ha abandonat la partida";
            Debug.Log("[DESCONNEXIO] Un jugador (id: " + clientId + ") ha abandonat la partida.");
        }

        if (labelMissatge != null)
            labelMissatge.text = missatge;
    }

    // -------------------------------------------------------------------------
    // BOTONS
    // -------------------------------------------------------------------------

    public void BotoTornarLobby()
    {
        Debug.Log("[NAVEGACIO] Tancant xarxa per tornar al Lobby...");
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene("Lobby");
    }

    public void BotoSortirMenu()
    {
        Debug.Log("[NAVEGACIO] Tancant xarxa per sortir al Menú...");
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene("Menu");
    }
}
