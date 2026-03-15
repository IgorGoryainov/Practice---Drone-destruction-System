using UnityEngine;

public class MoveScript : MonoBehaviour
{
    public Transform emptyObjectTargetPos;

    private static readonly Vector3 Origin = Vector3.zero;
    private const float LerpStep = 0.0001f;

    void Start()
    {
        transform.position = new Vector3(0, 50, 500);
    }

    void FixedUpdate()
    {
        emptyObjectTargetPos.position = Origin;
        transform.position = Vector3.Lerp(transform.position, emptyObjectTargetPos.position, LerpStep);
    }
}
