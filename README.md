# Platform Fighter

*Platform Fighter* is a prototype of a two-player platform fighter game, that is built with *Unity* in *C#* and developed as part of a university project by **Gregor Germerodt** and **Zishan Ahmed** under the supervision of **Prof. Dr. Christoph Rezk-Salama** and **Prof. Dr. Linda Breitlauch** at *Trier University of Applied Sciences*.

This project was created to explore and understand the core mechanics behind platform fighter games like *Super Smash Bros.*.
The result is a functional prototype focused on gameplay systems, technical experimentation, and custom tool development rather than visual polish.

<img width="1680" height="1050" alt="Screenshot of Platform Fighter" src="https://github.com/user-attachments/assets/f25ed357-4ac8-4de5-9841-9b30a04ec96e" />

---

## Gameplay Overview

*Platform Fighter* is a two-player local fighting game prototype.  
Both players control identical fighters (mirror match setup) and attempt to knock each other off the stage.  
Each hit increases the opponent’s damage percentage, making them easier to launch.  
A player scores a point by knocking their opponent off-screen — the defeated player respawns immediately.

## Key Features

#### **Custom Animation Command System (ACMD Framework)**  
  Inspired by the *Super Smash Bros.* engine, this framework allows moves to be defined frame-by-frame using animation scopes and custom state management.  
  It supports own `ACMDs` (Animation Commands) for move-specific logic and `GACMDs` (General Animation Commands) for persistent state updates — an original extension concept developed for this project.

#### **Custom Input System**  
  Handles dynamic device assignment between keyboard and controllers.  
  The first detected device becomes **Player 1**, the next becomes **Player 2**, with automatic reassignment on disconnects.

#### **Fighter Controller**  
  A fully custom 2D movement and collision system within a 3D *Unity* environment, including:
  - Ground and wall detection using raycasts  
  - Platform drop-through mechanics  
  - Physics-based acceleration and deceleration control

#### **Unity Development Tool - Parameter Table**  
  A custom Unity Editor window that loads fighter parameters from CSV files, allowing live tuning and saving without code recompiling.

#### **Motion Capture Animation Pipeline**  
  Over 8 hours of mocap sessions were recorded using *OptiTrack Motive* professionally at *University’s Design Campus*.  
  The data was cleaned and retargeted in *Blender* using *Mixamo* rigs before being imported into *Unity*.  
  Although only 9 of 28 animations were used, mocap made it much more easier to create animations for this prototype.

#### **Original Stage Design**  
  Modeled entirely in *Blender*, inspired by a donut covered in icing — a fun nod to the playful tone of the project.

## Controls

### **Keyboard (QWERTZ/QWERTY layout) \[always active\]**
#### Player 1
- `WASD` - Movement  
- `E` - Attack  
- `C` - Jump  

#### Player 2
- `IJKL` - Movement  
- `O` - Attack  
- `.` - Jump  

### **Controllers**
#### Both players
- `L-Stick` - Movement
- `A` - Attack
- `X` or `Y` - Jump

The first controller to register input is assigned to **Player 1**, and the next one automatically takes **Player 2**.  
When disconnected, controllers can be reconnected and reassigned automatically to the correct players.  
*Tested with Xbox One and Switch Pro controllers*

### **Global**
- `F1` - Reset game and controller assignments  

## Development Insights

This project served as a **technical exploration** rather than a full content production.  
We focused on understanding fighter logic, animation systems, and Unity tooling — not on polish or scale.
It’s a **learning prototype** that shows what’s possible with curiosity, persistence, and a lot of debugging.

#### We experimented with:
- Modular animation logic via custom command scripts  
- Data-driven character tuning using ScriptableObjects  
- Custom tools for balance testing  
- Integrating mocap data into Unity workflows

## Built using
- **Unity (C#)** - Game engine  
- **Blender** - Stage modeling and animation cleanup  
- **OptiTrack Motive** - Motion Capture sessions  
- **Mixamo** - Base rig and animation retargeting

---

## Authors
**Gregor Germerodt** - Stage modeling, Unity parameter tools, motion capture integration  
**Zishan Ahmed** - ACMD framework, fighter controller, input system, motion capture integration  

## Acknowledgments
Special thanks to our supervisors **Prof. Dr. Christoph Rezk-Salama** and **Prof. Dr. Linda Breitlauch** at *Trier University of Applied Sciences* for their consultation and assistance.  
Developed as part of the *Media Project: Interdisciplinary Game Development* course

## License

All code and assets are published with the agreement of **Gregor Germerodt** and **Zishan Ahmed**.  
**Code:** Licensed under MIT — see [LICENSE](./LICENSE).  
**Assets:** Rights to certain assets (e.g. Mixamo, OptiTrack) are reserved and used under their respective terms.
