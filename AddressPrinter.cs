
using DymoSDK.Implementations;
using DymoSDK.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace TCGPlayerAddressLabel
{
    public class AddressPrinter
    {
        private IDymoPrinter _printer;
        private IDymoLabel _label;
        public IPrinter[] Printers;
        public OrderList Orders;
        internal TCGPlayerOrder SelectedOrder;

        public AddressPrinter()
        {
            _printer = DymoPrinter.Instance;
            _label = DymoLabel.Instance;
            Printers = _printer.GetPrinters().Result.ToArray();
            Orders = new OrderList();

            _label.LoadLabelFromFilePath("default.dymo");
        }

        public void LoadAddresses(string filePath)
        {
            try
            {
                Orders.LoadFromFile(filePath);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }


        public void PrintLabel(IPrinter printer)
        {
            _printer.PrintLabel(_label, printer.Name);
        }

        internal void SetLabelAddress(string address)
        {
            var addressObj = _label.GetLabelObject("IAddressObject0");
            _label.UpdateLabelObject(addressObj, address);
        }

        public BitmapImage render()
        {
            using (MemoryStream memory = new MemoryStream(_label.GetPreviewLabel()))
            {
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        internal void FixAllAddresses()
        {
            foreach (var order in Orders.orders)
            {
                order.FixAddress();
            }
        }
    }
}
