using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ALS_BillingAppointmentAPI.Model
{

    public class InvoiceHeaderModel
    {
        [JsonProperty("@odata.context")]
        public string odata_context { get; set; }
        [JsonProperty("value")]
        public List<InvoiceHDModel> InoviceHD { get; set; }


    }
    public class InvoiceHDModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string ClientCode { get; set; }
        public DateTime? InvoiceIssuedDate { get; set; }
        public double InvoiceAmount { get; set; }
        public string QuoteCode { get; set; }

    }
}
