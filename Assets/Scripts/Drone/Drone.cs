using System;
using Assets.Scripts.Drone;
using UnityEngine;

namespace Assets.Scripts.Drone
{
    public class Drone : MonoBehaviour
    {
        // Public properties
        public double Weight = 10d;
        public double BatteryCapacity = 300d;
        public double ChargingTime = 60d;

        // Private properties
        private ModeOfOperation _modeOfOperation;

        void Start()
        {
            _modeOfOperation = ModeOfOperation.Idle;
        }

        void Update()
        {
            // just a test
            // gameObject.transform.Translate(Vector3.forward * Time.deltaTime);
        }

        public void ChangeModeOfOperation(ModeOfOperation mode, Vector3 target = new Vector3())
        {
            switch (mode)
            {
                case ModeOfOperation.Idle:
                    break;
                case ModeOfOperation.Charging:
                    break;
                case ModeOfOperation.Launch:
                    break;
                case ModeOfOperation.FlightToTarget:
                    break;
                case ModeOfOperation.FlightToBase:
                    break;
                case ModeOfOperation.Landing:
                    break;
                default:
                    break;
            }
        }
    }
}
