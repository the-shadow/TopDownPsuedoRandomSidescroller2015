# TopDownPsuedoRandomSidescroller2015
Sample Code from a Project where I experimented with generating 2D tile sprite environments for a sidescrolling game.  

Seeing the results of implementing basic roguelike algorithms like "Flood Fill" and "Drunkard Walk" was fun.
My favorite portions of the project are in WorldManager.cs, WorldMap.cs, and CloudManager.cs
There are different rulesets depending on the biomes: grass, snow, desert, rugged

Lots of sliders and parameters were added to adjust the likeyhood and prominence of different tiles being placed like flowers, mountains, trees, lakes, rivers being placed and so on.
The project is also set up to be able to take in any 16x16 tile set.  The corresponding tile types like ground, tree, water, mountain and so on just need to be placed into the appropriate public parameters on the World Manager object in the unity editor.
