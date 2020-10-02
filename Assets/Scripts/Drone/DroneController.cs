using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Drone
{
    public class DroneController : MonoBehaviour
    {
        public WorldSimulation WorldSimulation;
        private List<Drone> _drones;

        void Start()
        {
            if (WorldSimulation == null)
            {
                Debug.LogError("World Simulation Object not attached to Drone Controller");
            }

            _drones = FindObjectsOfType<Drone>().ToList();
            Debug.Log($"Found { _drones.Count } drones");

            // test 
            _drones[0].ChangeModeOfOperation(ModeOfOperation.FlightToTarget, new Vector3(10, 0f, 10f));
        }

        void Update()
        {
            
        }
    }
}
