# ProceduralStructures
Create procedural structures (walls, houses, cities) in the Unity 3D engine.

The material and the texture files used are taking quite a lot of space and are stored at https://nothingtv.net/download/Proceduralstructures-CC0-Material.zip instead of this repository.

# How to start

## Create a scene with a terrain
You know how to do that, right? The house placement relies on a terrain to get the correct height over terrain.

## Define waypoints for your streets
You can use the RoadMarker prefab and drop a bunch of them on your terrain. Although you can use any GameObject the RoadMarker
has the benefit that the resulting connections can be visualized in the scene view.
To keep things organized it's best to keep the markers in a hierarchy that will later represent the street.

## Add the city prefab
The city prefab will become the root of all the generated houses. You can add it to an empty if you usually put all your static objects
in one hierarchy.

## Define your streets
Using the placed road markers you can know add streets to the city by dragging the markers in the list of transforms. You can define as many streets
and transforms as you like. Your patience and RAM is the limit.
The length will be calculated later. The door to street is the shortest distance from the front of one house to the midline of the street.
That means a value of e.g. 3.5 makes the street 7 units wide as the script will try to place houses on both sides.
The house to house distance is obviously the distance from wall to wall along the street. A value of 0.8 for example will put the houses next
to each other closely and your character may not be able to walk between all of them. The name of the street currently is only for your overview
although I think about giving each house a unique address that could be used in-game. Currently the name does not matter.

## Define your houses
For your first try you can simply use the prefabs that include the different house definitions. When you get a feel on how the definitions affect the look
you can copy house definition files to your assets or create new ones as well as the prefabs. The included files are merely examples (and not the
prettiest and most realistic looking houses).
Drag the prefabs House1-House11 (or whatever you like) in the list in the city definition. From this list houses will be picked randomly.

## Define the point in your hierarchy where the houses should go
You can define any point but for now I suggest to create just an empty under the city and drag that in the field parent. I usually give this empty
a name like "GeneratedHouses" to make clear that all below is subject to the regeneration process.

## Finally let the script place houses
Click on the "Transform to points" button. This will generate a list of locations and calculate the street length.
Finally you can click on "Place buildings" and several boxes should appear along your streets. These are the placeholders in more or less the
correct size and can give you a first impression of your future city.
You will notice some disabled game objects in your hierarchy. These are the ones that collided with others and the scipt won't place a house there.
If you're not OK with this automated descission you can enable, disable or delete to your liking.

## Grey boxes to houses at last
The boxes don't make a beutiful city yet but as I mentioned these are the placeholders for an intermediate step. You can build/rebuild individual houses
but for a start you should add the ProceduralStructuresRoot component to the parent of your city (or to the city or somewhere wlese above the
placeholders. That component lets you rebuild all houses from that point into all children.
Click on the "Rebuild all in hierarchy starting here" button and it will do what the button label says.
Even on a low-end coputer this step usually just takes seconds and your city will finally appear.
Please note that if your camera is way up in a birds view the Unity editor may clip parts of the work because it's too far away. Just saying, I was
shocked for a moment when I experienced that for the first time.

