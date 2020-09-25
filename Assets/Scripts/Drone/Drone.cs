using System;
using Assets.Scripts.Drone;
using UnityEngine;

namespace Assets.Scripts.Drone
{
    public class Drone : MonoBehaviour
    {
        // Public properties
        public float Weight = 10f;
        public float CruiseSpeed = 5f;
        public float CruiseAltitude = 100f;
        public float BatteryCapacity = 300f;
        public float ChargingSpeed = 1f;

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
                case ModeOfOperation.Charging:
                    // charge battery
                    if (_batteryCharge >= BatteryCapacity)
                    {
                        _modeOfOperation = ModeOfOperation.Idle;
                    }
                    else
                    {
                        _batteryCharge += ChargingSpeed * Time.deltaTime; // charge battery by ChargingSpeed per second
                    }
                    break;
                case ModeOfOperation.Launch:
                    // launch straight forward and climb to cruise altitude
                    break;
                case ModeOfOperation.FlightToTarget:
                    // fly towards target on cruise altitude with cruise speed
                    // this doesn't work properly, just a mockup to make the drone move
                    gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, _currentTarget, CruiseSpeed * Time.deltaTime);
                    break;
                case ModeOfOperation.FlightToBase:
                    // fly towards base on cruise altitude with cruise speed
                    break;
                case ModeOfOperation.Landing:
                    // reduce altitude and speed to approach base
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
                case ModeOfOperation.Charging:
                    _modeOfOperation = mode;
                    return true;
                case ModeOfOperation.Launch:
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
                case ModeOfOperation.Landing:
                    _modeOfOperation = mode;
                    return true;
                default:
                    return false;
            }
        }
    }
}
