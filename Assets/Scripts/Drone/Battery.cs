using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Drone
{
    public class Battery : MonoBehaviour
    {
        public float Capacity;
        public float SwapTime;

        public Battery(float capacity, float swapTime)
        {
            this.Capacity = capacity;
            this.SwapTime = swapTime;
        }
    }
}
