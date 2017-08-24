using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (CarPhysics))]
    public class CarUserControl : MonoBehaviour
    {
        // Scripts
        private CarPhysics car; // the car controller we want to use

        private void Awake()
        {
            // get the car controller
            car = GetComponent<CarPhysics>();
        }

        private void FixedUpdate()
        {
            // pass the input to the car!
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
#if !MOBILE_INPUT
            float handbrake = CrossPlatformInputManager.GetAxis("Jump");
            car.Move(h, v, v, handbrake);
#else
            car.Move(h, v, v, 0f);
#endif
        }
    }
}
