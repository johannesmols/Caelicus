using System;

namespace SimulationCore.Simulation
{
    public class SimulationProgress
    {
        public Guid SimulationIdentifier { get; }
        public string Message { get; set; }

        public SimulationProgress(Guid identifier, string message)
        {
            SimulationIdentifier = identifier;
            Message = message;
        }
    }
}
