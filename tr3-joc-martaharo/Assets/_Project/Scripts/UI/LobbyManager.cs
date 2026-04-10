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

    [Header("UI References")]
    private Label labelCodi;
    private Label labelJugadors;
    private Button btnComencar;

    // SINCRONITZACIÓ NETCODE
    private NetworkVariable<FixedString32Bytes> codiSalaSincronitzat = new NetworkVariable<FixedString32Bytes>(
        "", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private NetworkList<FixedString32Bytes> nombresConectados;

    private void Awake()
    {
        nombresConectados = new NetworkList<FixedString32Bytes>();
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

        // Subscripcions
        nombresConectados.OnListChanged += OnLlistaNomsChanged;
        codiSalaSincronitzat.OnValueChanged += OnCodiChanged;

        if (IsServer)
        {
            codiSalaSincronitzat.Value = MainMenuManager.roomCode;
            AfegirNomALlista(AuthManager.username);
        }
        else
        {
            AfegirNomALlistaServerRpc(AuthManager.username);
        }

        ActualizarInterfaz();
        StartCoroutine(RefrescForcatCoroutine());
    }

    private IEnumerator RefrescForcatCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        ActualizarInterfaz();
    }

    public override void OnNetworkDespawn()
    {
        if (nombresConectados != null) nombresConectados.OnListChanged -= OnLlistaNomsChanged;
        if (codiSalaSincronitzat != null) codiSalaSincronitzat.OnValueChanged -= OnCodiChanged;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void AfegirNomALlistaServerRpc(string nom) { AfegirNomALlista(nom); }

    private void AfegirNomALlista(string nom)
    {
        FixedString32Bytes fixedNom = nom;
        if (!nombresConectados.Contains(fixedNom)) nombresConectados.Add(fixedNom);
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
                foreach (var n in nombresConectados) noms.Add(n.ToString());
            }
            if (noms.Count == 0 && !string.IsNullOrEmpty(localPlayerName)) noms.Add(localPlayerName);
            labelJugadors.text = string.Join("\n", noms);
        }

        if (btnComencar != null) btnComencar.style.display = DisplayStyle.Flex;
    }

    private void IniciarPartidaUI()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.LogError($"[LobbyManager] Error: Xarxa no escoltant. IsClient:{IsClient}, IsServer:{IsServer}");
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