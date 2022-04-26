# Driving Simulation
## Prerequisites
- Unity Version 2019.2.17f1 (newer *should* work as well)
- Arduino IDE

To make sure everything is connected at the correct port, open Arduino IDE and go to Tools -> Port.

The vest BLE receiver is expected to be connected at port COM6. You can change this in the script "Control Manager" in the awake function.

The Arduino, which the GSR sensor is connected to, should be at port COM3. Change at the beginning of "Serial Receiver" if necessary.

## Scene structure

In all "Text" scenes, press SPACE to proceed to the next scene.

- Welcome Text

  In this scene, the gameobject "Communication" is created, which is preserved in the other scenes. The gameobject is necessary for receiving data from the arduino (via Serial Receiver script) and for communication with the Android app (via UDP Sender script). 
  
- Training

  Simulation with 6 car encounters. A log file is created at the beginning of the scene and data logged at each frame.

- Predriving Text

  The training log file is closed and a new one created for the main simulation.

- Driving

  Simulation with 22 car encounters.

- End Text

  Not used. After the critical encounter, the simulation and the Android app are quit.

## Scripts

- Activate Display

  Monitors have to be activated before use (except the main one). This is done by this script.
  
- Camera Projection

  For right and left monitor; not much here yet.
  
- Coloring

  Changes the color of the other cars (called in Spawn Cars when a new encounter starts).

- Control Manager

  Switches between automated and manual control; processes user input; sends data to the vest; it is also calculated here if an accident happens (and if yes, a crash sound is played instead of the normal sound loop).
  
- Data Logger

  Creates a log file and writes data to that file.
  
- Generate Terrain

  Creates terrain tiles in the direction you are driving and destroys the ones in the back. Nature props are distributed randomly across the grass area.
  
- Load Scene

  Automatically switches the scene when training/driving is over or when SPACE is pressed in the text presentation scenes.

- Overtaking

  Calculates speed and position when approaching or overtaking other cars; sends triggers to the vest (via Control Manager).

- Serial Receiver

  Establishes connection with Arduino to receive GSR data (background thread). The data is logged to seperate files in the training and driving.

- Spawn Cars

  Called when an encounter is over. A new encounter is chosen randomly (1, 3 or 4) if there are scenarios left, the old cars destroyed and new ones positioned according to the scenario (1: cars on both lanes, autonomous car waits before overtaking; 3: car on right lane, direct overtaking; 4: car on left lane). If 21 encounters were shown already, scenario 2 is presented (cars on both lanes, but autonomous car overtakes).

- Tracker

  Each car follows a tracker. For driving down the road, the tracker is always positioned further in front of the car when the car collides with the tracker; for overtaking, the tracker of the autonomous car follows a route of cubes around the car that is being overtaken.

- UDP Sender

  UDP connection with the app; sends the session ID to the app (like an alive flag) as long as the simulation is running. When the user brakes or crashes in the critical encounter 2, an end message is send to the app and the connection is closed.
  
## Issues
- Brake pedal value is not constant (anymore) when not pressed -> you don't read zeros then; when pressed, values are ok.
- Sometimes the autonomous car is too close to the lead car when overtaking (in encounters 1 and/or 3), touches the back left corner of it and starts flying around. 
- Adding another camera to the text scenes and rendering this camera to the "experimenter monitor" would be helpful

## Future Work
- for adding LSL: https://github.com/labstreaminglayer/LSL4Unity/wiki
- manual control: instead of the Pause Simulation coroutine in the Control Manager, use the commented input control in Update. Issue: How long are users allowed to drive manually? How to go back to automation (taking care of position & speed)
- configuring inputs: on the driving simulator pc, there is a programme "Foerst Hardware Test", where all in- and outputs are displayed with their corresponding values
