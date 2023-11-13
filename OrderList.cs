using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DxTLabel
{
    internal class TCGPlayerOrder
    {
        public string OrderNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        private USPSAddress _address;
        public string Country { get; set; }
        public string OrderDate { get; set; }
        public string ProductWeight { get; set; }
        public string ShippingMethod { get; set; }
        public int ItemCount { get; set; }
        public decimal ValueOfProducts { get; set; }
        public string ShippingFeePaid { get; set; }
        public string TrackingNumber { get; set; }

        public TCGPlayerOrder(string orderNumber, string firstName, string lastName, string address1, string address2, string city, string state, string postalCode, string country, string orderDate, string productWeight, string shippingMethod, int itemCount, decimal valueOfProducts, string shippingFeePaid, string trackingNumber)
        {
            OrderNumber = orderNumber;
            FirstName = firstName;
            LastName = lastName;
            var zip5 = postalCode.Length > 5 ? postalCode.Substring(0, 5) : postalCode;
            var zip4 = postalCode.Length > 9 ? postalCode.Substring(6) : "";
            _address = new USPSAddress()
            {
                ID = 0,
                Address2 = address1, // USPS interprets the suite/apt as Line 1
                Address1 = address2, // and number/street as Line 2
                City = city,
                State = state,
                Zip4 = zip4,
                Zip5 = zip5,

            };
            Country = country;
            OrderDate = orderDate;
            ProductWeight = productWeight;
            ShippingMethod = shippingMethod;
            ItemCount = itemCount;
            ValueOfProducts = valueOfProducts;
            ShippingFeePaid = shippingFeePaid;
            TrackingNumber = trackingNumber;
        }

        public string Address
        {
            get
            {
                return $"{FirstName} {LastName}\n" +
                    $"{_address}";
            }
        }

        public bool FixAddress() => _address.FixAddress();

        public override string ToString()
        {
            return $"{OrderNumber} - {FirstName} {LastName}";
        }
    }


    public class OrderList : IEnumerable
    {
        internal List<TCGPlayerOrder> orders;

        public OrderList()
        {
            orders = new List<TCGPlayerOrder>();
        }

        public IEnumerator GetEnumerator()
        {
            return orders.GetEnumerator();
        }

        public int LoadFromFile(string path)
        {
            orders.Clear();
            var handle = new StreamReader(File.OpenRead(path));
            string line = handle.ReadLine();
            while (!handle.EndOfStream)
            {
                line = handle.ReadLine();
                line = line.Replace("\"", "").Replace("#", "UNIT ");
                var data = line.Split(',');
                if (data.Length >= 14)
                {
                    int count = int.TryParse(data[12], out int _) ? int.Parse(data[12]) : 0;
                    decimal value = int.TryParse(data[13], out int _) ? int.Parse(data[13]) : 0;
                    orders.Add(new TCGPlayerOrder(
                        data[0], // Order Number
                        data[1], // First Name
                        data[2], // Last Name
                        data[3], // Address1
                        data[4], // Address2
                        data[5], // City
                        data[6], // State
                        data[7], // PostalCode
                        data[8], // Country
                        data[9], // OrderDate
                        data[10], // ProductWeight
                        data[11], // Shipping Method
                        count,
                        value,
                        data.Length > 14 ? data[14] : "",
                        data.Length > 15 ? data[15] : ""));
                }
            }
            return orders.Count();
        }
    }
}