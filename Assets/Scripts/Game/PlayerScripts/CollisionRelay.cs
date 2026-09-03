using Game;
using UnityEngine;

public class CollisionRelay : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private void OnTriggerEnter(Collider collider)
    {
        playerController.HandleCollisionEnter(collider);
    }

    private void OnTriggerExit(Collider other)
    {
        playerController.HandleCollisionExit(other);
    }
}
