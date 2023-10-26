using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TheManager_GUI.Styles
{
    public class Langue
    {
        private static List<Langue> _langues = new List<Langue>();
        public static List<Langue> langues => _langues;
        public static Langue current { get; set; }


        private string reference;
        private string standardLanguageCode;
        private string resourceFileName;
        private string name;

        public Langue(string name, string reference, string standardLanguageCode, string resourceFileName)
        {
            this.name = name;
            this.reference = reference;
            this.standardLanguageCode = standardLanguageCode;
            this.resourceFileName = resourceFileName;
        }

        public void SetAsCurrentLangue()
        {
            //Copy all MergedDictionarys into a auxiliar list.
            var dictionaryList = Application.Current.Resources.MergedDictionaries.ToList();

            Application.Current.Properties["language"] = this.reference;
            CultureInfo.CurrentCulture = new CultureInfo(this.standardLanguageCode);
            //Search for the specified culture.
            string requestedCulture = this.resourceFileName;
            var resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString == requestedCulture);

            //If we have the requested resource, remove it from the list and place at the end.     
            //Then this language will be our string table to use.      
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
            current = this;
        }

        public override string ToString()
        {
            return this.name;
        }

    }
}
