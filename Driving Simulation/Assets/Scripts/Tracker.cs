using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;

public class Tracker : MonoBehaviour
{
    //used for driving "AI"

    public GameObject tracker;
    public GameObject myCar;
    private string myTag;
    public bool isOvertaking = false;
    private Overtaking overtaking;

    public List<GameObject> waypoints;
    public int currentWaypoint = 0;

    public int condition; //experiment condition; 1 = SA L2; 2 = SA L3
    public SerialPort port;
    string amplitude;
    string frequency;
    ControlManager manager;

    public GameObject label;
    public GameObject panel;
    bool reset = false;

    void Start() 
    {
        myTag = tracker.tag;
        condition = 1;
        if (myTag == "MyCar")
        {
            overtaking = myCar.GetComponent<Overtaking>();
            manager = myCar.GetComponent<ControlManager>();
        }
    }

    void FixedUpdate() 
    {
        if (manager != null)
        {
            reset = manager.resetTracker;
        }
        if (reset)
        {
            //stops ongoing overtaking; tracker is reset to middle of right lane
            currentWaypoint = 0;
            transform.position = new Vector3(0, transform.position.y, myCar.transform.position.z + 50);
            isOvertaking = false;
            myCar.GetComponent<spawnCars>().newAction = true;
            overtaking.timer = -1;
            manager.resetTracker = false;
        }
        if (waypoints.Count > 5) 
        {
            //for some reason the overtaking points around the lead car are doubled sometimes; remove duplicates
            for (int i = waypoints.Count-1; i >= 0; i--) 
            {
                if (waypoints[i] == null) 
                {
                    waypoints.RemoveAt(i);
                }
            }
        }
        if (isOvertaking)
        {
            if (currentWaypoint == 0)
            {
                tracker.transform.position = new Vector3(waypoints[currentWaypoint].transform.position.x, tracker.transform.position.y, waypoints[currentWaypoint].transform.position.z);
                currentWaypoint++;
            }
        }
    }


    IEnumerator OnTriggerEnter(Collider collision) {
        if (collision.gameObject.CompareTag(myTag)) 
        {
            if (myTag == "MyCar")
            {
                if (waypoints.Count > 0 && isOvertaking)
                {
                    if (condition == 2)
                    {
                        port = manager.port;
                        switch (currentWaypoint)
                        {
                            case 1:
                                manager.VestMessage("000100", port);
                                break;
                            case 2:
                                manager.VestMessage("001000", port);
                                break;
                            default:
                                break;
                        }
                    }
                    if (currentWaypoint == waypoints.Count) //overtake done
                    {
                        overtaking.ResetMotors(0); //redundant, for more robustness
                        currentWaypoint = 0;
                        isOvertaking = false;
                        myCar.GetComponent<spawnCars>().newAction = true;
                        overtaking.timer = -1;
                        tracker.transform.position = new Vector3(0, tracker.transform.position.y, tracker.transform.position.z + 50);
                    }
                    else
                    {
                        tracker.transform.position = new Vector3(waypoints[currentWaypoint].transform.position.x, tracker.transform.position.y, waypoints[currentWaypoint].transform.position.z);
                        currentWaypoint++;
                    }
                }
            }
            this.GetComponent<BoxCollider>().enabled = false;
            if (myTag == "DriverlessCarOpposite")
            {
                tracker.transform.position = tracker.transform.position - transform.forward * 50;
            }
            else
            {
                tracker.transform.position = tracker.transform.position + transform.forward * 50;
            }
            yield return new WaitForSeconds(0.5f);
            this.GetComponent<BoxCollider>().enabled = true;
        }
    }

}

