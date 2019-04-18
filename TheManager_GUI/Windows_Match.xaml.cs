﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheManager;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour Windows_Match.xaml
    /// </summary>
    public partial class Windows_Match : Window
    {


        public Windows_Match(Match match)
        {
            InitializeComponent();
            lbStade.Content = match.Domicile.Stade.Nom;
            lbAffluence.Content = match.Affluence + " spectateurs";
            lbEquipe1.Content = match.Domicile.Nom;
            lbEquipe2.Content = match.Exterieur.Nom;
            lbScore.Content = match.Score1 + " - " + match.Score2;
            if (match.Prolongations)
                lbScore.Content += " a.p.";
            if(match.TAB)
            {
                lbScore.Content += " (" + match.Tab1 + "-" + match.Tab2 + " tab)";
            }
            lbMT.Content = "(" + match.ScoreMT1 + " - " + match.ScoreMT2 + ")";

            foreach(EvenementMatch em in match.Evenements)
            {
                if(em.Type == Evenement.BUT || em.Type == Evenement.BUT_CSC || em.Type == Evenement.BUT_PENALTY)
                {
                    string c1 = "";
                    string c2 = "";
                    string c3 = "";
                    string c4 = "";
                    if (em.Club == match.Domicile)
                    {
                        c1 = em.MinuteStr;
                        c2 = em.Joueur.Prenom + " " + em.Joueur.Nom;
                        if (em.Type == Evenement.BUT_PENALTY) c2 += " (sp)";
                        if (em.Type == Evenement.BUT_CSC) c2 += " (csc)";
                    }
                    else
                    {
                        c4 = em.MinuteStr;
                        c3 = em.Joueur.Prenom + " " + em.Joueur.Nom;
                        if (em.Type == Evenement.BUT_PENALTY) c3 += " (sp)";
                        if (em.Type == Evenement.BUT_CSC) c3 += " (csc)";
                    }
                    dgEvenements.Items.Add(new EvenementElement { Col1 = c1, Col2 = c2, Col3 = c3, Col4 = c4 });
                }
                
            }
            foreach(Journaliste j in match.Journalistes)
            {
                dgEvenements.Items.Add(new JournalisteElement { Journaliste = j.Prenom + " " + j.Nom, Media = j.Media.Nom });
            }
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public struct JournalisteElement
    {
        public string Journaliste { get; set; }
        public string Media{ get; set; }
    }

    public struct EvenementElement
    {
        public string Col1 { get; set; }
        public string Col2 { get; set; }
        public string Col3 { get; set; }
        public string Col4 { get; set; }
    }
}
