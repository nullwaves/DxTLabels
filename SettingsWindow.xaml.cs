using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TCGPlayerAddressLabel.Properties;

namespace TCGPlayerAddressLabel
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
            Close();
        }

        private void ValidateReturnAddress_Click(object sender, RoutedEventArgs e)
        {
            if (!ReturnAddress.Text.Contains(Environment.NewLine))
            {
                MessageBox.Show("Invalid Address");
                return;
            }
            if (USPSApiURL.Text.Length < 10 || USPSApiUserID.Text.Length < 5)
            {
                MessageBox.Show("Insufficient USPS API credentials.");
                return;
            }
            var addrLines = ReturnAddress.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            USPSAddress address = new USPSAddress();
            if (addrLines.Length > 1) 
            {
                try
                {
                    address.Address2 = addrLines[1];
                    if (addrLines.Length == 3 && addrLines[2].Contains(','))
                    {
                        var commaPos = addrLines[2].IndexOf(',');
                        address.City = addrLines[2].Substring(0, commaPos);
                        address.State = addrLines[2].Substring(commaPos + 1, addrLines[2].IndexOfAny("0123456789".ToCharArray(), commaPos + 2) - (commaPos + 1)).Trim();
                    }
                    else if (addrLines.Length == 4 && addrLines[3].Contains(','))
                    {
                        address.Address1 = addrLines[2];
                        var commaPos = addrLines[3].IndexOf(',');
                        address.City = addrLines[3].Substring(0, commaPos);
                        address.State = addrLines[3].Substring(commaPos + 1, addrLines[3].IndexOfAny("0123456789".ToCharArray(), commaPos + 2) - (commaPos + 1)).Trim();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Return Address could not be parsed. Ensure that there is a comma between your City and State, " +
                        "and at least one number following the State.",
                        "Error Parsing Return Address",
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error
                        );
                    Debug.WriteLine(ex);
                    return;
                }
            }
            address.FixAddress();
            ReturnAddress.Text = addrLines[0] + Environment.NewLine + address;
        }
    }

    public class Converter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (ImageSource)value;
            return new Rect(0, 0, source.Width, source.Height);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
