using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using SocketIOClient;

public class LobbyManager : MonoBehaviour
{
    // URL del servidor definida en tu stack tecnológico
    private string urlServidor = "http://localhost:3000";

    [Header("UI Elements")]
    private Label labelCodi;
    private Label llistaJugadors;
    private Button btnComencar;

    // Referencias de red
    private SocketIO client;
    private string playerName;
    
    // PUBLIC para que EnemyAI y PlayerController lo lean (Error CS0122 solucionado)
public static string roomId;
    void OnEnable()
    {
        // 1. Inicializar UI Toolkit con los nombres de tu UI Builder
        var root = GetComponent<UIDocument>().rootVisualElement;
        labelCodi = root.Q<Label>("label-codi");
        llistaJugadors = root.Q<Label>("llista-jugadors");
        btnComencar = root.Q<Button>("btn-comencar");

        // 2. Cargar datos desde MainMenuManager
        roomId = MainMenuManager.roomId;
        labelCodi.text = "CODI: " + MainMenuManager.roomCode;
        
        // 3. Obtener el nombre del usuario logueado (según tu AuthManager)
        playerName = AuthManager.username;

        // 4. Configurar botón con tu paleta Azul/Cian
        btnComencar.SetEnabled(true); // Permitir jugar solo para testeo
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

        // Escuchar actualizaciones (Punto 5.4 de tu plan de trabajo)
        client.On("updateLobby", response => {
            Debug.Log("Sincronizando lista de jugadores...");
        });

        // Escuchar señal de inicio (Punto 3.3 de tu plan)
        client.On("startGame", response => {
            SceneManager.LoadScene("Joc");
        });

        await client.ConnectAsync();
    }

    void IniciarPartida()
    {
        if (client != null && client.Connected)
        {
            // Enviar evento al servidor para sincronizar a ambos jugadores
            client.EmitAsync("startGame", new { roomId = roomId });
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
}