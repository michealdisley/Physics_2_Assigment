using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSuspension : MonoBehaviour
{
    private Motor motor;

    // Float
    public float springForce;
    public float damperForce;
    public float springConstant;
    public float damperConstant;
    public float restLenght;

    private float previousLenght;
    private float currentLenght;
    private float springVelocity;

    // Other
    public Rigidbody rb;


    [Header("Suspension")]
    private bool tool;

	// Use this for initialization
	void Start ()
    {
        motor = transform.root.GetComponent<Motor>();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {

        // springConstant = rb.mass * 15;

        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, restLenght + .2f))
        {
            previousLenght = currentLenght;
            currentLenght = restLenght - (hit.distance - -.2f);
            springVelocity = (currentLenght - previousLenght / Time.fixedDeltaTime); // Resets the wheel back to its orgion point slowly.
            springForce = springConstant * currentLenght;
            damperForce = damperConstant * springVelocity;

            rb.AddForceAtPosition(transform.up * (springForce + damperForce), transform.position);
        }
	}
}
