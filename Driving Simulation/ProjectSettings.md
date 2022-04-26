In Edit -> Project Settings, make sure the following adjustments are made:


#### Input:
Add or overwrite two axes:

- Name: Accelerate

  Gravity: 0

  Dead: 0

  Sensitivity: 1

  Invert: yes

  Type: Joystick Axis

  Axis: Y Axis

  Joy Num: Get motion from all joysticks


- Name: Brake

  Gravity: 0

  Dead: 0

  Sensitivity: 1

  Invert: yes

  Type: Joystick Axis

  Axis: 3rd Axis

  Joy Num: Get motion from all joysticks


#### Player:
- Resolution and Presentation -> Standalone Player Options:

  Display Resolution Dialog: Disabled

- Other Settings -> Configuration:

  Scripting Backend: Mono

  Api Compability Level: .NET 4.x

#### Tags and Layers -> Tags
- lb_bird
- Fire
- lb_groundTarget
- Terrain
- DriverlessCarOpposite
- Waypoint
- DriverlessCar
- lb_perchTarget
- Label
- MyCar
- UITexts
- Communication
