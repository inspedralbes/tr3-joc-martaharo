using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Unity.Collections;
using System.Reflection;
using System.Collections;
using System.Text.Json;
using UnityEngine.Networking;

// =================================================================================
// SCRIPT: PlayerController
// UBICACIÓ: Assets/_Project/Scripts/Player/
// DESCRIPCIÓ: Control de moviment per a multijugador Unity 6 (Netcode).
//             Gestiona autoritat, colors, càmera, animacions i reaparició.
// =================================================================================

public class PlayerController : NetworkBehaviour
{
    [Header("Identitat i Estètica")]
    public string playerId;
    public Sprite spriteBlanco; // Sprite per al Client
    public NetworkVariable<FixedString32Bytes> username = new NetworkVariable<FixedString32Bytes>();

    [Header("Moviment")]
    public float speed = 5f;

    [Header("Sistema de Victòria")]
    public NetworkVariable<float> startTime = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public static PlayerController Instance { get; private set; }

    private Rigidbody2D rb;
    private Animator anim;
    private float inputX;
    private float inputY;
    private bool haArribatAMeta = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        haArribatAMeta = false;

        // Correció per al NetworkAnimator amb Reflection
        NetworkAnimator networkAnim = GetComponent<NetworkAnimator>();
        if (networkAnim != null)
        {
            var field = typeof(NetworkAnimator).GetField("m_Animator", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null) field.SetValue(networkAnim, anim);
        }
        
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Instance = this;
        haArribatAMeta = false;

        if (!IsServer) return;

        // Assignar el nom d'usuari des de l'AuthManager
        username.Value = (FixedString32Bytes)AuthManager.username;

        startTime.Value = Time.time;

        // LÒGICA DE COLOR: Si no som el servidor, canviem el sprite al blanc
        if (!IsServer && spriteBlanco != null)
        {
            GetComponent<SpriteRenderer>().sprite = spriteBlanco;
        }

        if (IsOwner)
        {
            playerId = "Jugador Local (" + OwnerClientId + ")";
            
            if (rb != null)
            {
                rb.simulated = true;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            StartCoroutine(CorrutinaCamaraBuildFix());
        }
        else
        {
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.simulated = true;
            }
            DesactivarComponentsRemots();
        }
    }

    private System.Collections.IEnumerator CorrutinaCamaraBuildFix()
    {
        if (!IsOwner) yield break;

        SeguimentOcell scriptCamara = null;

        while (scriptCamara == null)
        {
            scriptCamara = Object.FindFirstObjectByType<SeguimentOcell>();

            if (scriptCamara == null && Camera.main != null)
            {
                scriptCamara = Camera.main.GetComponent<SeguimentOcell>();
            }

            if (scriptCamara == null) yield return new WaitForSeconds(0.1f);
        }

        scriptCamara.SetTarget(this.transform);
        Debug.Log("<color=cyan>[SISTEMA]</color> Càmera vinculada amb èxit al Build local.");
    }

    void DesactivarComponentsRemots()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null) cam.enabled = false;

        AudioListener listener = GetComponentInChildren<AudioListener>();
        if (listener != null) listener.enabled = false;
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        if (!IsOwner) return;

        if (haArribatAMeta)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (rb != null && rb.bodyType != RigidbodyType2D.Dynamic)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
        }

        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        if (anim != null)
        {
            anim.SetFloat("VelocitatX", inputX);
            anim.SetFloat("VelocitatY", inputY);
            float magnitud = new Vector2(inputX, inputY).magnitude;
            anim.SetFloat("Velocitat", magnitud);
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (haArribatAMeta)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (rb != null && rb.bodyType != RigidbodyType2D.Dynamic)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
        }

        Vector2 moviment = new Vector2(inputX, inputY).normalized;
        if (rb != null) rb.linearVelocity = moviment * speed;
    }

    // --- SISTEMA DE RESPAWN ---
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void RecibirDanyoRpc()
    {
        if (!IsOwner) return;

        GameObject spawnPoint = GameObject.FindWithTag("Respawn");
        Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.transform.position : Vector3.zero;

        GetComponent<ClientNetworkTransform>().Teleport(spawnPos, Quaternion.identity, transform.localScale);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void EfectoDanyoVisualRpc()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        Invoke(nameof(ResetColor), 0.2f);
    }

    private void ResetColor()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        if (haArribatAMeta) return;

        if (other.CompareTag("Finish"))
        {
            haArribatAMeta = true;

            float temps = Time.time - startTime.Value;
            string nomJugador = username.Value.ToString();

            FinalitzarPartidaRpc(nomJugador, temps);
            StartCoroutine(EnviarRanking(nomJugador, temps));
        }
    }

    private IEnumerator EnviarRanking(string nomUsuari, float durada)
    {
        RankingData data = new RankingData { username = nomUsuari, puntuacio = durada, tipus = "MULTIPLAYER" };
        string json = JsonUtility.ToJson(data);
        Debug.Log("[RANKING] Enviant rànquing: " + json);

        using (UnityWebRequest req = new UnityWebRequest("http://localhost:3000/api/rankings", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            Debug.Log("[RANKING] Rànquing enviat correctament.");
        }
    }

    [System.Serializable]
    private class RankingData
    {
        public string username;
        public float puntuacio;
        public string tipus;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void FinalitzarPartidaRpc(string guanyador, float temps)
    {
        UIDocument[] totsElsDocs = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
        VisualElement panell = null;
        VisualElement arrel = null;
        
        foreach (var doc in totsElsDocs)
        {
            panell = doc.rootVisualElement.Q<VisualElement>("panel-resultats");
            if (panell != null)
            {
                arrel = doc.rootVisualElement;
                break;
            }
        }

        if (panell == null)
        {
            Debug.LogError("[RESULTATS] ERROR: No s'ha trobat l'element 'panel-resultats'. Revisa el camp 'Name' al UI Builder perquè coincideixi amb 'panel-resultats'.");
            return;
        }

        Label labelGuanyador = arrel.Q<Label>("label-guanyador");
        Label labelTemps = arrel.Q<Label>("label-temps");
        Button btnTornar = arrel.Q<Button>("btn-tornar");
        Button btnSortir = arrel.Q<Button>("btn-sortir");

        string tempsFormat = temps.ToString("F2");

        if (labelGuanyador != null) labelGuanyador.text = $"GUANYADOR: {guanyador}";
        if (labelTemps != null) labelTemps.text = $"TEMPS: {tempsFormat}s";

        if (btnTornar != null)
        {
            btnTornar.clicked += BotoTornarLobby;
        }

        if (btnSortir != null)
        {
            btnSortir.clicked += () =>
            {
                if (NetworkManager.Singleton != null)
                {
                    NetworkManager.Singleton.Shutdown();
                }
                if (SceneManager.GetActiveScene().name != "Menu")
                {
                    SceneManager.LoadScene("Menu");
                }
                else
                {
                    Application.Quit();
                }
            };
        }

        panell.style.display = DisplayStyle.Flex;
        panell.style.opacity = 1;
        Debug.Log("<color=green>[UI]</color> Panell de resultats mostrat correctament.");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    // --- REAPARICIÓ (RESPAWN) ---
    public void Respawn() 
    {
        if (!IsServer) return; 

        GameObject spawnPoint = GameObject.FindWithTag("Respawn");
        Vector3 posicionDestino = (spawnPoint != null) ? spawnPoint.transform.position : Vector3.zero;

        transform.position = posicionDestino;

        if (rb != null) 
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        Debug.Log($"[SERVER] Jugador {OwnerClientId} reaparegut en {posicionDestino}");
    }

    public override void OnDestroy()
    {
        haArribatAMeta = false;
        base.OnDestroy();
    }

    public void BotoTornarLobby()
    {
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("[NAVEGACIO] Tancant xarxa per tornar al Lobby...");
            NetworkManager.Singleton.Shutdown();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }
}