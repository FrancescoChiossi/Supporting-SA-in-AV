using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using System.Linq;
using UnityEngine.SceneManagement;

public class spawnCars : MonoBehaviour
{
    //destroys or instantiates cars when encounters begin/end

    Overtaking overtaking;
    int trials; 
    private List<int> encounterCases = new List<int>();
    int encounterNum = 0;
    float minRange = 400; //minimum range between myCar and the new driverlessCar
    float maxRange = 700;
    public GameObject driverlessCar;
    public GameObject driverlessCarOpposite;
    public GameObject driverlessCarTracker;
    public GameObject driverlessCarOppositeTracker;
    float offset = 4.73f; //offset of lane midpoints
    public GameObject myTrackerObject;
    private Tracker myTracker;
    ControlManager manager;

    GameObject newOppCar;
    GameObject newCar;
    GameObject newTracker;
    GameObject newOppTracker;
    public bool newAction; //trigger for spawning

    void Start()
    {
        overtaking = GetComponent<Overtaking>();
        newAction = false;
        myTracker = myTrackerObject.GetComponent<Tracker>();
        manager = GetComponent<ControlManager>();
        if (SceneManager.GetActiveScene().name == "Driving")
        {
            trials = 21;
        }
        else
        {
            trials = 6;
        }
        for (int i = 0; i < trials / 3; i++)
        {
            encounterCases.Add(1);
            encounterCases.Add(3);
            encounterCases.Add(4);
        }
    }

    void FixedUpdate()
    {
        int action = 0;
        if (newAction)
        {
            manager.VestMessage("000000", manager.port); //reset vest
            if (encounterCases.Count == 0)
            {
                if (trials == 21)
                {
                    overtaking.action = action = 2; //critical action
                }
                else
                {
                    manager.QuitDataCollection();
                    SceneManager.LoadScene("PredrivingText", LoadSceneMode.Single);
                }
            }
            else
            {
                action = CalculateScenario();
                overtaking.action = action;
            }
            float newZ = transform.position.z + Random.Range(minRange, maxRange);
            Destroy(newCar);
            if (action != 4)
            {
                newCar = Instantiate(driverlessCar) as GameObject;
                newCar.transform.position = new Vector3(0, transform.position.y, newZ);

                Destroy(newTracker);
                newTracker = Instantiate(driverlessCarTracker) as GameObject;
                newTracker.transform.position = new Vector3(0, 0.5f, newZ + 50f);
                newCar.GetComponent<CarAIControl>().SetTarget(newTracker.transform);
                overtaking.driverlessCar = newCar.transform;

                myTracker.waypoints.Clear();
                myTracker.waypoints = GameObject.FindGameObjectsWithTag("Waypoint").ToList<GameObject>();
                myTracker.waypoints = myTracker.waypoints.OrderBy(x => x.transform.position.z).ToList<GameObject>();
                overtaking.driverlessCar = newCar.transform;
            }
            Destroy(newOppCar);
            if (action != 3) {
                float gap;
                if (action == 1) 
                //position the opp car further away to account for the speed decrease when approaching the driverless car
                {
                    gap = 140;
                }
                else
                {
                    gap = 60;
                }
                float t = (newZ  + gap - transform.position.z) / 20;
                /*time after which myCar.z = driverlessCar.z
                 * 20 is the speed difference between the cars in mps
                 * t is used for the positioning of the opp car to ensure the cars meet
                 */            
                newOppCar = Instantiate(driverlessCarOpposite) as GameObject;
                newOppCar.transform.position = new Vector3(0 - offset, driverlessCarOpposite.transform.position.y, newZ + (30 * t));
                overtaking.oppCar = newOppCar.transform;

                Destroy(newOppTracker);
                newOppTracker = Instantiate(driverlessCarOppositeTracker) as GameObject;
                newOppTracker.transform.position = new Vector3(0 - offset, 0.5f, newOppCar.transform.position.z - 50f);
                newOppCar.GetComponent<CarAIControl>().SetTarget(newOppTracker.transform);
            }
            newAction = false;
        }        
    }

    int CalculateScenario()
    {
        int index = Random.Range(0, encounterCases.Count);
        int value = encounterCases[index];
        encounterCases.RemoveAt(index);

        return value;
    }
}
