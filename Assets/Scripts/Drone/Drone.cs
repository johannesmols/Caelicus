using System;
using System.Collections;
using Assets.Scripts.Drone;
using UnityEngine;

namespace Assets.Scripts.Drone
{
    public class Drone : MonoBehaviour
    {
        // Public properties
        public float DroneWeight = 10f;
        public float CruiseSpeed = 5f;
        public BaseStation CurrentBaseStation;

        // Private properties
        private ModeOfOperation _modeOfOperation;
        private Vector3 _currentTarget;
        private Battery _installedBattery;
        private float _currentPayloadWeight;

        void Start()
        {
            if (CurrentBaseStation == null)
            {
                Debug.LogError($"{ gameObject.name } does not have a base station defined at the start of the simulation");
            }
        }

        void Update()
        {
            switch (_modeOfOperation)
            {
                case ModeOfOperation.Idle:
                    // do nothing
                    break;
                case ModeOfOperation.FlightToTarget:
                    // fly towards target with cruise speed
                    break;
                case ModeOfOperation.FlightToBase:
                    // fly towards base with cruise speed
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
                    if (target == Vector3.zero || _installedBattery == null || _installedBattery.Capacity <= 0f) return false;
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

        public IEnumerator SwapBatteries(Battery newBattery)
        {
            _installedBattery = null;
            yield return new WaitForSeconds(newBattery.SwapTime);
            _installedBattery = newBattery;
        }

        public IEnumerator LoadCargo(float weight, float loadingTime)
        {
            if (_currentPayloadWeight <= 0f)
            {
                Debug.LogError($"{ gameObject.name } still has payload attached, can't load more");
                yield return null;
            }

            yield return new WaitForSeconds(loadingTime);
            _currentPayloadWeight = weight;
        }
    }
}
