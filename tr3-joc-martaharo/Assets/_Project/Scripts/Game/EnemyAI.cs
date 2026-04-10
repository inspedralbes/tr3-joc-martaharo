using Unity.Netcode;
using UnityEngine;

public class EnemyAI : NetworkBehaviour
{
    [Header("Configuración de movimiento")]
    public float speed = 2.5f;
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
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (targetPlayer != null)
        {
            Vector2 direction = (targetPlayer.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;

            if (anim != null)
            {
                anim.SetBool("isWalking", true);
            }
        }
        else
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;

            if (anim != null)
            {
                anim.SetBool("isWalking", false);
            }
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

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.RecibirDanyo();
            }
        }
    }
}