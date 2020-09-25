using UnityEngine;

namespace Assets.Scripts.Objects
{
    public class Drone : MonoBehaviour
    {
        public double Weight = 10d;
        public double BatteryCapacity = 300d;
        public double ChargingTime = 60d;

        void Start()
        {
            
        }

        void Update()
        {
            gameObject.transform.Translate(Vector3.forward * Time.deltaTime);
        }
    }
}
