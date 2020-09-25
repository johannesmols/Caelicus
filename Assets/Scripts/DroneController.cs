using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Objects;
using UnityEngine;

namespace Assets.Scripts
{
    public class DroneController : MonoBehaviour
    {
        private List<Drone> _drones;

        void Start()
        {
            _drones = FindObjectsOfType<Drone>().ToList();
            Debug.Log($"Found { _drones.Count } drones");
        }

        void Update()
        {
        
        }
    }
}
