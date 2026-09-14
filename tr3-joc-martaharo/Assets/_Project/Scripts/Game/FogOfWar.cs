using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    [Header("Configuració de visió")]
    public Transform playerTarget;
    public float visionRadius = 5f;
    public float lightIntensity = 1f;
    public Color lightColor = Color.white;

    private UnityEngine.Rendering.Universal.Light2D playerLight;
    private GameObject lightObj;

    void Start()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTarget = player.transform;
        }

        CrearLlum();
    }

    void CrearLlum()
    {
        lightObj = new GameObject("PlayerLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;

        playerLight = lightObj.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
        
        playerLight.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
        playerLight.color = lightColor;
        playerLight.intensity = lightIntensity;
        playerLight.pointLightInnerRadius = visionRadius * 0.5f;
        playerLight.pointLightOuterRadius = visionRadius;
        playerLight.falloffIntensity = 0.5f;

        Debug.Log($"[FogOfWar] Llum creada. Radi: {visionRadius}, Intensitat: {lightIntensity}");
    }

    void LateUpdate()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTarget = player.transform;
        }

        if (playerTarget != null && playerLight != null && lightObj != null)
        {
            transform.position = new Vector3(playerTarget.position.x, playerTarget.position.y, 0);
            lightObj.transform.position = transform.position;
        }
    }
}
