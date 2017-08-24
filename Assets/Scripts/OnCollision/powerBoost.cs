using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerBoost : MonoBehaviour
{

    // Other
    public CarMotor car;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            car = other.gameObject.GetComponent<CarMotor>(); // not sure about the syntax here...
            if (car)
            {
                // We speed up the player and then tell to stop after a few seconds
                car.rb.AddTorque(0, 0, car.speedBoost);

                car.speedBoost = 1.5f;
                StartCoroutine(car.StopSpeedUp());
            }
            Destroy(gameObject);
        }
    }

}
