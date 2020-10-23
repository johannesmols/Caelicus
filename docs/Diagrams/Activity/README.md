
# Table of Contents

1.  [How I immagine the simulation to works](#orgacbfe1a)
    1.  [A bad State machine diagram](#org0d0bb18)
    2.  [Start new mission](#orge8395a1)
    3.  [Check drone arrived to target](#orgab243be)
    4.  [Check completed mission](#org00f363e)
    5.  [Update wind conditions](#orgdcb777f)
    6.  [Update Drone position](#org6e4a1d0)


<a id="orgacbfe1a"></a>

# How I immagine the simulation to works

There are two main task, generating events and processing events.
The simulation ends whene there are no more clock events.

![img](general-view.png)

-   A clock event is just a normal advancment of time in the simulation.
-   The wind event is random and can happen or not during the current time.
-   The mission event is generated only if there is a mission scheduled for that particular clock time.

The simulations end when the simulator reach the max ammount of time


<a id="org0d0bb18"></a>

## A bad State machine diagram

![img](bad-state-machine.png)


<a id="orge8395a1"></a>

## Start new mission

![img](start-new-mission.png)


<a id="orgab243be"></a>

## Check drone arrived to target

![img](arrived-to-target.png)


<a id="org00f363e"></a>

## Check completed mission

![img](completed-mission.png)


<a id="orgdcb777f"></a>

## Update wind conditions

![img](update-wind-conditions.png)


<a id="org6e4a1d0"></a>

## Update Drone position

![img](update-drone-position.png)

