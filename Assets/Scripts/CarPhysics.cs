using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class WheelInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool speed; // is this wheel controlled by speed??
    public bool steering; // is this front or rear wheel??
}

internal enum CarDriveType
{
    FrontWheelDrive,
    RearWheelDrive,
    FourWheelDrive
}

internal enum SpeedType
{
    MPH,
    KPH
}

public class CarPhysics : MonoBehaviour
{
    // Ints
    private int gearNum;
    private int count;

    [SerializeField] private static int NoOfGears = 5;

    // Floats
    public float CurrentSpeed { get { return rb.velocity.magnitude * 2.23693629f; } }
    public float maxSpeed { get { return topSpeed; } }
    public float BrakeInput { get; private set; }
    public float AccelInput { get; private set; }

    // private float speed = 50f;

    [SerializeField] private float topSpeed = 200;
    [SerializeField] private float torqueOverall;
    private float currentTorque;
    private float steeringAngle;
    [SerializeField] private float maxSteeringAngle;
    [SerializeField] private float breakTorque;
    [SerializeField] private float maxBreakTorque;
    [SerializeField] private float reverseTorque;
    [SerializeField] private float downforce = 100f;
    [Range(0, 1)] [SerializeField] private float tractionControl; // 0 is no traction control, 1 is full interference
    [SerializeField] private float slipLimit;

    // Other
    public Text countText;

    private Rigidbody rb;

    [SerializeField] private CarDriveType carDriveType = CarDriveType.FourWheelDrive;
    [SerializeField] private WheelCollider[] wheelCol = new WheelCollider[4];
    [SerializeField] private GameObject[] wheelMesh = new GameObject[4];
    [SerializeField] private SpeedType speedType;
    private Quaternion[] wheelMeshRot;

    public void ApplyLcalPostionToVisual(WheelCollider col)
    {
        if (col.transform.childCount == 0)
            return;
        Transform visualWheel = col.transform.GetChild(0);

        Vector3 postion;
        Quaternion rotation;
        col.GetWorldPose(out postion, out rotation);

        visualWheel.transform.position = postion;
        visualWheel.transform.rotation = rotation;

    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    private void Start()
    {
        count = 0;
        SetCountText();

        wheelMeshRot = new Quaternion[4];
        for (int i = 0; i < 4; i++)
        {
            wheelMeshRot[i] = wheelMesh[i].transform.localRotation;
        }

        maxBreakTorque = float.MaxValue;

        currentTorque = torqueOverall - (tractionControl * torqueOverall);
    }

    //void FixedUpdate()
    //{
    //    float moveHorizontal = Input.GetAxis("Horizontal");
    //    float moveVertical = Input.GetAxis("Vertical");

    //    Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

    //    rb.AddForce(movement * speed);
    //}

    private void GearChanging()
    {
        float f = Mathf.Abs(CurrentSpeed / maxSpeed);
        float upgearlimit = (1 / (float)NoOfGears) * (gearNum + 1);
        float downgearlimit = (1 / (float)NoOfGears) * gearNum;

        if (gearNum > 0 && f < downgearlimit)
        {
            gearNum--;
        }

        if (f > upgearlimit && (gearNum < (NoOfGears - 1)))
        {
            gearNum++;
        }
    }

    public void Move(float steering, float accel, float footbrake, float handbrake)
    {
        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 position;
            wheelCol[i].GetWorldPose(out position, out quat);
            wheelMesh[i].transform.position = position;
            wheelMesh[i].transform.rotation = quat;
        }

        //clamp input values
        steering = Mathf.Clamp(steering, -1, 1);
        AccelInput = accel = Mathf.Clamp(accel, 0, 1);
        BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        //Set the steer on the front wheels.
        //Assuming that wheels 0 and 1 are the front wheels.
        steeringAngle = steering * maxSteeringAngle;
        wheelCol[0].steerAngle = steeringAngle;
        wheelCol[1].steerAngle = steeringAngle;

        ApplyDrive(accel, footbrake);
        CapSpeed();

        //Set the handbrake.
        //Assuming that wheels 2 and 3 are the rear wheels.
        if (handbrake > 0f)
        {
            var hbTorque = handbrake * maxBreakTorque;
            wheelCol[2].brakeTorque = hbTorque;
            wheelCol[3].brakeTorque = hbTorque;
        }


        GearChanging();

        Adddownforce();
        TractionControl();
    }

    // Calculate the speed based on the speedtype.
    private void CapSpeed()
    {
        float speed = rb.velocity.magnitude;

        switch (speedType)
        {
            case SpeedType.MPH:

                speed *= 2.2f;
                if (speed > topSpeed)
                    rb.velocity = (topSpeed / 2.2f) * rb.velocity.normalized;
                break;

            case SpeedType.KPH:
                speed *= 3.6f;
                if (speed > topSpeed)
                    rb.velocity = (topSpeed / 3.6f) * rb.velocity.normalized;
                break;
        }
    }

    // Applys the proper torque to the proper wheels.
    private void ApplyDrive(float accel, float footbrake)
    {
        float thrustTorque;

        switch (carDriveType)
        {
            case CarDriveType.FourWheelDrive:
                thrustTorque = accel * (currentTorque / 4f);
                for (int i = 0; i < 4; i++)
                {
                    wheelCol[i].motorTorque = thrustTorque;
                }
                break;

            case CarDriveType.FrontWheelDrive:
                thrustTorque = accel * (currentTorque / 2f);
                wheelCol[0].motorTorque = wheelCol[1].motorTorque = thrustTorque;
                break;

            case CarDriveType.RearWheelDrive:
                thrustTorque = accel * (currentTorque / 2f);
                wheelCol[2].motorTorque = wheelCol[3].motorTorque = thrustTorque;
                break;

        }


        for (int i = 0; i < 4; i++)
        {
            if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, rb.velocity) < 50f)
            {
                wheelCol[i].brakeTorque = breakTorque * footbrake;
            }
            else if (footbrake > 0)
            {
                wheelCol[i].brakeTorque = 0f;
                wheelCol[i].motorTorque = -reverseTorque * footbrake;
            }
        }

    }

    // this is used to add more grip in relation to speed
    private void Adddownforce()
    {
        wheelCol[0].attachedRigidbody.AddForce(-transform.up * downforce *
                                                     wheelCol[0].attachedRigidbody.velocity.magnitude);
    }

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        WheelHit wheelHit;
        switch (carDriveType)
        {
            case CarDriveType.FourWheelDrive:
                // loop through all wheels
                for (int i = 0; i < 4; i++)
                {
                    wheelCol[i].GetGroundHit(out wheelHit);

                    AdjustTorque(wheelHit.forwardSlip);
                }
                break;

            case CarDriveType.RearWheelDrive:
                wheelCol[2].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);

                wheelCol[3].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
                break;

            case CarDriveType.FrontWheelDrive:
                wheelCol[0].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);

                wheelCol[1].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
                break;
        }
    }


    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= slipLimit && currentTorque >= 0)
        {
            currentTorque -= 10 * tractionControl;
        }
        else
        {
            currentTorque += 10 * tractionControl;
            if (currentTorque > torqueOverall)
            {
                currentTorque = torqueOverall;
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("FinishLine"))
        {
            // other.gameObject.SetActive(false);
            count = count + 1;
            SetCountText();
        }

        else if(other.gameObject.CompareTag("Pick up"))
        {
            other.gameObject.SetActive(false);

        }
    }

    void SetCountText()
    {
        countText.text = "Lap: " + count.ToString();
        if (count >= 3)
        {
            // scene will be applied later.
        }
    }

}
