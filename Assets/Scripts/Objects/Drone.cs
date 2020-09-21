using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public Transform location;
    public double weight;
    public double batteryCapacity;
    public double chargingTime;

    void Start()
    {
        
    }

    void Update()
    {
        location.Translate(Vector3.forward * Time.deltaTime);
    }
}
