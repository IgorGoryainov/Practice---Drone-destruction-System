using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveScript : MonoBehaviour
{
    public Transform myTransform;
    public Transform emtyObjectTargetPos;

    void Start()
    {
        myTransform.position = new Vector3(0, 50, 500);
    }

    void FixedUpdate()
    {
        emtyObjectTargetPos.position = new Vector3(0, 0, 0);
        myTransform.position =
            Vector3
                .Lerp(myTransform.position, emtyObjectTargetPos.position, 0.0001f);
    }
}
