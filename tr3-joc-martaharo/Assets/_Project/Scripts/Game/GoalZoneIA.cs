using UnityEngine;

public class GoalZoneIA : MonoBehaviour
{
    private bool jaGuanyat = false;

    void OnTriggerEnter2D(Collider2D altre)
    {
        if (jaGuanyat) return;

        if (altre.CompareTag("Player") || altre.CompareTag("Finish") || altre.CompareTag("Goal"))
        {
            if (altre.GetComponent<BirdAgentIA>() != null || altre.GetComponent<Rigidbody2D>() != null)
            {
                GestionaraVictoria();
            }
        }
    }

    void GestionaraVictoria()
    {
        if (jaGuanyat) return;
        jaGuanyat = true;

        Debug.Log("Victoria! El jugador ha guanyat!");

        if (GameManagerIA.Instance != null)
        {
            GameManagerIA.Instance.Victory();
        }

        Debug.Log("Puntuació guardada: 100");
    }

    public void ResetGoal()
    {
        jaGuanyat = false;
    }
}