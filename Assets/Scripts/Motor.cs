using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Motor : MonoBehaviour
{

    // Float
    public float speed = 100f;
    public float turnRadius = 5;
    public float upForce = 50f;
    public float hoverHeight = 4f;

    private float powerInput;
    private float turnInput;

    // Other
    private Rigidbody rb;

	void Awake ()
    {
        rb = GetComponent<Rigidbody>();
	}
	
	void Update ()
    {
        powerInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
	}

    private void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, hoverHeight))
        {
            float thrust = (hoverHeight - hit.distance) / hoverHeight;

            Vector3 hoverForce = Vector3.up * thrust * upForce;
            rb.AddForce(hoverForce, ForceMode.Acceleration);
        }

        rb.AddRelativeForce(0f, 0f, powerInput * speed);
        rb.AddRelativeTorque(0f, turnInput * turnRadius, 0f);
    }
}
