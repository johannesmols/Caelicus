[![Deploy to GitHub Pages](https://github.com/johannesmols/Caelicus/workflows/Deploy%20to%20GitHub%20Pages/badge.svg)](https://johannesmols.github.io/Caelicus/)

[![GitHub contributors](https://img.shields.io/github/contributors/johannesmols/Caelicus.svg)](https://github.com/johannesmols/Caelicus/graphs/contributors) ![Size](https://github-size-badge.herokuapp.com/johannesmols/Caelicus.svg)

[![GitHub issues](https://img.shields.io/github/issues/johannesmols/Caelicus.svg)](https://GitHub.com/johannesmols/Caelicus/issues/) [![GitHub issues-closed](https://img.shields.io/github/issues-closed/johannesmols/Caelicus.svg)](https://GitHub.com/johannesmols/Caelicus/issues?q=is%3Aissue+is%3Aclosed) [![Average time to resolve an issue](http://isitmaintained.com/badge/resolution/johannesmols/Caelicus.svg)](http://isitmaintained.com/project/johannesmols/Caelicus "Average time to resolve an issue")

[![GitHub pull-requests](https://img.shields.io/github/issues-pr/johannesmols/Caelicus.svg)](https://GitHub.com/johannesmols/Caelicus/pulls/) [![GitHub pull-requests closed](https://img.shields.io/github/issues-pr-closed/johannesmols/Caelicus.svg)](https://github.com/johannesmols/Caelicus/pulls?q=is%3Apr+is%3Aclosed)

# 💨 Caelicus 🩺

A project in the course of Model-Based Systems Engineering at the Technical University of Denmark ([02223](https://kurser.dtu.dk/course/02223))

## Main question

- How would a transport network consisting of drones compare to established networks (e.g. trucks, trains, boats, airplanes) if measured in terms of average delivery time and cost efficiency? In which scenarios, if any, would drones be the best solution?

## Assumptions: 

1. Collision with other drones or aircrafts is not of concern 
1. Terrain and obstacles are not considered
1. Vehicles are loaded and fueled automatically and without delay
1. Unlimited supply of fuel/electricity at the base stations
1. There is always a constant communication link between the vehicles and the central coordination system, and there is no delay

## Simulation Parameters 

The following parameters describe values that the simulation can be evaluated upon, not the runtime variables.

### Vehicle

- Movement
    1. Speed (km/h)
    1. Maximum range (km)
    1. Movement penalty (e.g. `1.5x` for road vehicles due to traffic and road networks)
- Transport
    1. Maximum payload weight (kg)
- Cost
    1. Cost per hour (€)
    1. Cost per km (€)

### World

1. Wind direction and speed (heading and m/s)
1. List of orders (_should be many and randomly generated to ensure equal chances for all vehicle types, and allows for analysis which vehicles can fulfill which orders most efficiently_)
    - Origin and destination vertex
    - Payload weight

### Transportation network

1. Number of vehicles
1. Initial distribution of vehicles (if multiple bases)

## Resources

- Current application: https://johannesmols.github.io/Caelicus/

- Mockups: https://www.figma.com/file/9LLukX1RzZNmQ4a6vpPVAq/Caelicus

## Tech Stack
- C#
- Blazor WebAssembly
- Magic 🔮

## The Team

<div align="left">
    <img src="https://cultofthepartyparrot.com/flags/hd/italyparrot.gif" width="30" height="30"/>
    <img src="https://cultofthepartyparrot.com/flags/hd/denmarkparrot.gif" width="30" height="30"/>
    <img src="https://cultofthepartyparrot.com/flags/hd/germanyparrot.gif" width="30" height="30"/>
    <img src="https://cultofthepartyparrot.com/flags/hd/germanyparrot.gif" width="30" height="30"/>
    <img src="https://cultofthepartyparrot.com/flags/hd/denmarkparrot.gif" width="30" height="30"/>
    <img src="https://cultofthepartyparrot.com/flags/hd/romaniaparrot.gif" width="30" height="30"/>
</div>
