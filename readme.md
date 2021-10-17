# Procedural Dungeon

## Senior Project May 2020

### How it works

- Dungeon: A collection of rooms
- Room: Connects to other rooms based on its sides (Left, Right, Up, Down, Forward, Backward)
- Side: A side is where a connection needs to be made meaning door to door or wall to wall connection

#### Setup

- On start all possible room combinations are generated using a template room with doors on all sides.

- After all combination are created:
  - They are saved in a dictionary with a side as key and list of rooms having a door on that side as value.
  - A room that has a left connection and a right connection will be in the list that has left as a key and the list that has right as a key.

- All generated rooms are saved in a dictionary with their coordinates as key

- A list of available nodes determines the coordinates where rooms still need to be generated

- Bread first search expansion:

  - This creates a dungeon with many path almost all rooms are connected to each other.
  - Once the generated room count and the available node count exceed the limit start generating rooms that only connect to rooms that already exist to prevent having an infinite dungeon.
  - The coordinates will be saved in a queue allowing to place a room then all its neighboring rooms and so on.

- Depth first search expansion:

  - This creates a dungeon with a single path.
  - Once the generated room count exceed the limit backtrack to the neighbors of the generated rooms and generate rooms that only connect to the existing rooms.
  - The coordinates will be saved in a stack allowing to place a room followed by one of its child and so on then backtracking and filling up the remaining coordinates.

### Room Placement

- Once a room is placed it checks the restrictions based on the already placed neigbors. If the right neighbor has a left door then the current room needs to have a right door, else it needs to have right wall.

- To position the generated room a reference neighbor is chosen then the sides are positioned on top of each other in order to position the room correctly. The left side must have the same position as the right side of the neighbor in order for the new room to be correctly positioned.

- After the room is positioned all sides that have a door adds a coordinate to the list of available coordinates in order to generate a room that will connect to the door.

### Generate room count

- To prevent stopping the generation before the count is reached every room that is generated needs to lead to another room that is not yet created to make sure that a new coordinate is added to the list.

- To prevent exceeding the room count every room that is generated can only lead to rooms that where already generated and all remaining sides needs to have wall.
