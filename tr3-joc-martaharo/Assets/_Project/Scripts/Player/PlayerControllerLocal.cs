using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PlayerControllerLocal : MonoBehaviour
{
    [Header("Moviment")]
    public float speed = 5f;
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private Animator anim;
    private float inputX;
    private float inputY;
    private bool haArribatAMeta = false;
    private Vector3 spawnPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spawnPosition = transform.position;
        haArribatAMeta = false;

        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearDamping = 0.5f;
        }
    }

    void Start()
    {
        // Local player always owns itself; refresh spawn position at scene start
        spawnPosition = transform.position;

        GameManagerIA gameManager = FindFirstObjectByType<GameManagerIA>();
        if (gameManager != null)
        {
            gameManager.player = this.gameObject;
        }

        SeguimentOcell camara = FindFirstObjectByType<SeguimentOcell>();
        if (camara != null)
        {
            camara.SetTarget(this.transform);
        }
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        if (haArribatAMeta)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
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
        if (haArribatAMeta)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (Input.GetKey(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        Vector2 moviment = new Vector2(inputX, inputY).normalized;
        if (rb != null)
        {
            rb.linearVelocity = moviment * speed;
        }
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.2f);
        return hit.collider != null;
    }

    public void Respawn()
    {
        transform.position = spawnPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        if (anim != null)
        {
            anim.SetFloat("VelocitatX", 0);
            anim.SetFloat("VelocitatY", 0);
            anim.SetFloat("Velocitat", 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Player colisiona amb: " + other.tag);

        if (haArribatAMeta) return;

        if (other.CompareTag("Enemy"))
        {
            // Local respawn: teleport back to spawn and flash red
            transform.position = spawnPosition;
            StartCoroutine(FlashRed());

            StartCoroutine(DamageEffect());
            if (GameManagerIA.Instance != null)
            {
                GameManagerIA.Instance.DamagePlayerAndEnemy();
            }
            return;
        }

        if (other.CompareTag("Finish"))
        {
            haArribatAMeta = true;

            UI_Manager_IA ui = FindFirstObjectByType<UI_Manager_IA>();
            if (ui != null)
            {
                ui.MostrarVictoria();
            }
            else if (GameManagerIA.Instance != null)
            {
                GameManagerIA.Instance.Victory("Jugador");
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player colisiona amb: " + collision.gameObject.tag);

        if (collision.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(DamageEffect());
            if (GameManagerIA.Instance != null)
            {
                GameManagerIA.Instance.DamagePlayerAndEnemy();
            }
        }
    }

    private IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        if (sr != null) sr.color = Color.white;
    }

    private IEnumerator DamageEffect()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
        }
        yield return new WaitForSeconds(0.2f);
        if (sr != null)
        {
            sr.color = Color.white;
        }
    }
}