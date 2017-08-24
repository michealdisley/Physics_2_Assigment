using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarMotor : MonoBehaviour
{
    [Header("Motor")]
    // Float
    public float idealRPM = 500f;
    public float maxRPM = 1500f;

    public float turnTradius = 10f;
    public float torque = 25f;
    public float brakeTorque = 100f;

    private float AntiRoll = 20000.0f;
    public float speedBoost = 1.0f;

    [Header("Wheel's")]
    public WheelCollider wheelFR;
    public WheelCollider wheelFL;
    public WheelCollider wheelRR;
    public WheelCollider wheelRL;

    // Other
    // public Text speedometer;
    public Transform centerOfGravity;

    public Rigidbody rb;

    public enum DriveMode { Front, Rear, All};
    public DriveMode driveMode = DriveMode.Rear;



    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rb.centerOfMass = centerOfGravity.localPosition;
    }

    public float Speed()
    {
        return wheelRR.radius * Mathf.PI * wheelRR.rpm * 60f / 100f;
    }

    public float Rpm()
    {
        return wheelRL.rpm;
    }

    private void FixedUpdate()
    {
        // if(speedmoter!=null)
        // speedometer.text = "Speed: " + SPeed().ToString("f0") + " km/h";

        float scaledTorque = Input.GetAxis("Vertical") * torque * speedBoost;

        if (wheelRL.rpm < idealRPM)
            scaledTorque = Mathf.Lerp(scaledTorque / 10f, scaledTorque, wheelRL.rpm / idealRPM);
        else
        {
            scaledTorque = Mathf.Lerp(scaledTorque, 0, (wheelRL.rpm-idealRPM) / (maxRPM-idealRPM) ) ;
        }

        // reduces the chance of the car rolling over
        // csets center of gravity.
        DoRollBar(wheelFR, wheelFL);
        DoRollBar(wheelRR, wheelRL);

        wheelFR.steerAngle = Input.GetAxis("Horizontal") * turnTradius; // Sets the Front Right wheel to move when button is pushed.
        wheelFL.steerAngle = Input.GetAxis("Horizontal") * turnTradius; // Sets the Front Left wheel to move when button is pushed.

        // Based on wheel settings applys speed to the wheels.
        wheelFR.motorTorque = driveMode == DriveMode.Rear ? 0 : scaledTorque; // applys no speed to the front wheels.
        wheelFL.motorTorque = driveMode == DriveMode.Rear ? 0 : scaledTorque; // applys no speed to the front wheels.
        wheelRR.motorTorque = driveMode == DriveMode.Front ? 0 : scaledTorque; // applys no speed to the rear wheels.
        wheelRL.motorTorque = driveMode == DriveMode.Front ? 0 : scaledTorque; // applys no speed to the rear wheels.

        if (Input.GetButton("Jump"))
        {
            wheelFR.brakeTorque = brakeTorque;
            wheelFL.brakeTorque = brakeTorque;
            wheelRR.brakeTorque = brakeTorque;
            wheelRL.brakeTorque = brakeTorque;
        }
        else
        {
            wheelFR.brakeTorque = 0;
            wheelFL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
            wheelRL.brakeTorque = 0;
        }
    }

    // helpes prevent the car from rolling over.
    void DoRollBar(WheelCollider WheelL, WheelCollider WheelR)
    {
        WheelHit hit;
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = WheelL.GetGroundHit(out hit);
        if (groundedL)
            travelL = (WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;

        bool groundedR = WheelR.GetGroundHit(out hit);
        if (groundedR)
            travelR = (WheelR.transform.InverseTransformPoint(hit.point).y - WheelR.radius) / WheelR.suspensionDistance;

        float antiRollForce = (travelL - travelR) * AntiRoll;

        if (groundedL)
            rb.AddForceAtPosition(WheelL.transform.up * -antiRollForce, WheelL.transform.position);

        if (groundedR)
            rb.AddForceAtPosition(WheelR.transform.up * antiRollForce, WheelR.transform.position);
    }


    public IEnumerator StopSpeedUp()
    {
        yield return new WaitForSeconds(2.5f); // the number corresponds to the number of seconds the speed up will be applied
        speedBoost = 1.0f; // back to normal !
    }
}
