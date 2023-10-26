![The Manager](Pics/themanager.png?raw=true "The manager")

The Manager is a soccer simulator aiming to simulate tournaments, matchs, and clubs/players evolution, inspired by games such as Football Manager, FIFA Manager.

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/cd29bda54a73415fac1a7449fdbc4b3a)](https://app.codacy.com/gh/nicolasLepy/TheManager/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

## Screenshots

### Menu

![Main menu](Pics/pic1.png?raw=true "Main menu")

### Simulation configuration

<img src="Pics/pic2.png" alt="Configuration screen" width="400"/> <img src="Pics/pic11.png" alt="Team selection" width="400"/> 

### Tournaments screen

![Menu screen](Pics/pic3.png?raw=true "Menu screen") ![Menu screen](Pics/pic10.png?raw=true "Menu screen")

### Screens

<p float="left">
	<img src="Pics/pic6.png" alt="Match screen" width="45%"/> 
	<img src="Pics/pic6b.png" alt="Match screen" width="45%"/>
</p>

<img src="Pics/pic5.png" alt="Player screen"/> 

<img src="Pics/pic4.png" alt="Club screen"/>

<img src="Pics/pic13.png" alt="Tournament screen"/>

<img src="Pics/pic14.png" alt="Tournament screen"/>

<p float="left">
	<img src="Pics/pic15.png" alt="International ranking" width="45%"/>
	<img src="Pics/pic16.png" alt="UEFA associations ranking" width="45%"/>
</p>

<img src="Pics/pic17.png" alt="UEFA clubs ranking" width="45%"/>

### Search players screen

<img src="Pics/pic12.png" alt="Search players screen"/>

### Pre-match screen

![Pre-match screen](Pics/pic8.png?raw=true "Pre-match screen")

### Live match screen

![Live match](Pics/pic7.png?raw=true "Live match")

## Simulation stability

The simulation has gained in stability : money in game stabilizes after few years and remains stable after. Players and clubs average level tends to incrase over time.

Some realism issues : Small clubs at beginning can't stabilize in professionnals divisions

<p float="left">
  <img src="Pics/graph_budget.png" width="40%" />
  <img src="Pics/graph_level.png" width="40%" />
</p>

<p float="left">
  <img src="Pics/graph_players.png" width="40%" />
  <img src="Pics/graph_debts.png" width="40%" />
</p>

## Features

| Task                                                                  | Status           |
| --------------------------------------------------------------------- | ---------------- |
| **World**                                                             |                  |
| Tournaments rules                                                     | 🟢 OK           |
| Improve game stability                                                | 🟡 In progress  |
| Managers and staff                                                    | 🟡 In progress  |
| Transferts                                                            | 🟡 In progress  |
| **Game**                                                              |                 |
| Detailed game simulation                                              | 🔴 Not started  |
| Games in real-time                                                    | 🟡 In progress  |
| Players evolution                                                     | 🟡 In progress  |
| **Tournaments**                                                       |                 |
| Hierarchical Competitions (promotion, relegation ...)                 | 🟢 OK  |
| Automatically created domestic cup (qualifications and scheduling)    | 🟢 OK  |
| International tournaments                                 			| 🟢 OK  |
| International ranking                                 				| 🟢 OK  |
| Specials rules (lower team at home for domestic cups...)              | 🟢 OK  |
| **Club**                                                              |                 |
| Evolution of club facilities                                          | 🟡 In progress  |
| Financial control                                                     | 🔴 Not started  |
| **Managing**                                                          |                 |
| Staff management                                                      | 🔴 Not started |
| Youth team management                                                 | 🔴 Not started |
| Recruitment tasks                                                     | 🟡 In progress |
| **Miscellaneous**                                                     |				  |
| Simulation serialization				                                | 🟢 OK |
| Manage memory for long games                                          | 🔴 Not started |
| Improve serialization to manage large objects graph                   | 🔴 Not started |

## Authors
Nicolas Lépy

## Tools used
*   .NET Framework 4.8 and WPF
*   Visual Studio
*   Rider
*   Windows Media Player library
*   MapWindow GIS

## Credits

*   [Live Charts for WPF](https://www.google.com)
*   [MapWindow GIS](https://www.mapwindow.org/)
*   MathNet.Numerics
*   [pngtree.com](https://pngtree.com)
*   Data about cities were obtained from [SimpleMaps](https://simplemaps.com/data/world-cities)
*   Data about players and clubs were obtained from [Kaggle](https://www.kaggle.com/stefanoleone992/fifa-22-complete-player-dataset?select=players_22.csv)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
