using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinMover : MonoBehaviour
{

    public Vector3 moveDir;
    public float moveSpeed = 2;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDir, moveSpeed * Time.fixedDeltaTime);

        if(transform.position.x > 14 || transform.position.x < -14)
        {
            moveDir = new Vector3(-moveDir.x, moveDir.y, moveDir.z);
        }
        if (transform.position.y > 14 || transform.position.y < -14)
        {
            moveDir = new Vector3(moveDir.x, -moveDir.y, moveDir.z);
        }
    }
}
