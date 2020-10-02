using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class WorldSimulation : MonoBehaviour
    {
        public float FixedBaseStationCost, FixedDroneCost;

        public List<BaseStation> BaseStations;
        public List<Vector3> Targets;

        // for later. could be updated in random intervals in the update method
        public Tuple<Vector3, float> WindDirectionAndSpeed;

        void Start()
        {
            if (BaseStations?.Count == 0 || Targets?.Count == 0)
            {
                Debug.LogError("No base stations or targets defined");
            }
        }

        void Update()
        {
        
        }
    }
}
