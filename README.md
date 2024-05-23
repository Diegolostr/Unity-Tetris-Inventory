# Unity-Tetris-Inventory
Tetris Inventory for Unity

How to Setup :

 - First, create an empty gameobject (or a panel) and then put
 the ObjectWidthManager and the GridManager scripts in it <GridManager>.

 - Create a canvas and put it inside our emtpy gameobject <GridManager>.

 - Later, inside our canvas, create another empty gameobject <Inventory> and put a GridLayoutGrup in it.

 - Then create inside our canvas and Image gameobject <Cursor> and another emtpy gameobject <Items>.

 -- Create a tile prefab too for the GridManager script

 ----------------------------------------------------------------------------------------------------------

 We have some options to twick in our scripts, but the important ones for the inventory to work are:

GridManager:
  - Tile Prefab = Just insert your tile prefab in it
  - Spawn parent = Its the inventory transform to spawn the tiles in it

ObjectWidthManager:

  - Cursor transform = Put your cursor object in it
  - Image prefab = Just and image to instantiate the objects
   
