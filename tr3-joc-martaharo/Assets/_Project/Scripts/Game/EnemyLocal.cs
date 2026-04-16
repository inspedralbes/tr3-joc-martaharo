using UnityEngine;

public class EnemyLocal : MonoBehaviour
{
    public float speed = 3f;
    public float detectionRadius = 10f;

    private Transform target;
    private Vector3 startPosition;
    private float patrolDirection = 1f;
    private float patrolDistance = 3f;

    void Start()
    {
        if (string.IsNullOrEmpty(gameObject.tag) || gameObject.tag == "Untagged")
            gameObject.tag = "Enemy";
        startPosition = transform.position;
    }

    void Update()
    {
        FindClosestTarget();
        if (target != null)
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        else
            PatrolMovement();
    }

    private void FindClosestTarget()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        GameObject agentObj = GameObject.FindWithTag("Agent");

        float playerDist = float.MaxValue;
        float agentDist = float.MaxValue;

        if (playerObj != null) playerDist = Vector2.Distance(transform.position, playerObj.transform.position);
        if (agentObj != null) agentDist = Vector2.Distance(transform.position, agentObj.transform.position);

        if (playerDist < detectionRadius && agentDist < detectionRadius)
            target = (playerDist < agentDist) ? playerObj.transform : agentObj.transform;
        else if (playerDist < detectionRadius)
            target = playerObj.transform;
        else if (agentDist < detectionRadius)
            target = agentObj.transform;
        else
            target = null;
    }

    private void PatrolMovement()
    {
        float newX = startPosition.x + (patrolDistance * patrolDirection);
        Vector3 patrolTarget = new Vector3(newX, startPosition.y, startPosition.z);
        transform.position = Vector3.MoveTowards(transform.position, patrolTarget, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, patrolTarget) < 0.1f) patrolDirection *= -1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerControllerLocal player = other.GetComponent<PlayerControllerLocal>();
        if (player != null) { GameManagerIA.Instance?.DamagePlayerAndEnemy(); return; }

        BirdAgentIA agent = other.GetComponent<BirdAgentIA>();
        if (agent != null) { agent.AddReward(-1f); agent.EndEpisode(); }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        { GameManagerIA.Instance?.DamagePlayerAndEnemy(); return; }

        PlayerControllerLocal player = collision.gameObject.GetComponent<PlayerControllerLocal>();
        if (player != null) { GameManagerIA.Instance?.DamagePlayerAndEnemy(); return; }

        BirdAgentIA agent = collision.gameObject.GetComponent<BirdAgentIA>();
        if (agent != null) { agent.AddReward(-1f); agent.EndEpisode(); }
    }

    public void ResetPosition() { transform.position = startPosition; target = null; }
}