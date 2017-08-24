using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAudio : MonoBehaviour
{

    public AudioSource jet;

    private float jetPitch;
    private const float LowPitch = .1f;
    private const float HighPitch = 2.0f;
    private const float SpeedToRevs = .01f;

    // Other
    Vector3 velocity;
    private Rigidbody rb;


	void Awake ()
    {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        velocity = rb.velocity;

        float forwardSpeed = transform.InverseTransformDirection(rb.velocity).z; // gets the speed from the rb speed

        float engineRevs = Mathf.Abs(forwardSpeed) * SpeedToRevs; // caps the value to always be postive.

        jet.pitch = Mathf.Clamp(engineRevs, LowPitch, HighPitch);

	}
}
