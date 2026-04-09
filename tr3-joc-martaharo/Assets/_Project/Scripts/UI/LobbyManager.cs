using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using SocketIOClient;
using TMPro;
using Unity.Netcode;

public class LobbyManager : MonoBehaviour
{
    // URL del servidor definida en tu stack tecnológico
    private string urlServidor = "http://localhost:3000";

    [Header("UI Elements")]
    private Label labelCodi;
    private Label llistaJugadors;
    private Label labelErrorMenu;
    private Button btnComencar;

    // Referencias de red
    private SocketIO client;
    private string playerName;
    
    // Variable estàtica per passar el nom al PlayerController
    public static string localPlayerName;
    
    // PUBLIC para que EnemyAI y PlayerController lo lean (Error CS0122 solucionado)
    public static string roomId;

    void Start()
    {
        // El problema del 'ja existeix': Limpiar sesisones previas si el NetworkManager persiste
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            Debug.Log("LobbyManager: Netejant sessió de xarxa anterior...");
            NetworkManager.Singleton.Shutdown();
        }
    }

    void OnEnable()
    {
        // 1. Inicializar UI Toolkit con los nombres de tu UI Builder
        var root = GetComponent<UIDocument>().rootVisualElement;
        labelCodi = root.Q<Label>("label-codi");
        llistaJugadors = root.Q<Label>("llista-jugadors");
        labelErrorMenu = root.Q<Label>("label-error-menu");
        btnComencar = root.Q<Button>("btn-comencar");

        // 2. Cargar datos desde MainMenuManager
        roomId = MainMenuManager.roomId;
        labelCodi.text = "CODI: " + MainMenuManager.roomCode;
        
        // 3. Obtener el nombre del usuario logueado (según tu AuthManager)
        playerName = AuthManager.username;
        
        // Passar el nom al PlayerController via variable estàtica
        localPlayerName = playerName;

        // 4. Configurar botón - sempre actiu
        btnComencar.SetEnabled(true);
        btnComencar.clicked += IniciarPartida;

        // Efectos Visuales (Hover)
        btnComencar.RegisterCallback<MouseEnterEvent>(evt => {
            btnComencar.style.backgroundColor = new Color(0f, 0.8f, 1f, 1f); // Cian brillante
            btnComencar.transform.scale = new Vector3(1.1f, 1.1f, 1f);
        });

        btnComencar.RegisterCallback<MouseLeaveEvent>(evt => {
            btnComencar.style.backgroundColor = new Color(0f, 0.4f, 0.6f, 1f); // Azul estándar
            btnComencar.transform.scale = new Vector3(1f, 1f, 1f);
        });

        // 5. Iniciar conexión sincronizada
        ConnectarAlServidor();
    }

    async void ConnectarAlServidor()
    {
        client = new SocketIO(urlServidor);

        // Al conectar, unirse a la sala creada en el servidor Node.js
        client.OnConnected += (sender, e) => {
            client.EmitAsync("joinRoom", new { 
                roomId = roomId, 
                playerName = playerName 
            });
        };

        // Error de Codi Incorrecte: Rebre errors d'unió a la sala
        client.On("joinError", response => {
            string errorMsg = response.GetValue<string>();
            UnityMainThreadDispatcher.Instance.Enqueue(() => {
                if (labelErrorMenu != null)
                {
                    labelErrorMenu.text = "CODI INCORRECTE";
                }
                if (labelCodi != null)
                {
                    if (errorMsg.Contains("full"))
                    {
                        labelCodi.text = "SALA PLENA";
                    }
                    else
                    {
                        labelCodi.text = "CODI INCORRECTE";
                    }
                }
            });
        });

        // Sincronització de Noms: escoltar updateLobby
        client.On("updateLobby", response => {
            // Obtenir array de noms directament (sense JsonUtility)
            string[] jugadors = response.GetValue<string[]>();
            
            // Crear llista incloent el jugador local
            List<string> todosJugadores = new List<string>();
            
            // Sempre incloure el propi usuari
            if (!string.IsNullOrEmpty(playerName))
            {
                todosJugadores.Add(playerName);
            }
            
            // Afegir la resta de jugadors del servidor usant foreach
            if (jugadors != null)
            {
                foreach (string nom in jugadors)
                {
                    if (!string.IsNullOrEmpty(nom) && !todosJugadores.Contains(nom))
                    {
                        todosJugadores.Add(nom);
                    }
                }
            }

            // Debug.Log mostrant quants jugadors hi ha connectats
            Debug.Log("Jugadors connectats: " + todosJugadores.Count);

            // Crear string per al Label amb cada nom en una línia nova
            string textLlista = "";
            foreach (string nom in todosJugadores)
            {
                textLlista += nom + "\n";
            }
            // Treure l'últim "\n"
            if (!string.IsNullOrEmpty(textLlista))
            {
                textLlista = textLlista.TrimEnd('\n');
            }
            
            // Actualitzar la UI (Thread principal)
            UnityMainThreadDispatcher.Instance.Enqueue(() => {
                llistaJugadors.text = "";
                llistaJugadors.text = textLlista;
            });
        });

        // Millora Visual: Si la sala està plena
        client.On("roomFull", response => {
            UnityMainThreadDispatcher.Instance.Enqueue(() => {
                labelCodi.text = "SALA PLENA";
            });
        });

        // Escuchar señal de inicio (Punto 3.3 de tu plan)
        client.On("startGame", response => {
            UnityMainThreadDispatcher.Instance.Enqueue(() => {
                if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.StartClient();
                }
            });
        });

        await client.ConnectAsync();
    }

    void IniciarPartida()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            // 1. Encendre el motor abans de marxar
            NetworkManager.Singleton.StartHost();
            
            // 2. Avisar als altres jugadors via Socket.io
            client.EmitAsync("startGame", new { roomId = roomId });
            
            // 3. Canviar d'escena amb el NetworkSceneManager (MOLT IMPORTANT)
            // Això assegura que els jugadors que es connectin arribin directament a l'escena de joc
            NetworkManager.Singleton.SceneManager.LoadScene("Joc", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    void OnDestroy()
    {
        // NO cridem a NetworkManager.Singleton.Shutdown() aquí!
        // Això matava la xarxa enmig del canvi d'escena i provocava el NullReferenceException.
        
        if (client != null) client.DisconnectAsync();
    }
    
    [System.Serializable]
    public class LobbyUpdate
    {
        public List<string> players;
    }
}