using DymoSDK.Interfaces;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TCGPlayerAddressLabel.Properties;

namespace TCGPlayerAddressLabel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AddressPrinter myPrinter;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            DymoSDK.App.Init();
            myPrinter = new AddressPrinter();

            // Printer Selection Binding
            var printerListBinding = new Binding() { Source = myPrinter.Printers };
            printerList.SetBinding(ItemsControl.ItemsSourceProperty, printerListBinding);
            printerList.DisplayMemberPath = "Name";
            printerList.SelectedValuePath = "Name";
            if (myPrinter.Printers.Count() > 0)
            {
                printerList.SelectedIndex = 0;
            }
            var printerTypeBinding = new Binding("PrinterType") { Source = (IPrinter)printerList.SelectedItem };
            PrinterType.SetBinding(TextBox.TextProperty, printerTypeBinding);

            // Orders Binding
            var orderListBinding = new Binding() { Source = myPrinter.Orders.orders };
            OrdersListBox.SetBinding(ItemsControl.ItemsSourceProperty, orderListBinding);

            Settings.Default.PropertyChanged += Default_PropertyChanged;
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void LoadAddressesButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "csv files (*.csv)|*.csv";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() ?? false)
            {
                filePath = openFileDialog.FileName;
                myPrinter.LoadAddresses(filePath);
                OrdersListBox.Items.Refresh();
            }
        }

        private void OrdersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var order = (TCGPlayerOrder)OrdersListBox.SelectedItem; if (order == null) return;
            myPrinter.SetLabelAddress(order.Address);
            image.Source = myPrinter.render();
        }

        private void PrintAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(TCGPlayerOrder order in myPrinter.Orders)
            {
                myPrinter.SetLabelAddress(order.Address);
                myPrinter.PrintLabel((IPrinter)printerList.SelectedItem);
                if (PrintReturnAddressLabels.IsChecked ?? false)
                {
                    myPrinter.SetLabelAddress(Settings.Default.ReturnAddress);
                    myPrinter.PrintLabel((IPrinter) printerList.SelectedItem);
                }
            }
            MessageBox.Show($"Printed {myPrinter.Orders.orders.Count} labels.", Title, MessageBoxButton.OK);
        }

        private void FixAddressButton_Click(object sender, RoutedEventArgs e)
        {

            var order = (TCGPlayerOrder)OrdersListBox.SelectedItem; if (order == null) return;
            order.FixAddress();
            myPrinter.SetLabelAddress(order.Address);
            image.Source = myPrinter.render();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }
    }
}
