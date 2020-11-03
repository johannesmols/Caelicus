using System;
using Caelicus.Models.Graph;
using Caelicus.Models.Vehicles;

namespace Caelicus.Simulation
{
    public enum VehicleState
    {
        MovingToTarget,
        MovingToBase,
        Steady
    }
    
    public class VehicleInstance : Vehicle
    {
        public VehicleInstance(Vehicle v, VertexInfo b) : base(v)
        {
            Base = b;
        }
        public VertexInfo Base {get; set; } 
        public VertexInfo Target { get; set; }
        public VehicleState State { get; set; }
        public float Distance { get; set; }
        
        public float TotalDistance { get; set; }

        public double Threshold { get; set; } = 0.0010;

        public void SetOrder(VertexInfo t)
        {
            State = VehicleState.MovingToTarget;
            Target = t;
        }

        /// <summary>
        /// Check if we arrived at the destination. Since the coordinates are floating points we should get distance
        /// between two points and compare it with a minimum threshold. The threshold for now is 0.10 but I think it
        /// is quite large!!.
        /// </summary>
        /// <returns></returns>
        public bool ArrivedToTarget()
        {
            return Distance <= Threshold;

        }
        
        public void SetTarget(VertexInfo t)
        {
            Target = t;
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

        public void ReturnToBase()
        {
            Target = Base;
        }

        public bool IsMoving()
        {
            return State == VehicleState.MovingToBase || State == VehicleState.MovingToTarget;
        }
    }
}