![The Manager](images/themanager.png?raw=true "The manager")

The Manager is a soccer simulator aiming to simulate tournaments, matchs, and clubs/players evolution, inspired by games such as Football Manager, FIFA Manager.

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/cd29bda54a73415fac1a7449fdbc4b3a)](https://app.codacy.com/gh/nicolasLepy/TheManager/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

## Screenshots

### Menu

![Main menu](images/pic1.png?raw=true "Main menu")

### Simulation configuration

<img src="images/pic2.png" alt="Configuration screen" width="400"/> <img src="images/pic11.png" alt="Team selection" width="400"/> 

### Tournaments screen

![Menu screen](images/pic3.png?raw=true "Menu screen") ![Menu screen](images/pic10.png?raw=true "Menu screen")

### Screens

<p float="left">
	<img src="images/pic6.png" alt="Match screen" width="45%"/> 
	<img src="images/pic6b.png" alt="Match screen" width="45%"/>
</p>

<img src="images/pic5.png" alt="Player screen"/> 

<img src="images/pic4.png" alt="Club screen"/>

<img src="images/pic13.png" alt="Tournament screen"/>

<img src="images/pic14.png" alt="Tournament screen"/>

<p float="left">
	<img src="images/pic15.png" alt="International ranking" width="45%"/>
	<img src="images/pic16.png" alt="UEFA associations ranking" width="45%"/>
</p>

<img src="images/pic17.png" alt="UEFA clubs ranking" width="45%"/>

### Search players screen

<img src="images/pic12.png" alt="Search players screen"/>

### Pre-match screen

![Pre-match screen](images/pic8.png?raw=true "Pre-match screen")

### Live match screen

![Live match](images/pic7.png?raw=true "Live match")

## Simulation stability

The simulation has gained in stability : money in game stabilizes after few years and remains stable after. Players and clubs average level tends to incrase over time.

Some realism issues : Small clubs at beginning can't stabilize in professionnals divisions

<p float="left">
  <img src="images/graph_budget.png" width="40%" />
  <img src="images/graph_level.png" width="40%" />
</p>

<p float="left">
  <img src="images/graph_players.png" width="40%" />
  <img src="images/graph_debts.png" width="40%" />
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
*   Mapsui
*   Windows Media Player library

## Credits

*   [Live Charts for WPF](https://www.google.com)
*   MathNet.Numerics
*   [pngtree.com](https://pngtree.com)
*   Data about cities were obtained from [SimpleMaps](https://simplemaps.com/data/world-cities)
*   Data about players and clubs were obtained from [Kaggle](https://www.kaggle.com/stefanoleone992/fifa-22-complete-player-dataset?select=players_22.csv)
*   [flaticon.com (iconnut, Freepik, kerismaker and Futuer)](https://www.flaticon.com)


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

Map is licensed under the [Open Government Licence](https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/) ([World Administrative Boundaries - Countries and Territories](https://public.opendatasoft.com/explore/dataset/world-administrative-boundaries/information/?flg=fr-fr&dataChart=eyJxdWVyaWVzIjpbeyJjb25maWciOnsiZGF0YXNldCI6IndvcmxkLWFkbWluaXN0cmF0aXZlLWJvdW5kYXJpZXMiLCJvcHRpb25zIjp7ImZsZyI6ImZyLWZyIn19LCJjaGFydHMiOlt7ImFsaWduTW9udGgiOnRydWUsInR5cGUiOiJjb2x1bW4iLCJmdW5jIjoiQ09VTlQiLCJzY2llbnRpZmljRGlzcGxheSI6dHJ1ZSwiY29sb3IiOiIjRkY1MTVBIn1dLCJ4QXhpcyI6InN0YXR1cyIsIm1heHBvaW50cyI6NTAsInNvcnQiOiIifV0sInRpbWVzY2FsZSI6IiIsImRpc3BsYXlMZWdlbmQiOnRydWUsImFsaWduTW9udGgiOnRydWV9&location=7,14.42936,-56.20056&basemap=jawg.light))