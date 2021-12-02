using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed;
    void Update()
    {
        Movement();
    }

    private void Movement()
    {
        this.transform.Translate(speed * Time.deltaTime * Vector3.forward, Space.World);
    }
}
