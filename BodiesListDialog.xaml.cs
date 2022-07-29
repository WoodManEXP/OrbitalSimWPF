using System;
using System.Collections.Generic;
using System.Windows;

namespace OrbitalSimWPF
{
    /// <summary>
    /// Interaction logic for BodiesListDialog.xaml
    /// </summary>
    public partial class BodiesListDialog : Window
    {

        private BodyList BodyList;

        public BodiesListDialog(BodyList bodyList)
        {
            InitializeComponent();

            this.BodyList = bodyList;

            PopulateList();
        }

        private class ListEntry
        {
            public Boolean Selected { get; set; }
            public String? Text { get; set; }
        }
        private void PopulateList()
        {
            List<Body> bodies = BodyList.Bodies;
            List<ListEntry> listEntries = new List<ListEntry>();

            foreach (Body b in bodies)
            {
                listEntries.Add(new ListEntry() { Selected = b.Selected, Text = b.Name });
            }

            bodiesList.ItemsSource = listEntries;
        }

        private void Button_OK(object sender, RoutedEventArgs e)
        {
            // Save state of BodiesList

            // Transfer current selected state into BodiesList
            BodyList.SelectAll(false);

            List<ListEntry> listEntries = (List<ListEntry>)bodiesList.ItemsSource;
            int i = -1;
            foreach (ListEntry entry in listEntries)
            {
                BodyList.SetSelected(++i, entry.Selected);
            }

            BodyList.SaveBodyList();
            DialogResult = true;
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // Reload from init
        private void Button_Reload(object sender, RoutedEventArgs e)
        {
            BodyList.LoadList(true);
            PopulateList();
        }

        private void Button_Clear(object sender, RoutedEventArgs e)
        {
            BodyList.SelectAll(false);
            PopulateList();
        }

        private void Button_SelAll(object sender, RoutedEventArgs e)
        {
            BodyList.SelectAll(true);
            PopulateList();
        }

    }
}
