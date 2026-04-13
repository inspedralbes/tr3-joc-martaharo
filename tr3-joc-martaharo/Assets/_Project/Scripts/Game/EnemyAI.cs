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
            rb.bodyType = RigidbodyType2D.Kinematic;
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

    private void Update()
    {
        // FUERZA DE POSICIÓN: Asegurar Z = 0 siempre
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        // FUERZA DE POSICIÓN: Corregir Z del otro objeto
        other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y, 0);

        Debug.Log("[Enemigo] Colisión detectada con: " + other.name);

        // LLAMADA SEGURA: Obtener PlayerController y llamar a RecibirDanyo
        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            Debug.Log("[Enemigo] PlayerController encontrado, llamando a RecibirDanyo()");
            player.RecibirDanyo();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y, 0);

        if (other.TryGetComponent<PlayerController>(out PlayerController player))
        {
            player.RecibirDanyo();
        }
    }
}
