using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ProcessGAR
{
    class Program
    {
        static void Main(string[] args)
        {
            var filename = @"C:\Jeff\STARK011320.txt";
            var reader = ReadAsLines(filename);

            var data = new DataTable();

            //this assume the first record is filled with the column names
            var headers = reader.First().Split('|');
            foreach (var header in headers)
                data.Columns.Add(header.Replace("\"", ""));

            var records = reader.Skip(1);
            foreach (var record in records)
                data.Rows.Add(record.Split('|'));

            var groupData = from b in data.AsEnumerable()
                            group b by b.Field<string>("orderid") into g
                            select new
                            {
                                orderid = g.Key
                            };
            foreach (var item in groupData)
            {
                var groupOrderId = data
                    .AsEnumerable()
                    .Where(myRow => myRow.Field<string>("orderid") == item.orderid);

                int i = 0;
                
                BillingAddress billingAddress = new BillingAddress();
                Request request = new Request()
                {
                    Charges = new List<Charge>(),
                    Items = new List<Item>(),
                    ShipTos = new List<ShipTo>()
                };
                Customer customer = new Customer();

                Payment payment = new Payment();
                CashPayment cashPayment = new CashPayment();

                ShipTo shipTo = new ShipTo();
                Recipient recipient = new Recipient();
                ShippingAddress shippingAddress = new ShippingAddress();

                All all = new All();

                foreach (var items in groupOrderId)
                {
                    string address1 = "";

                    if (i == 0)
                    {
                        address1 = items.Field<string>("s_address").Replace("\"", "");

                        billingAddress.Address1 = items.Field<string>("b_address").Replace("\"", "");
                        billingAddress.Address2 = items.Field<string>("b_address2").Replace("\"", "");
                        billingAddress.City = items.Field<string>("b_city").Replace("\"", "");
                        billingAddress.Contact = items.Field<string>("title").Replace("\"", "") + " " + items.Field<string>("firstname").Replace("\"", "") + " " + items.Field<string>("lastname").Replace("\"", "");
                        billingAddress.Email = items.Field<string>("email").Replace("\"", "");
                        billingAddress.Fax = items.Field<string>("fax").Replace("\"", "");
                        billingAddress.Organization = items.Field<string>("company").Replace("\"", "");
                        billingAddress.PhoneNumber = items.Field<string>("phone").Replace("\"", "");
                        billingAddress.PostalCode = items.Field<string>("b_zipcode").Replace("\"", "");
                        request.BillingAddress = billingAddress;

                        Charge charge = new Charge
                        {
                            Amount = items.Field<string>("total").Replace("\"", ""),
                            ChargeCode = items.Field<string>("payment_method").Replace("\"", "")
                        };
                        request.Charges.Add(charge);

                        customer.CustomerNumber = items.Field<string>("customer").Replace("\"", "");
                        customer.FirstName = items.Field<string>("firstname").Replace("\"", "");
                        customer.LastName = items.Field<string>("lastname").Replace("\"", "");
                        customer.MiddleName = items.Field<string>("customer").Replace("\"", "");

                        request.Customer = customer;

                        request.Discounts = items.Field<string>("discount").Replace("\"", "");

                        request.OrderDate = items.Field<string>("Date").Replace("\"", ""); ;

                        request.OrderReferenceNumber = items.Field<string>("orderid").Replace("\"", "");

                        cashPayment.Amount = items.Field<string>("total").Replace("\"", "");
                        payment.CashPayment = cashPayment;
                        request.Payment = payment;
                    }
                    Item item1 = new Item
                    {
                        Amount = items.Field<string>("price").Replace("\"", ""),
                        ItemNumber = items.Field<string>("productcode").Replace("\"", ""),
                        LineNumber = int.Parse(items.Field<string>("productid").Replace("\"", "")),
                        Personalizations = items.Field<string>("product_options").Replace("\"", ""),
                        Quantity = 1

                    };
                    request.Items.Add(item1);

                    Recipient recipient1 = new Recipient
                    {
                        FirstName = items.Field<string>("s_firstname").Replace("\"", ""),
                        LastName = items.Field<string>("s_lastname").Replace("\"", ""),
                    };

                    ShippingAddress shippingAddress1 = new ShippingAddress
                    { 
                        Address1 = items.Field<string>("s_address").Replace("\"", ""),
                        Address2 = items.Field<string>("s_address_2").Replace("\"", ""),
                        City = items.Field<string>("s_city").Replace("\"", ""),
                        Contact = items.Field<string>("title").Replace("\"", "") + " " + items.Field<string>("firstname").Replace("\"", "") + " " + items.Field<string>("lastname").Replace("\"", ""),
                        Email = items.Field<string>("email").Replace("\"", ""),
                        Fax = items.Field<string>("fax").Replace("\"", ""),
                        Organization = items.Field<string>("company").Replace("\"", ""),
                        PhoneNumber = items.Field<string>("phone").Replace("\"", ""),
                        PostalCode = items.Field<string>("s_zipcode").Replace("\"", ""),
                        Region = items.Field<string>("s_state").Replace("\"", "")
                    };


                    ShipTo shipTo1 = new ShipTo
                    {
                        CarrierAccountNumber = items.Field<string>("shippingid").Replace("\"", ""),
                        //ExternalShipCode = items.Field<string>("shipping_method").Replace("\"", ""),
                        Recipient = recipient1,
                        ShipLineID = i + 1,
                        ShippingAddress = shippingAddress1,
                        ShippingMethodCode = items.Field<string>("shipping_method").Replace("\"", "")
                    };

                    if (i==0)
                        request.ShipTos.Add(shipTo1);

                    if (i>0 & address1 != items.Field<string>("s_address").Replace("\"", ""))
                        request.ShipTos.Add(shipTo1);
                    i++;
                }
                all.Request = request;
                all.CompanyId = "GAR";

                string json = JsonConvert.SerializeObject(all, Formatting.Indented);

                using (StreamWriter writetext = new StreamWriter(@"C:\Jeff\json.txt", true))
                {
                    writetext.WriteLine(json);
                    //Console.WriteLine(json);
                    //Console.ReadLine();
                }
            }
        }

        private static IEnumerable<string> ReadAsLines(string filename)
        {
            using (var reader = new StreamReader(filename))
                while (!reader.EndOfStream)
                    yield return reader.ReadLine();
        }
    }



    public class All
    {
        public string CompanyId { get; set; }
        public Request Request { get; set; }
    }

    
    public class BillingAddress
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public object Contact { get; set; }
        public int CountryCode { get; set; }
        public string Email { get; set; }
        public object Fax { get; set; }
        public bool IsGiftAddress { get; set; }
        public object Organization { get; set; }
        public string PhoneNumber { get; set; }
        public string PostalCode { get; set; }
        public object Region { get; set; }
        public int StateOrProvinceCode { get; set; }
    }

    public class Charge
    {
        public string Amount { get; set; }
        public string ChargeCode { get; set; }
    }

    public class Customer
    {
        public object CustomerNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string RefCustomerNumber { get; set; }
    }

    public class Item
    {
        public string Amount { get; set; }
        public object Charges { get; set; }
        public object Comments { get; set; }
        public object Discounts { get; set; }
        public object GroupName { get; set; }
        public string ItemNumber { get; set; }
        public int LineNumber { get; set; }
        public int ParentLineNumber { get; set; }
        public object Personalizations { get; set; }
        public int Quantity { get; set; }
        public int ShipTo { get; set; }
    }

    public class CashPayment
    {
        public string Amount { get; set; }
        public DateTime ChequeDate { get; set; }
        public string ChequeNumber { get; set; }
    }

    public class Payment
    {
        public object AuthorizeProfilePayment { get; set; }
        public CashPayment CashPayment { get; set; }
        public object CreditCardPayments { get; set; }
        public bool IsOnAccountPayment { get; set; }
        public object PayPalPayment { get; set; }
        public object RedeemablePayments { get; set; }
        public object WireTransferPayment { get; set; }
    }

    public class Recipient
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public object MiddleName { get; set; }
    }

    public class ShippingAddress
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public object Contact { get; set; }
        public int CountryCode { get; set; }
        public object Email { get; set; }
        public object Fax { get; set; }
        public bool IsGiftAddress { get; set; }
        public string Organization { get; set; }
        public string PhoneNumber { get; set; }
        public string PostalCode { get; set; }
        public object Region { get; set; }
        public int StateOrProvinceCode { get; set; }
    }

    public class ShipTo
    {
        public object CarrierAccountNumber { get; set; }
        public string ExternalShipCode { get; set; }
        public Recipient Recipient { get; set; }
        public int ShipLineID { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public object ShippingMethodCode { get; set; }
    }

    public class Request
    {
        public BillingAddress BillingAddress { get; set; }
        public List<Charge> Charges { get; set; }
        public int CountryCode { get; set; }
        public int CurrencyCode { get; set; }
        public Customer Customer { get; set; }
        public object CustomerPONumber { get; set; }
        public object Discounts { get; set; }
        public object FulfillmentWarehouse { get; set; }
        public bool IsWholesaleDirect { get; set; }
        public List<Item> Items { get; set; }
        public string OrderDate { get; set; }
        public string OrderMessage { get; set; }
        public object OrderMessage1 { get; set; }
        public object OrderMessage2 { get; set; }
        public object OrderMessage3 { get; set; }
        public object OrderMessage4 { get; set; }
        public object OrderMessage5 { get; set; }
        public object OrderMessage6 { get; set; }
        public string OrderReferenceNumber { get; set; }
        public Payment Payment { get; set; }
        public List<ShipTo> ShipTos { get; set; }
        public string Source { get; set; }
        public string SourceCode { get; set; }
    }

    public class RootObject
    {
        public string CompanyId { get; set; }
        public Request Request { get; set; }
    }
}
