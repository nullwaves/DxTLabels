using DymoSDK.Interfaces;
using Microsoft.Win32;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            myPrinter.SetLabelAddress(order);
            image.Source = myPrinter.render();
        }

        private void PrintAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(TCGPlayerOrder order in myPrinter.Orders)
            {
                myPrinter.SetLabelAddress(order);
                myPrinter.PrintLabel((IPrinter)printerList.SelectedItem);
            }
            MessageBox.Show($"Printed {myPrinter.Orders.orders.Count} labels.", Title, MessageBoxButton.OK);
        }
    }
}
