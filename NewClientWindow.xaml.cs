using AtlasSimulator;
using AtlasSimulator.playerclasses;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AtlasClientSimulator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 
    struct PresetLocation
    {
        public GLocation g;
        public string name;

        public override string ToString()
        {
            return name;
        }
    }

    enum ClassList
    {
        Paladin,
        Wizard
    }
    

    public partial class NewClientWindow : Window
    {
        PlayerCreator pc;

        public NewClientWindow()
        {
            InitializeComponent();

            // Setup for prefill locations
            List<PresetLocation> locations = new List<PresetLocation>();

            PresetLocation albDragon;
            albDragon.g = new GLocation(391433, 755797, 227, 1);
            albDragon.name = "Alb Dragon";
            locations.Add(albDragon);

            PresetLocation hibDragon;
            hibDragon.g = new GLocation(111111, 222222, 333, 4);
            hibDragon.name = "Hib Dragon";
            locations.Add(hibDragon);

            presetList.ItemsSource = locations;

            // Setup Class List
            classList.ItemsSource = Enum.GetValues(typeof(ClassList));
            pc = new PlayerCreator();
        }

        private void presetList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PresetLocation loc = (PresetLocation)presetList.SelectedItem;
            xloc.Text = loc.g.x.ToString();
            yloc.Text = loc.g.y.ToString();
            zloc.Text = loc.g.z.ToString();
            zone.Text = loc.g.zone.ToString();

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Check for multiples
            int numberToCreate = 1;
            int spread = 0;
            if (multipleCheckBox.IsChecked == true)
            {
                numberToCreate = Int32.Parse(Input_Count.Text);

                // Do some validation on that int
                if(numberToCreate <= 0 || numberToCreate > 500)
                {
                    Output_Error.Text = "Invalid number of clients, must be between 1 and 500";
                    return;
                }

                spread = Int32.Parse(Input_Spread.Text);
                if (spread < 0 || spread > 50)
                {
                    Output_Error.Text = "Invalid spread value, must be between 0 and 50";
                    return;
                }
            }

            // Create the clients
            MainWindow mw = (MainWindow)Application.Current.MainWindow;
            var rand = new Random();
            for (int i = 0; i < numberToCreate; ++i)
            {
                GLocation g = new GLocation(Int32.Parse(xloc.Text)+rand.Next(-spread, spread), 
                    Int32.Parse(yloc.Text) + rand.Next(-spread, spread), 
                    Int32.Parse(zloc.Text), Int32.Parse(zone.Text));

                ClassList c = (ClassList)classList.SelectedItem;
                PlayerClass newClass;
                string accountName = Input_AccountName.Text + i;
                string charName = Input_CharName.Text + i;
                switch (c)
                {
                    case ClassList.Wizard:
                        newClass = new Wizard(accountName, Input_Password.Text, accountName, g);
                        break;
                    case ClassList.Paladin:
                        newClass = new Paladin(accountName, Input_Password.Text, accountName, g);
                        break;
                    default:
                        newClass = new Wizard(accountName, Input_Password.Text, accountName, g);
                        break;
                }
                pc.Create(newClass);
                mw.clients.Add(newClass);
            }
            mw.clientList.Items.Refresh();
        }
    }
}
