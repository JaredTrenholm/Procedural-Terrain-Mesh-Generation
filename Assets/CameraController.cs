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
        if (Input.GetKey(KeyCode.W))
        {
            this.transform.Translate(speed * Time.deltaTime * Vector3.forward);
        } else if (Input.GetKey(KeyCode.S))
        {
            this.transform.Translate(speed * Time.deltaTime * Vector3.back);
        }
        
        if (Input.GetKey(KeyCode.A))
        {
            this.transform.Translate(speed * Time.deltaTime * Vector3.left);
        } else if (Input.GetKey(KeyCode.D))
        {
            this.transform.Translate(speed * Time.deltaTime * Vector3.right);
        }

        this.transform.position = new Vector3(Mathf.Round(this.transform.position.x), originalY, Mathf.Round(this.transform.position.z));
    }
}
