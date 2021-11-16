using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TheManager.Parsers
{
    /// <summary>
    /// Parse FIFA data into XML database format
    /// </summary>
    public class FifaDataParser
    {

        private string RemoveQuotedText(string input)
        {
            string output = input;
            int firstQuoteIndex = input.IndexOf("\"");

            if (firstQuoteIndex >= 0)
            {
                int secondQuoteIndex = input.IndexOf("\"", firstQuoteIndex + 1);

                if (secondQuoteIndex >= 0)
                {
                    output = input.Substring(0, firstQuoteIndex + 1) + input.Substring(secondQuoteIndex);
                }
            }
            return output;
        }

        public void Parse()
        {

            Dictionary<string, int> leaguesCount = new Dictionary<string, int>();
            Dictionary<int, string> leagueClubs = new Dictionary<int, string>();
            Dictionary<string, int> clubsId = new Dictionary<string, int>();
            Dictionary<int, int> clubsLevel = new Dictionary<int, int>();
            List<int> playersId = new List<int>();


            Session.Instance.Game = new Game();

            DatabaseLoader dl = new DatabaseLoader(Session.Instance.Game.kernel);
            dl.LoadLanguages();
            dl.LoadGeography();
            dl.LoadCities();
            dl.LoadClubs();

            XDocument d = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            XElement root = new XElement("Clubs");
            d.Add(root);

            List<string> lines = File.ReadLines(Utils.dataFolderName + "/fifa22/teams_fifa22.csv").ToList();
            lines.RemoveAt(0);

            foreach (string line in lines)
            {
                string[] lineSplit = line.Split(',');
                string clubName = lineSplit[1];
                string shortClubName = lineSplit[1];
                int clubReputation = int.Parse(lineSplit[9])*10;
                int clubBudget = int.Parse(lineSplit[8])*5;
                Club clubKernel = Session.Instance.Game.kernel.String2Club(clubName);
                
                string clubCity = clubKernel != null ? (clubKernel as CityClub).city.Name : "";
                int clubSupporters = clubKernel != null ? (clubKernel as CityClub).supporters : 30000;
                string clubStadiumName = clubKernel != null ? (clubKernel as CityClub).stadium.name : "";
                string clubLogo = clubKernel != null ? (clubKernel as CityClub).logo  : clubName.Trim().Replace(" ", "").Replace("&", "");

                int clubFormationCenter = int.Parse(lineSplit[4]);
                int clubId = int.Parse(lineSplit[0]);
                clubsId.Add(clubName, clubId);
                string clubLeague = lineSplit[2];
                clubLeague = clubLeague.Remove(clubLeague.Length - 4);
                leagueClubs[clubId] = clubLeague;
                if(!leaguesCount.ContainsKey(clubLeague))
                {
                    leaguesCount[clubLeague] = 0;
                }
                leaguesCount[clubLeague]++;
                clubsLevel[clubId] = clubFormationCenter;

                XElement e = new XElement("Club");
                e.Add(new XAttribute("nom", clubName));
                e.Add(new XAttribute("nomCourt", shortClubName));
                e.Add(new XAttribute("reputation", clubReputation));
                e.Add(new XAttribute("budget", clubBudget));
                e.Add(new XAttribute("supporters", clubSupporters));
                e.Add(new XAttribute("stade", clubStadiumName));
                e.Add(new XAttribute("ville", clubCity));
                e.Add(new XAttribute("centreFormation", clubFormationCenter));
                e.Add(new XAttribute("logo", clubLogo));
                e.Add(new XAttribute("id", clubId));
                root.Add(e);
            }

            d.Save("data/fifa22/clubs.xml");

            d = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            root = new XElement("Competitions");
            d.Add(root);

            foreach (KeyValuePair<string, int> kvp in leaguesCount)
            {
                double averageClubLevel = 0;
                XElement e = new XElement("Competition");
                e.Add(new XAttribute("localisation", ""));
                e.Add(new XAttribute("nom", kvp.Key));
                e.Add(new XAttribute("nomCourt", ""));
                e.Add(new XAttribute("logo", ""));
                e.Add(new XAttribute("debut_saison", 25));
                e.Add(new XAttribute("championnat", "oui"));
                e.Add(new XAttribute("niveau", 1));
                e.Add(new XAttribute("color", "255,0,0"));
                root.Add(e);

                XElement e2 = new XElement("Tour");
                if(kvp.Value >= 8)
                {
                    e2.Add(new XAttribute("type", "championnat"));
                    e2.Add(new XAttribute("nom", "Championnat"));
                    e2.Add(new XAttribute("allerRetour", "oui"));
                    e2.Add(new XAttribute("initialisation", 26));
                    e2.Add(new XAttribute("fin", 21));
                    e2.Add(new XAttribute("heureParDefaut", "15:30"));
                    e2.Add(new XAttribute("dernieresJourneesMemeJour", 2));
                    e2.Add(new XAttribute("calendrier", ((kvp.Value - 1) * 2) + "_D1"));
                }
                else
                {
                    e2.Add(new XAttribute("type", "inactif"));
                    e2.Add(new XAttribute("nom", "Championnat"));
                    e2.Add(new XAttribute("allerRetour", "oui"));
                    e2.Add(new XAttribute("initialisation", 26));
                    e2.Add(new XAttribute("fin", 21));
                    e2.Add(new XAttribute("heureParDefaut", "15:30"));
                }

                e.Add(e2);

                foreach(KeyValuePair<int, string> kvpClubs in leagueClubs)
                {
                    if(kvpClubs.Value == kvp.Key)
                    {
                        XElement e3 = new XElement("Club");
                        e3.Add(new XAttribute("id", kvpClubs.Key));
                        e2.Add(e3);
                        averageClubLevel += clubsLevel[kvpClubs.Key];
                    }
                }
                averageClubLevel /= (kvp.Value + 0.0);

                XElement eQualif = new XElement("Qualification");
                eQualif.Add(new XAttribute("de", 1));
                eQualif.Add(new XAttribute("a", kvp.Value));
                eQualif.Add(new XAttribute("id_tour", 0));
                eQualif.Add(new XAttribute("anneeSuivante", "oui"));
                e2.Add(eQualif);

                double dotationValue = 0.00001 * Math.Exp(0.21 * averageClubLevel)*1000000;
                for (int i = 0; i < kvp.Value; i++)
                {
                    XElement eDotation = new XElement("Dotation");
                    eDotation.Add(new XAttribute("classement", i+1));
                    eDotation.Add(new XAttribute("somme", dotationValue.ToString("0") ));
                    e2.Add(eDotation);
                    dotationValue = 0.94 * dotationValue;

                }

            }

            d.Save("data/fifa22/comp.xml");

            d = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            root = new XElement("Joueurs");
            d.Add(root);


            lines = File.ReadLines(Utils.dataFolderName + "/fifa22/players_fifa22.csv").ToList();
            lines.RemoveAt(0);

            foreach (string line in lines)
            {
                string treatedLine = RemoveQuotedText(line);
                string[] lineSplit = treatedLine.Split(',');
                string playerPrenom = "";
                string playerNom = lineSplit[2];
                int playerLevel = int.Parse(lineSplit[8]);
                int playerPotential = int.Parse(lineSplit[9]);
                int playerId = int.Parse(lineSplit[0]);
                int playerClub = lineSplit[15] == "Free agent" ? 0 : clubsId[lineSplit[15]];
                string playerBirthday = "1-1-" + (2022-int.Parse(lineSplit[3]));
                string playerCountry = lineSplit[7];

                string playerPosition;
                switch (lineSplit[14])
                {
                    case "GK":
                        playerPosition = "GARDIEN";
                        break;
                    case "CB":case "LB":case "RB":case "LCB":case "RCB":case "RDM":
                        playerPosition = "DEFENSEUR";
                        break;
                    case "CDM":case "CM":case "LM":case "LW":case "LWB":case "RM":case "RCM":case "LDM":case "RW":case "RWB":
                        playerPosition = "MILIEU";
                        break;
                    case "CAM":case "CF":case "ST":case "LAM":case "RF":case "LCM":case "RAM":case "LF":case "LS":case "RS":
                        playerPosition = "ATTAQUANT";
                        break;
                    default:
                        playerPosition = "DEFENSEUR";
                        break;
                }

                if(!playersId.Contains(playerId))
                {
                    playersId.Add(playerId);
                    XElement e = new XElement("Joueur");
                    e.Add(new XAttribute("prenom", playerPrenom));
                    e.Add(new XAttribute("nom", playerNom));
                    e.Add(new XAttribute("niveau", playerLevel));
                    e.Add(new XAttribute("potentiel", playerPotential));
                    e.Add(new XAttribute("poste", playerPosition));
                    e.Add(new XAttribute("club", playerClub));
                    e.Add(new XAttribute("naissance", playerBirthday));
                    e.Add(new XAttribute("pays", playerCountry));
                    root.Add(e);
                }
            }

            d.Save("data/fifa22/players.xml");
        }
    }
}
