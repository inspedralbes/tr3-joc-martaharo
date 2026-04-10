using Unity.Netcode;
using UnityEngine;

public class EnemyAI : NetworkBehaviour
{
    [Header("Configuración de movimiento")]
    public float speed = 3.5f;
    public float chaseRadius = 10f;

    private Rigidbody2D rb;
    private Transform targetPlayer;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearDamping = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (targetPlayer != null)
        {
            Vector2 direction = (targetPlayer.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
        else
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        bool isMoving = rb != null && rb.linearVelocity.magnitude > 0.1f;
        if (anim != null)
        {
            anim.SetBool("isWalking", isMoving);
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform nearestPlayer = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject player in players)
        {
            if (player.activeInHierarchy)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearestPlayer = player.transform;
                }
            }
        }

        if (nearestPlayer != null && nearestDistance <= chaseRadius)
        {
            targetPlayer = nearestPlayer;
        }
        else
        {
            targetPlayer = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;
        ProcesarColision(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsServer) return;
        ProcesarColision(collision.gameObject);
    }

   private void ProcesarColision(GameObject objeto)
    {
        if (objeto.CompareTag("Player"))
        {
            PlayerController pc = objeto.GetComponent<PlayerController>();
            if (pc != null)
            {
                // Este log te confirmará en la consola de Unity si el servidor detecta el choque
                Debug.Log($"[SERVER] Impacto detectado con: {objeto.name}");
                pc.RecibirDanyo();
            }
        }
    }
}