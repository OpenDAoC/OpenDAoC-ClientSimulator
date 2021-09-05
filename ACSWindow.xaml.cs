using AtlasSimulator;
using AtlasSimulator.playerclasses;
using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AtlasClientSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PlayerCreator pc = new PlayerCreator();
        public List<PlayerClass> clients;

        public MainWindow()
        {
            InitializeComponent();

            // Temp Client list
            clients = new List<PlayerClass>();
            var rand = new Random();
            int spread = 50;
            for (int i = 0; i < 1; i++)
            {
                GLocation g = new GLocation(391433 + rand.Next(-spread, spread), 755797 + rand.Next(-spread, spread), 227, 1);
                Wizard w = new Wizard("FooWizzy" + i, "FooPassword", "FooWizzy" + i, g);
                pc.Create(w);
                clients.Add(w);
                //items[i].login();
            }
            //clients[0].login();
            clientList.ItemsSource = clients;
        }

        private void clientList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void LoadClientHandler(object sender, RoutedEventArgs e)
        {
            LoadClientWindow lcw = new LoadClientWindow();
            lcw.Show();
        }

        private void NewClientHandler(object sender, RoutedEventArgs e)
        {
            NewClientWindow ncw = new NewClientWindow();
            ncw.Show();
        }

        private void LoginHandler(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            var client  = (PlayerClass)checkbox.DataContext;
            client.login();
        }

        private void LogoutHandler(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(e.Source.ToString());
        }

        private void ActionOnHandler(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            var client = (PlayerClass)checkbox.DataContext;
            client.actionEnabled = true;
        }

        private void ActionOffHandler(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            var client = (PlayerClass)checkbox.DataContext;
            client.actionEnabled = false;
        }
    }




}
