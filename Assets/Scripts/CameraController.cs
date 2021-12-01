using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed;
    private float originalY;

    private void Start()
    {
        originalY = transform.position.y;
    }
    void Update()
    {
        CheckInputs();
    }

    private void CheckInputs()
    {
        this.transform.Translate(speed * Time.deltaTime * Vector3.forward, Space.World);
        this.transform.position = new Vector3(Mathf.Round(this.transform.position.x), originalY, Mathf.Round(this.transform.position.z));
    }
}
