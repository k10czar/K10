[logo]: https://github.com/k10czar/K10/raw/master/icon.png "Logo"
![alt text][logo]

Library that help to work in Unity

Scratched with [**Dungeonland**][DL]

Started on [**Star Vikings**][SV]

Perfected in [**Relic Hunters Legend**][RHL]

[DL]: https://www.youtube.com/watch?v=yxM9N1xOBqQ
[SV]: https://www.starvikings.com
[RHL]: https://www.relichunters.com.br

Still evolving...

## Add as submodule on your Unity project repository

``git submodule add https://github.com/k10czar/K10.git "Assets/Plugins/K10/Core"``

## Samples
//TO DO

## Check out for more K10 Modules

[**GUI Skinner**](https://github.com/k10czar/GuiSkinner.git)

[**DOTS Utils**](https://github.com/k10czar/K10-DOTS.git)

[**Audio Tools**](https://github.com/k10czar/K10-Audio.git)

[**Dialog Window System**](https://github.com/k10czar/K10-Dialog-Window-System.git) *

[**Localization Tool**](https://bitbucket.org/roguesnail/k10-localization-tool.git) *

###### *To get access our private modules please contact us

## How To remove the submodule

1.  Delete from the  _.gitmodules_  file:

	`[submodule "Assets/Plugins/K10/Core"]`
	
	`path = Assets/Plugins/K10/Core`
	
	`url = https://github.com/k10czar/K10.git`
	
2.  Delete from  _.git/config_:

	`[submodule "Assets/Plugins/K10/Core"]`
	
	`url = https://github.com/k10czar/K10.git`
	
	`active = true`
	
3.  Run:

	`git rm --cached "Assets/Plugins/K10/Core"`

4.  Commit the superproject.

5.  Delete the submodule folder _`Assets/Plugins/K10`_.
