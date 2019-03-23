using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public enum Poste
    {
        GARDIEN,
        DEFENSEUR,
        MILIEU,
        ATTAQUANT
    }

    public class Joueur
    {
        private string _nom;
        private string _prenom;
        private DateTime _naissance;
        private int _niveau;
        private int _potentiel;
        private Pays _nationalite;
        private Poste _poste;
        private bool _suspendu;
        
        //Propriétés
        //nb de jaunes cette saison
        //nb de rouges cette saison
        //nb de buts cette saison
        //appelé en sélection
    }

    public class Contrat
    {
        private float _salaire;
        private bool _transferable;
        private DateTime _fin;
    }
}