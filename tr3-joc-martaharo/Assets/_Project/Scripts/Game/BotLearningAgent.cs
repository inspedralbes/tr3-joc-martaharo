using UnityEngine;

public class BotLearningAgent : MonoBehaviour
{
    [Header("Configuració Agent")]
    public Transform targetGoal;
    public float moveSpeed = 5f;
    public string wallTag = "Paredes";
    public string goalTag = "Finish";

    [Header("Configuració Raycasts")]
    public float rayLength = 3f;
    public LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private Vector2 lastValidPosition;
    private bool hasReachedGoal = false;

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
        lastValidPosition = transform.position;
        
        if (targetGoal == null)
        {
            GameObject goalObj = GameObject.FindGameObjectWithTag(goalTag);
            if (goalObj != null)
                targetGoal = goalObj.transform;
        }
    }

    private void FixedUpdate()
    {
        if (hasReachedGoal) return;

        MoureCapObjectiu();
    }

    private void MoureCapObjectiu()
    {
        if (targetGoal == null) return;

        Vector2 directionToTarget = (targetGoal.position - transform.position).normalized;
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        bool frontClear = !RaycastDetect(transform.up, rayLength);
        bool leftClear = !RaycastDetect(Quaternion.Euler(0, 0, 45) * transform.up, rayLength);
        bool rightClear = !RaycastDetect(Quaternion.Euler(0, 0, -45) * transform.up, rayLength);

        Vector2 moveDirection;

        if (!frontClear)
        {
            if (leftClear && !rightClear)
                moveDirection = Quaternion.Euler(0, 0, 45) * transform.up;
            else if (rightClear && !leftClear)
                moveDirection = Quaternion.Euler(0, 0, -45) * transform.up;
            else if (leftClear && rightClear)
                moveDirection = (Random.value > 0.5f) 
                    ? Quaternion.Euler(0, 0, 45) * transform.up 
                    : Quaternion.Euler(0, 0, -45) * transform.up;
            else
                moveDirection = -directionToTarget;
        }
        else
        {
            moveDirection = directionToTarget;
        }

        float currentAngle = Mathf.LerpAngle(transform.eulerAngles.z, Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg, Time.fixedDeltaTime * 5f);
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);

        rb.linearVelocity = moveDirection * moveSpeed;
    }

    private bool RaycastDetect(Vector2 direction, float length)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, length, obstacleLayer);
        Debug.DrawRay(transform.position, direction * length, Color.red);
        return hit.collider != null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(goalTag))
        {
            hasReachedGoal = true;
            rb.linearVelocity = Vector2.zero;
            Debug.Log("Meta assolida!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(wallTag))
        {
            Debug.Log("Xoc amb paret - rebotant...");
            Vector2 reboundDirection = (transform.position - collision.transform.position).normalized;
            rb.linearVelocity = reboundDirection * moveSpeed;
            Invoke("RestoreMovement", 0.3f);
        }
    }

    private void RestoreMovement()
    {
        if (!hasReachedGoal && targetGoal != null)
            lastValidPosition = transform.position;
    }
}