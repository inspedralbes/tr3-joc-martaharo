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
            Debug.Log("Sincronizando lista de jugadores...");
            
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
            
            // Crear string per al Label
            string textLlista = "JUGADORS: ";
            foreach (string nom in todosJugadores)
            {
                textLlista += nom + ", ";
            }
            // Treure l'últim ", "
            if (todosJugadores.Count > 0)
            {
                textLlista = textLlista.Substring(0, textLlista.Length - 2);
            }
            
            // Actualitzar la UI (Thread principal)
            UnityMainThreadDispatcher.Instance.Enqueue(() => {
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
                SceneManager.LoadScene("Joc");
            });
        });

        await client.ConnectAsync();
    }

    void IniciarPartida()
    {
        if (client != null && client.Connected)
        {
            // Iniciar el Host
            NetworkManager.Singleton.StartHost();
            
            // Enviar event perquè la resta de jugadors s'uneixin com a clients
            client.EmitAsync("startGame", new { roomId = roomId });
            
            // Carregar l'escena de joc
            NetworkManager.Singleton.SceneManager.LoadScene("Joc", LoadSceneMode.Single);
        }
        else
        {
            // Fallback: Si el servidor está apagado, entrar al juego para testear movimiento
            Debug.LogWarning("Servidor offline. Entrando en modo local.");
            SceneManager.LoadScene("Joc");
        }
    }

    void OnDestroy()
    {
        if (client != null) client.DisconnectAsync();
    }
    
    [System.Serializable]
    public class LobbyUpdate
    {
        public List<string> players;
    }
}