using UnityEngine;

public class SeguimentOcell : MonoBehaviour{
    public Transform playerTarget;
    public Transform Target { get { return playerTarget; } set { playerTarget = value; } }
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    void LateUpdate()
    {
        if (playerTarget == null) return;

        Vector3 desiredPosition = playerTarget.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }


// ESTA ES LA FUNCIÓN QUE TE FALTA Y POR LA QUE DA ERROR:
    public void SetTarget(Transform newTarget)
    {
        playerTarget = newTarget;
        Debug.Log("[Càmera] Nou objectiu assignat: " + newTarget.name);
    }
}