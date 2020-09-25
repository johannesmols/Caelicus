using System;
using Assets.Scripts.Drone;
using UnityEngine;

namespace Assets.Scripts.Drone
{
    public class Drone : MonoBehaviour
    {
        // Public properties
        public float Weight = 10f;
        public float BatteryCapacity = 300f;
        public float ChargingSpeed = 1f;

        // Private properties
        private ModeOfOperation _modeOfOperation;
        private Vector3 _currentTarget;
        private float _batteryCharge;

        void Start()
        {
            _modeOfOperation = ModeOfOperation.Idle;
            _batteryCharge = BatteryCapacity;
        }

        void Update()
        {
            switch (_modeOfOperation)
            {
                case ModeOfOperation.Idle:
                    break;
                case ModeOfOperation.Charging:
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
