using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Drone;
using UnityEngine;

namespace Assets.Scripts
{
    public class BaseStation : MonoBehaviour
    {
        private Vector3 _location;
        public float CargoLoadingTime;
        public List<Battery> BatteryTypesAvailable; // assuming unlimited supply of all battery types

        public BaseStation(float cargoLoadingTime)
        {
            CargoLoadingTime = cargoLoadingTime;
        }

        void Start()
        {
            _location = gameObject.gameObject.transform.position;
        }
    }
}
