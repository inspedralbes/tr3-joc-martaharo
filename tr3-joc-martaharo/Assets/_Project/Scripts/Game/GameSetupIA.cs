using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSetupIA : MonoBehaviour
{
    [Header("Configuracio Joc_IA")]
    public GameObject gameManagerIA;
    public GameObject birdObject;
    public GameObject enemyObject;
    public GameObject goalZone;
    public SeguimentOcell camara;

    void Start()
    {
        StartCoroutine(SetupJoc());
    }

    IEnumerator SetupJoc()
    {
        yield return new WaitForSeconds(0.1f);

        if (birdObject != null)
        {
            BirdAgentIA agent = birdObject.GetComponent<BirdAgentIA>();
            if (agent != null && goalZone != null)
            {
                agent.goalZone = goalZone.transform;
            }
        }

        if (camara != null && birdObject != null)
        {
            camara.SetTarget(birdObject.transform);
        }

        Debug.Log("[Joc_IA] Configuracio completada!");
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Joc_IA")
        {
            StartCoroutine(SetupJoc());
        }
    }
}