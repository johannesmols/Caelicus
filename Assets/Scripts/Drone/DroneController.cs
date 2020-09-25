using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Drone
{
    public class DroneController : MonoBehaviour
    {
        private List<Drone> _drones;

        void Start()
        {
            _drones = FindObjectsOfType<Drone>().ToList();
            Debug.Log($"Found { _drones.Count } drones");
            _drones[0].ChangeModeOfOperation(ModeOfOperation.FlightToTarget, new Vector3(10, 0f, 10f));
        }

        void Update()
        {
            
        }
    }
}
