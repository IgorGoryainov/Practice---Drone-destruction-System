using UnityEngine;

public class CollisionCheck : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Hit {collision.gameObject.name}");
    }
}
