using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.UI;
using System.IO.Ports;

public class Overtaking : MonoBehaviour
{
    public int action { get; set; }
    public float distanceA = 2000; //distance to driverless car
    float distanceB = 2000; //distance to opposite car

    CarController controller;
    public float timer = -1; 
    public Transform driverlessCar;
    public Transform oppCar;
    public bool newAction { get; set; }
    float driverlessCarZ;
    float oppZ;
    float maxSpeed;
    float currentSpeed;
    public GameObject myTracker;

    float pitch;
    AudioSource sound;

    int condition;
    ControlManager manager;
    SerialPort port;

    bool overtaking;
    char[] motors;
    /*
     *  Motors:
     *  
     *  3   2
     *  4   1
     *  5   0
     */

    private void Awake()
    {
        controller = GetComponent<CarController>();
        manager = GetComponent<ControlManager>();
        sound = GetComponent<AudioSource>();
    }

    void Start()
    {
        condition = myTracker.GetComponent<Tracker>().condition;
        motors = new char[6];
        ResetMotors(0);
    }

    void FixedUpdate()
    {
        currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
        maxSpeed = controller.MaxSpeed;
        if (sound.loop == true)
        {
            //change sound depending on speed
            pitch = currentSpeed / 80;
        }
        else
        {
            pitch = 1.0f;
        }
        sound.pitch = pitch;
        overtaking = myTracker.GetComponent<Tracker>().isOvertaking;

        if (action == 0)
        {
            port = manager.port;
            //wait for car to take up speed
            if (currentSpeed >= 108f)
            {
                GetComponent<spawnCars>().newAction = true;
            }
        }

        if (driverlessCar != null)
        {
            driverlessCarZ = driverlessCar.position.z;
            distanceA = driverlessCarZ - transform.position.z;
        }
        else
        {
            distanceA = 2000;
        }
        if (oppCar != null)
        {
            oppZ = oppCar.position.z;
            distanceB = oppZ - transform.position.z;
        }
        else
        {
            distanceB = 2000;
        }
        if (action == 4 && distanceB < -10)
        {
            ResetMotors(2);
            GetComponent<spawnCars>().newAction = true;
        }
        if (action == 1) //waiting 
        {
            if (distanceA <= 80 && overtaking == false)
            { 
                if (maxSpeed > 36)
                {
                    if(condition == 2) manager.VestMessage("100000", port);
                    SlowDown();
                }
                else
                {
                    controller.MaxSpeed = 36;
                    UpdateTimer();
                    if (timer >= 6)
                    {
                        Overtake();
                        timer = -1;
                    }
                    else if (condition == 2 && timer > 3) manager.VestMessage("000001", port);
                }
            }
        }
        else if (action == 2 || action == 3) //overtake 
        {
            if (distanceA <= 60)
            {
                UpdateTimer();
                Overtake();
            }
            else if (condition == 2)
            {
                if (distanceA <= 100 && overtaking == false) manager.VestMessage("000001", port);
                else if (distanceA <= 150 && overtaking == false) manager.VestMessage("100000", port);
            }
        }
        if (!newAction && condition == 1 && !overtaking)
        {
            VestMotors();
        }
    }

    void Overtake()
    {
        controller.MaxSpeed = 108;
        if (!overtaking)
        {
            overtaking = myTracker.GetComponent<Tracker>().isOvertaking = true;
            if (condition == 1) ResetMotors(0);
            else manager.VestMessage("000010", port);
        }
    }

    void SlowDown()
    {
        controller.MaxSpeed *= 0.997f;
        if (controller.MaxSpeed <= 36)
        {
            controller.MaxSpeed = 36;
        }
    }

    void UpdateTimer()
    {
        if (timer == -1) //timer not set
        {
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    void VestMotors()
    {
            if (driverlessCar != null)
            {
                if (distanceA <= 250 && distanceA > 150)
                {
                    motors[2] = '1';
                }
                else if (distanceA <= 150 && distanceA > 0)
                {
                    motors[2] = '0';
                    motors[1] = '1';
                }
                else
                {
                    ResetMotors(0);
                }
            }
        if (oppCar != null)
        {
            if (distanceB <= 350 && distanceB > 200)
            {
                motors[3] = '1';
            }
            else if (distanceB <= 200 && distanceB > 100)
            {
                motors[3] = '0';
                motors[4] = '1';
            }
            else if (distanceB <= 100 && distanceB > -50 && oppZ > transform.position.z)
            {
                motors[4] = '0';
                motors[5] = '1';
            }
            else if (distanceB <= -50)
            {
                ResetMotors(2);
            }
        }
        manager.VestMessage(new string(motors), port);
    }

    public void ResetMotors(int motorsToReset)
    {
        int resetStart = 0;
        int resetEnd = 6;
        switch (motorsToReset)
        {
            case 0:                 //reset all
                resetStart = 0;
                resetEnd = 6;
                break;
            case 1:                 //reset carA
                resetStart = 0;
                resetEnd = 3;
                break;
            case 2:                 //reset carB
                resetStart = 3;
                resetEnd = 6;
                break;
            default:
                break;
        }
        for (int i = resetStart; i < resetEnd; i++)
        {
            motors[i] = '0';
        }
        manager.VestMessage(new string(motors), port);
    }
}
