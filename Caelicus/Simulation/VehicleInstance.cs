using System;
using Caelicus.Graph;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation
{
    public enum VehicleState
    {
        MovingToTarget,
        MovingToBase,
        Idle
    }
    
    public class VehicleInstance : Vehicle
    {
        public VehicleInstance(Vehicle vehicle, Vertex<VertexInfo, EdgeInfo> startingVertex) : base(vehicle)
        {
            CurrentVertexPosition = startingVertex;
        }

        public Vertex<VertexInfo, EdgeInfo> CurrentVertexPosition {get; set; } 
        public Vertex<VertexInfo, EdgeInfo> Target { get; set; }

        public VehicleState State { get; set; }
        public double Distance { get; set; }
        public float TotalDistance { get; set; }
        public double Threshold { get; set; } = 0.0010;

        public void StartOrder(Vertex<VertexInfo, EdgeInfo> target)
        {
            State = VehicleState.MovingToTarget;
            Target = target;
        }

        public void ReturnToBase()
        {
            State = VehicleState.MovingToBase;
            Target = CurrentVertexPosition;
        }

        /// <summary>
        /// Check if we arrived at the destination. Since the coordinates are floating points we should get distance
        /// between two points and compare it with a minimum threshold. The threshold for now is 0.10 but I think it
        /// is quite large!!.
        /// </summary>
        /// <returns></returns>
        public bool ArrivedAtTarget()
        {
            return Distance <= Threshold;
        }
        
        public void SetTarget(Vertex<VertexInfo, EdgeInfo> target)
        {
            Target = target;
        }

        public void Advance()
        {
            Distance -= MovementCost();
        }

        /// <summary>
        /// TODO: complete the formula
        /// </summary>
        /// <returns></returns>
        private float MovementCost()
        {
            return 20f;
        }

        public bool IsMoving()
        {
            return State == VehicleState.MovingToBase || State == VehicleState.MovingToTarget;
        }
    }
}