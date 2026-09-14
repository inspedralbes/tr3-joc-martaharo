using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;

public class LobbyManager : NetworkBehaviour
{
    // VARIABLES ESTÀTIQUES
    public static string roomId;
    public static string localPlayerName;

    // Mapa clientId → nom, per identificar jugadors en desconnexions
    public static Dictionary<ulong, string> nomPerClientId = new Dictionary<ulong, string>();

    [Header("UI References")]
    private Label labelCodi;
    private Label labelJugadors;
    private Button btnComencar;

    // SINCRONITZACIÓ NETCODE
    private NetworkVariable<FixedString32Bytes> codiSalaSincronitzat = new NetworkVariable<FixedString32Bytes>(
        "", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public NetworkList<FixedString32Bytes> listaNombres;

    private void Awake()
    {
        listaNombres = new NetworkList<FixedString32Bytes>();
    }

    private void Start()
    {
        // 1. Vincular UI immediatament
        var root = GetComponent<UIDocument>().rootVisualElement;
        labelCodi = root.Q<Label>("label-codi");
        labelJugadors = root.Q<Label>("llista-jugadors");
        btnComencar = root.Q<Button>("btn-comencar");

        if (btnComencar != null)
        {
            btnComencar.style.display = DisplayStyle.Flex;
            btnComencar.clicked -= IniciarPartidaUI;
            btnComencar.clicked += IniciarPartidaUI;
        }

        // 2. Restaurar dades locals
        roomId = MainMenuManager.roomId;
        localPlayerName = AuthManager.username;

        // 3. ENGEGAR XARXA (Lògica de Lobby-Centric)
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
        {
            Debug.Log("[LobbyManager] Engegant xarxa des del Lobby...");
            if (MainMenuManager.isHost)
            {
                Debug.Log("[LobbyManager] Iniciant com a HOST.");
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                Debug.Log("[LobbyManager] Iniciant com a CLIENT.");
                NetworkManager.Singleton.StartClient();
            }
        }

        ActualizarInterfaz();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("[LobbyManager] OnNetworkSpawn: Xarxa spawnejada.");

        // Subscripcions als canvis de dades de xarxa
        listaNombres.OnListChanged += OnLlistaNomsChanged;
        codiSalaSincronitzat.OnValueChanged += OnCodiChanged;

        if (IsServer)
        {
            codiSalaSincronitzat.Value = MainMenuManager.roomCode;
            // El servidor afegeix el seu propi nom (clientId del host és sempre 0)
            AfegirNomALlista(AuthManager.username, NetworkManager.Singleton.LocalClientId);
            // BUG FIX: Escoltar quan nous clients es connecten per afegir-los a la llista
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        else
        {
            // El client envia el seu nom al servidor (el RPC anota automàticament el senderId)
            AfegirNomALlistaServerRpc(AuthManager.username);
        }

        ActualizarInterfaz();
        StartCoroutine(RefrescForcatCoroutine());
    }

    // Cridat al servidor quan un nou client es connecta al lobby
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[LobbyManager] Client connectat al lobby: {clientId}. Esperant que enviï el seu nom...");
        // El client enviarà el seu nom via AfegirNomALlistaServerRpc des del seu OnNetworkSpawn
    }

    private IEnumerator RefrescForcatCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        ActualizarInterfaz();
    }

    public override void OnNetworkDespawn()
    {
        // Neteja de subscripcions per evitar fuites de memòria i NullReferenceException
        if (listaNombres != null) listaNombres.OnListChanged -= OnLlistaNomsChanged;
        if (codiSalaSincronitzat != null) codiSalaSincronitzat.OnValueChanged -= OnCodiChanged;
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void AfegirNomALlistaServerRpc(string nom, RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        AfegirNomALlista(nom, senderId);
    }

    private void AfegirNomALlista(string nom, ulong clientId = 0)
    {
        FixedString32Bytes fixedNom = nom;
        if (!listaNombres.Contains(fixedNom)) listaNombres.Add(fixedNom);

        // Guardar mapatge clientId → nom per a la pantalla de desconnexió
        if (!nomPerClientId.ContainsKey(clientId))
        {
            nomPerClientId[clientId] = nom;
            Debug.Log($"[LobbyManager] Mapejat clientId {clientId} → '{nom}'");
        }
    }

    private void OnLlistaNomsChanged(NetworkListEvent<FixedString32Bytes> changeEvent) { ActualizarInterfaz(); }
    private void OnCodiChanged(FixedString32Bytes vell, FixedString32Bytes nou) { ActualizarInterfaz(); }

    public void ActualizarInterfaz()
    {
        if (labelCodi != null)
        {
            string codi = (IsSpawned) ? codiSalaSincronitzat.Value.ToString() : "";
            if (string.IsNullOrEmpty(codi)) codi = MainMenuManager.roomCode;
            labelCodi.text = "CODI: " + codi;
        }

        if (labelJugadors != null)
        {
            List<string> noms = new List<string>();
            if (IsSpawned)
            {
                foreach (var n in listaNombres) noms.Add(n.ToString());
            }
            if (noms.Count == 0 && !string.IsNullOrEmpty(localPlayerName)) noms.Add(localPlayerName);
            labelJugadors.text = string.Join("\n", noms);
        }

        if (btnComencar != null) btnComencar.style.display = DisplayStyle.Flex;
    }

    private void IniciarPartidaUI()
    {
        // Guard: evitar enviar RPCs si l'objecte de xarxa no existeix encara
        if (!IsSpawned)
        {
            Debug.LogWarning("[LobbyManager] IniciarPartidaUI: l'objecte no està spawnejat a la xarxa encara.");
            return;
        }

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.LogError($"[LobbyManager] ERROR: La xarxa no s'ha iniciat correctament. IsClient: {IsClient}, IsServer: {IsServer}. Comprova que el NetworkManager estigui actiu abans de prémer el botó.");
            return;
        }

        // SEMPRE fem la petició al servidor per carregar l'escena de forma sincronitzada
        SolicitarInicioPartidaServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SolicitarInicioPartidaServerRpc(RpcParams rpcParams = default)
    {
        Debug.Log("[LobbyManager] Petición de inicio recibida del jugador: " + rpcParams.Receive.SenderClientId);
        if (IsServer)
        {
            NetworkManager.SceneManager.LoadScene("Joc", LoadSceneMode.Single);
        }
    }

    public override void OnDestroy() { base.OnDestroy(); }
}