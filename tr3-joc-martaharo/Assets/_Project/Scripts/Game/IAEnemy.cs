using UnityEngine;

/// <summary>
/// Script per a la IA del mode individual.
/// La IA busca l'objecte "Goal" i s'hi mou.
/// </summary>
public class IAEnemy : MonoBehaviour
{
    [Header("Configuració")]
    public float speed = 3.5f;
    public string goalTag = "Finish";

    private Transform targetGoal;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Start()
    {
        GameObject goal = GameObject.FindGameObjectWithTag(goalTag);
        if (goal != null)
        {
            targetGoal = goal.transform;
            Debug.Log("IA iniciada: cercant Goal");
        }
        else
        {
            Debug.LogWarning("IA: No s'ha trobat l'objecte Goal amb tag " + goalTag);
        }
    }

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }

    private void FixedUpdate()
    {
        if (targetGoal != null)
        {
            Vector2 direction = (targetGoal.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.RecibirDanyoRpc();
                player.EfectoDanyoVisualRpc();
            }
        }
    }
}