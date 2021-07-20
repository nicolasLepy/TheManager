![The Manager](Pics/themanager.png?raw=true "The manager")

The Manager is a soccer simulator aiming to simulate tournaments, matchs, and clubs/players evolution, inspired by games such as Football Manager, FIFA Manager.

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/981285a9e5e542f79c39d71e14b04c59)](https://app.codacy.com/manual/lepynicolas/TheManager?utm_source=github.com&utm_medium=referral&utm_content=nicolasLepy/TheManager&utm_campaign=Badge_Grade_Dashboard)


## Features implemented

| Environment                                                               | 
| ------------------------------------------------------------------------- |
| Simulation on multiple years                                              |
| Managers                                                                  |
| Medias                                                                    |
| Free players transfers                                                    |

| Games                                                                     | 
| ------------------------------------------------------------------------- |
| Match simulation (goals, possession, substitutions)                       |
| Follow games in real-time (ranking, live results)                         |

| Players                                                                   | 
| ------------------------------------------------------------------------- |
| Players generation                                                        |
| Players evolution                                                         |

| Clubs                                                                     | 
| ------------------------------------------------------------------------- |
| Evolution of club training facilities                                     |
| Reserves teams                                                            |
| Can be forbidden to recruit if their finances are bad                     |

| Tournaments                                                               | 
| ------------------------------------------------------------------------- |
| Hierarchical Competitions (promotion, relegation ...)                     |
| Domestic Cup                                                              |
| International tournaments                                                 |
| Specials rules (lower team at home for domestic cups...)                  |
| Reserves teams can't go too high in league structure                      |

| Database                                                                  | 
| ------------------------------------------------------------------------- |
| Savegame                                                                  |
| XML Database                                                              |
| HTML exportation                                                          |

| GUI                                                                       | 
| ------------------------------------------------------------------------- |
| Sample GUI                                                                | 
| Team composition                                                          |
| Custom themes                                                             |


## Features to implement and issues to fix

| Task                                                                  | Status           |
| --------------------------------------------------------------------- | ---------------- |
| **Stability**                                                         |                  |
| Improve world stability                                               | 🔴 Not started |
| **Managing**                                                          |                 |
| Staff management                                                      | 🔴 Not started |
| Youth team management                                                 | 🔴 Not started |
| Search / observe players                                              | 🟡 In progress |
| Negociate contracts / transferts with other clubs                     | 🔴 Not started |
| **Environment**                                                       |                 |
| Dynamic dates, from one season to an other                            | 🔴 Not started |
| A system for transferring players between clubs                       | 🟡 In progress |
| National Directorate of Management Control (DNCG)                     | 🟡 In progress |
| **Tournaments**                                                       |                 |
| Set up international tournaments on multiples years                   | 🔴 Not started |
| **GUI**                                                               |                 |
| Panel to give infos (transfers, main results) during simulation       | 🟡 In progress |
| Make player search table faster                                       | 🟡 In progress |
| Improve GUI                                                           | 🟡 In progress |
| **Miscellaneous**                                                     |				  |
| Try to reduce savegame size (135mo / year)                            | 🟡 In progress |
| Manage memory for long games                                          | 🔴 Not started |
| (Data editor)                                                         | 🔴 Not started |
| (Generate Wiki with simulation data)                                  | 🔴 Not started |
| (3D Engine for games)                                                 | 🔴 Not started |

## Screenshots

### Menu

![Main menu](Pics/pic1.png?raw=true "Main menu")

### Simulation configuration

<img src="Pics/pic2.png" alt="Configuration screen" width="400"/> <img src="Pics/pic11.png" alt="Team selection" width="400"/> 

### Tournaments screen

![Menu screen](Pics/pic3.png?raw=true "Menu screen") ![Menu screen](Pics/pic10.png?raw=true "Menu screen")

### Record screens

<img src="Pics/pic5.png" alt="Player screen" width="400"/> <img src="Pics/pic6.png" alt="Match screen" width="400"/> 

<img src="Pics/pic4.png" alt="Club screen"/>

### Search players screen

<img src="Pics/pic12.png" alt="Search players screen"/>

### Pre-match screen

![Pre-match screen](Pics/pic8.png?raw=true "Pre-match screen")

### Live match screen

![Live match](Pics/pic7.png?raw=true "Live match")

## Simulation stability

The simulation is currently not very stable: money circulating in the game varies too much, and the more the years pass, the more the average level of the players decreases.

![Menu screen](Pics/graph_budget.png?raw=true "Total money in game by simulation year") ![Menu screen](Pics/graph_level.png?raw=true "Average level by year")

![Menu screen](Pics/graph_players.png?raw=true "Players in game by simulation year") ![Menu screen](Pics/graph_goals.png?raw=true "Average game goals by simulation year")


## Authors
Nicolas Lépy

## Tools used
Visual Studio
C#
WPF
Windows Media Player library

## Credits
[Live Charts for WPF](https://www.google.com)
MathNet.Numerics
[beauty png from pngtree.com](https://pngtree.com/so/beauty)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details