using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using TCGPlayerAddressLabel.Properties;

namespace TCGPlayerAddressLabel
{
    public class USPSAddress
    {
        public int ID { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip5 { get; set; }
        public string Zip4 { get; set; }

        public XDocument GetAddressValidateRequestDocument()
        {
            XDocument requestDoc = new XDocument(
                new XElement("AddressValidateRequest",
                    new XAttribute("USERID", Settings.Default.UspsApiUserID),
                    new XElement("Revision", "1"),
                    new XElement("Address",
                        new XAttribute("ID", ID),
                        new XElement("Address1", Address1),
                        new XElement("Address2", Address2),
                        new XElement("City", City),
                        new XElement("State", State),
                        new XElement("Zip5", Zip5),
                        new XElement("Zip4", Zip4)
                    )
                )
            );
            return requestDoc;
        }

        public bool FixAddress()
        {
            var request = GetAddressValidateRequestDocument();
            var result = new USPSApi(Settings.Default.UspsApiUrl).Request(USPSApi.Endpoint.VERIFY, request);
            if (result != null)
            {
                var addressElement = result.Descendants("Address").First();
                Address1 = GetXMLElement(addressElement, "Address1");
                Address2 = GetXMLElement(addressElement, "Address2");
                City = GetXMLElement(addressElement, "City");
                State = GetXMLElement(addressElement, "State");
                Zip5 = GetXMLElement(addressElement, "Zip5");
                Zip4 = GetXMLElement(addressElement, "Zip4");
                return true;
            }
            else return false;
        }

        internal static string GetXMLElement(XElement element, string name)
        {
            var el = element.Element(name);
            if (el != null)
            {
                return el.Value;
            }
            return "";
        }

        public override string ToString()
        {
            var sb = new StringBuilder(string.Empty);
            if (Address2.Length > 0) sb.Append(Address2);
            if (Address1.Length > 0) sb.Append(" " + Address1);
            sb.AppendLine();
            sb.Append($"{City}, {State} {Zip5}");
            if (Zip4.Length > 0) sb.Append($"-{Zip4}");
            return sb.ToString();
        }
    }

    internal class USPSApiEndpoint
    {
        public string Target { get; set; }
        public USPSApiEndpoint(string target) 
        {
            Target = target;
        }
    }

    internal class USPSApi
    {
        private string _url;

        public enum Endpoint
        {
            VERIFY,
        }

        private Dictionary<Endpoint, USPSApiEndpoint> _endpoints = new Dictionary<Endpoint, USPSApiEndpoint>()
        {
            { Endpoint.VERIFY, new USPSApiEndpoint("API=Verify") },
        };

        public USPSApi(string apiUrl) 
        {
            _url = apiUrl;
        }

        public XDocument Request(Endpoint endpoint, XDocument request)
        {
            try
            {
                var url = $"{_url}?{_endpoints[endpoint].Target}&XML=" + request;
                Debug.WriteLine(url);
                var client = new WebClient();
                var response = client.DownloadString(url);

                var xdoc = XDocument.Parse(response.ToString());
                return xdoc;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
