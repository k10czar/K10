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

``git submodule add https://github.com/k10czar/K10.git "Assets/Standard Assets/K10"``

## Samples
//TO DO

## Check out for more K10 Modules

[**GUI Skinner**](https://github.com/k10czar/GuiSkinner.git)

[**DOTS Utils**](https://github.com/k10czar/K10-DOTS.git)

[**Dialog Window System**](https://github.com/k10czar/K10-Dialog-Window-System.git) *

[**Localisation Tool**](https://bitbucket.org/roguesnail/k10localisationtool.git) *

###### *To get access our private modules please contact us

## How To remove the submodule

1.  Delete from the  _.gitmodules_  file:

	`[submodule "Assets/Standard Assets/K10"]`
	
	`path = Assets/Standard Assets/K10`
	
	`url = https://github.com/k10czar/K10.git`
	
2.  Delete from  _.git/config_:

	`[submodule "Assets/Standard Assets/K10"]`
	
	`url = https://github.com/k10czar/K10.git`
	
	`active = true`
	
3.  Run:

	`git rm --cached "Assets/Standard Assets/K10"`

4.  Commit the superproject.

5.  Delete the submodule folder _`Assets/Standard Assets/K10`_.
