### Commands:
`up arrow` - Moves selected image 1 pixel up.

`down arrow` - Moves selected image 1 pixel down.

`left arrow` - Moves selected image 1 pixel left.

`right arrow` - Moves selected image 1 pixel right.

`enter` - Confirms the change with the image selected (does not save changes)


`left click`- On an image selects that image to move.

In progress commands, have to do with scaling which doesn't currently save to plist files.:

`-` - x scale down

`=` - x scale up

`[` - y scale down

`]` - y scale up

### How to use the application
1. First you need to create a new project. Go to `File > New Project`. In future you can open that existing project. Once a project is open you can open the file location from `File` as well.
2. Adding gamesheets to the application. I supplied some in the `Example Files` directory. Select `Add Gamesheet` from the menu. Supply a `.png` file and the corresponding `.plist` file. For example: `GJ_GameSheet03-uhd.png` and `GJ_GameSheet03-uhd.plist`. Doing this will store the contents of the split into a new directory when opening the project location.
3. Modify the textures. This is done with external tools such as Photoshop or paint. You can also modify the texture locations by using the menu option `Change Workspace` and selecting an option. Currently on `Main Menu` exists. When you enable a workspace it takes images from the project location. So if you're missing `GameSheet04` or `LaunchSheet` then some UI elements will appear missing. 
4. Move the elements around by clicking and using the commands above. When you're happy with the location of the UI element click the menu option `Save Workspace Changes`.
5. Finally when you finish everything you can select the menu option `Generate Texture Pack`. This will prompt you for an output location and it will create the files that are compatible with Geometry Dash.
6. Buy Geometry Dash from Steam for $4 and drag and drop the files into the the resources folder within the game files to see your changes in action.
