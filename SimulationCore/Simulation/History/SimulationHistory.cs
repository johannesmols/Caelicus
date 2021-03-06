﻿using System.Collections.Generic;

namespace SimulationCore.Simulation.History
{
    /// <summary>
    /// Stores the entire history of a simulation and all its steps
    /// </summary>
    public class SimulationHistory
    {
        public SimulationParameters Parameters { get; }

        public List<SimulationHistoryStep> Steps = new List<SimulationHistoryStep>();

        public SimulationHistory(SimulationParameters parameters)
        {
            Parameters = parameters;
        }
    }
}
