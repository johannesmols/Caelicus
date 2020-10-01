using System;
using Assets.Scripts.Drone;
using UnityEngine;

namespace Assets.Scripts.Drone
{
    public class Drone : MonoBehaviour
    {
        // Public properties
        public float FixedWeight = 10f;
        public float PayloadWeight = 0f;
        public float CruiseSpeed = 5f;
        public float BatteryCapacity = 300f;
        public float ChargingSpeed = 1f;
        public float PurchasingCost = 3000f;

        // Private properties
        private ModeOfOperation _modeOfOperation;
        private Vector3 _currentTarget;
        private float _batteryCharge;

        void Start()
        {
            _batteryCharge = BatteryCapacity;
        }

        void Update()
        {
            Debug.Log(gameObject.name + " is in mode " + _modeOfOperation);

            switch (_modeOfOperation)
            {
                case ModeOfOperation.Idle:
                    // do nothing
                    break;
                case ModeOfOperation.FlightToTarget:
                    // fly towards target on cruise altitude with cruise speed
                    // this doesn't work properly, just a mockup to make the drone move
                    gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, _currentTarget, CruiseSpeed * Time.deltaTime);
                    break;
                case ModeOfOperation.FlightToBase:
                    // fly towards base on cruise altitude with cruise speed
                    break;
                default:
                    break;
            }
        }

        // Return value: success or failure to switch mode
        public bool ChangeModeOfOperation(ModeOfOperation mode, Vector3 target = new Vector3())
        {
            switch (mode)
            {
                case ModeOfOperation.Idle:
                    _modeOfOperation = mode;
                    return true;
                case ModeOfOperation.FlightToTarget:
                    if (target == Vector3.zero) return false;
                    _modeOfOperation = mode;
                    _currentTarget = target;
                    return true;
                case ModeOfOperation.FlightToBase:
                    if (target == Vector3.zero) return false;
                    _modeOfOperation = mode;
                    _currentTarget = target;
                    return true;
                default:
                    return false;
            }
        }
    }
}
