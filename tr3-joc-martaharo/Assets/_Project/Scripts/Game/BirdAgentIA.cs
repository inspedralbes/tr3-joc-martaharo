using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// El nom del comportament al Behavior Parameters ha de ser: IA_Enemy_Perseguidor
public class BirdAgentIA : Agent
{
    [Header("Config")]
    public float moveSpeed = 5f;
    public float verticalSpeed = 5f;

    private Rigidbody2D rb;
    private Vector3 spawnPosition;
    private float stepCount = 0;
    private float maxSteps = 1000;

    // Comptador per al Debug.Log cada ~1 segon
    private float debugTimer = 0f;

    public Transform goalZone;

    protected override void Awake()
    {
        base.Awake();
        Debug.LogError("!!! EL SCRIPT BIRDAGENTIA S'ESTÀ EXECUTANT !!!"); // En vermell perquè es vegi clar
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = transform.position;
        gameObject.tag = "Agent";

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearDamping = 0f;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        GameManagerIA gm = FindFirstObjectByType<GameManagerIA>();
        if (gm != null) gm.birdAgent = this;
    }

    private void FixedUpdate()
    {
        debugTimer += Time.fixedDeltaTime;
        if (debugTimer >= 1f)
        {
            Debug.Log("IA ACTIVA");
            debugTimer = 0f;
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.position = spawnPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        stepCount = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float enemyDistX = 0f;
        float enemyDistY = 0f;
        float goalDistX  = 0f;
        float goalDistY  = 0f;

        GameObject enemyObj = GameObject.FindWithTag("Enemy");
        if (enemyObj != null)
        {
            enemyDistX = enemyObj.transform.position.x - transform.position.x;
            enemyDistY = enemyObj.transform.position.y - transform.position.y;
        }

        GameObject goalObj = GameObject.FindWithTag("Finish");
        if (goalObj != null)
        {
            goalDistX = goalObj.transform.position.x - transform.position.x;
            goalDistY = goalObj.transform.position.y - transform.position.y;
        }
        else if (goalZone != null)
        {
            goalDistX = goalZone.position.x - transform.position.x;
            goalDistY = goalZone.position.y - transform.position.y;
        }

        // Slot 1 – posició Y
        sensor.AddObservation(transform.localPosition.y);
        // Slot 2 – velocitat Y
        sensor.AddObservation(rb.linearVelocity.y);
        // Slot 3 – distància X a l'enemic
        sensor.AddObservation(enemyDistX);
        // Slot 4 – distància Y a l'enemic
        sensor.AddObservation(enemyDistY);
        // Slot 5 – distància X a la meta
        sensor.AddObservation(goalDistX);
        // Slot 6 – distància Y a la meta
        sensor.AddObservation(goalDistY);
        // Slot 7 – velocitat X
        sensor.AddObservation(rb.linearVelocity.x);
    }

  public override void OnActionReceived(ActionBuffers actions)
{
    stepCount++;
    if (stepCount >= maxSteps) { AddReward(-1f); EndEpisode(); return; }

    // Useu dues branques (necessitaràs canviar el "Discrete Branches" a 2 a l'Inspector)
    int moveY = actions.DiscreteActions[0]; // 0: quiet, 1: amunt, 2: avall
    int moveX = actions.DiscreteActions[1]; // 0: quiet, 1: endavant

    float vSpeed = (moveY == 1 ? 5f : (moveY == 2 ? -5f : 0f));
    float hSpeed = (moveX == 1 ? 3f : 0f); // Ara ell decideix quan avançar

    rb.linearVelocity = new Vector2(hSpeed, vSpeed);
    
    // Premi petit per estar viu i no xocar (ajuda a que no es quedi quiet)
    AddReward(0.001f);
}
public override void Heuristic(in ActionBuffers actionsOut)
{
    var discreteActions = actionsOut.DiscreteActions;
    // Netegem l'acció per defecte
    discreteActions[0] = 0;

    if (Input.GetKey(KeyCode.W)) {
        discreteActions[0] = 1;
        Debug.Log("Tecla W detectada -> Acció 1");
    }
    else if (Input.GetKey(KeyCode.S)) {
        discreteActions[0] = 2;
        Debug.Log("Tecla S detectada -> Acció 2");
    }
}

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("IA colisiona amb: " + other.tag);

        if (other.CompareTag("Finish"))
        {
            AddReward(2f);
            EndEpisode();
        }
        else if (other.CompareTag("Enemy") || other.CompareTag("Pared") || other.CompareTag("Paret"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    public void Respawn()
    {
        AddReward(-1f);
        EndEpisode();
    }

    public void SetSpawnPosition(Vector3 pos)
    {
        spawnPosition = pos;
    }
}