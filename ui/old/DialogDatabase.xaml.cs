using System;
using System.Collections.Generic;
using System.IO;
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

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour DialogDatabase.xaml
    /// </summary>
    public partial class DialogDatabase : Window
    {
        private string database;
        public string Database { get => database; }

        public DialogDatabase()
        {
            database = "";
            InitializeComponent();
            string[] directories = Directory.GetDirectories("data");
            foreach(string directory in directories)
            {
                if(directory.StartsWith("data\\database_"))
                {
                    comboDatabase.Items.Add(directory.Remove(0, 14));
                }
            }
        }

        private void formValidation(object sender, EventArgs e)
        {
            if(comboDatabase.SelectedItem != null)
            {
                database = comboDatabase.SelectedItem.ToString();
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(FindResource("str_nodatabaseselected").ToString(), FindResource("str_error").ToString(), MessageBoxButton.OK, MessageBoxImage.Exclamation);

            }
        }

        private void formCancel(object sender, EventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
