using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ALS_BillingAppointmentAPI.Model
{
    public class InvoiceDetailModel
    {
        [JsonProperty("@odata.context")]
        public string odata_context { get; set; }
        [JsonProperty("value")]
        public List<InvoiceDTModel> InvoiceDT { get; set; }
    }
    public class InvoiceDTModel
    {
        public int InvoiceId { get; set; }
        public string Workorder { get; set; }
        public string InvoiceDeliveryType { get; set; }
        public string AnalysisReportDeliveryType { get; set; }
        public object InvoiceDeliveryLaboratory { get; set; }
        public string AnalysisReportDeliveryLaboratory { get; set; }
        public object QuoteCode { get; set; }
        public int CreditTerm { get; set; }
        public string InvoiceDeliveryClientCode { get; set; }
        public string AnalysisReportDeliverToClientCode { get; set; }
        public string AnalysisReportDeliverToClientName { get; set; }
        public string InvoiceDeliveryClientName { get; set; }
        public string InvoiceDeliverToPersonName { get; set; }
        public string InvoiceDeliverToPersonTelephone { get; set; }
        public string InvoiceDeliverToPersonMobile { get; set; }
        public string AnalysisReportDeliverToPersonName { get; set; }
        public string AnalysisReportDeliverToPersonTelephone { get; set; }
        public object AnalysisReportDeliverToPersonMobile { get; set; }
        public string InvoiceDeliverToOffice { get; set; }
        public string InvoiceDeliverToAddress1 { get; set; }
        public string InvoiceDeliverToAddress2 { get; set; }
        public string InvoiceDeliverToAddress3 { get; set; }
        public string InvoiceDeliverToCity { get; set; }
        public string InvoiceDeliverToState { get; set; }
        public string InvoiceDeliverToZip { get; set; }
        public string InvoiceDeliverToLocation { get; set; }
        public string AnalysisReportDeliverToOffice { get; set; }
        public string AnalysisReportDeliverToAddress1 { get; set; }
        public string AnalysisReportDeliverToAddress2 { get; set; }
        public string AnalysisReportDeliverToAddress3 { get; set; }
        public string AnalysisReportDeliverToCity { get; set; }
        public string AnalysisReportDeliverToState { get; set; }
        public string AnalysisReportDeliverToZip { get; set; }
        public string AnalysisReportDeliverToLocation { get; set; }
        public object InvoiceDeliveryNotes { get; set; }
        public object InvoiceDeliveryProcess { get; set; }
        public DateTime? InvoiceIssuedDate { get; set; }
        public DateTime? InvoiceDateOverride { get; set; }
        public string InvoiceNumbersIncludedInSearch { get; set; }
        public double TotalInvoiceAmountIncludingVat { get; set; }
        public string ReportToPersonName { get; set; }
        public string ReportToAddress1 { get; set; }
        public string ReportToAddress2 { get; set; }
        public string ReportToAddress3 { get; set; }
        public string ReportToCity { get; set; }
        public string ReportToState { get; set; }
        public string ReportToZip { get; set; }
        public string ReportToLocation { get; set; }
        public object PaymentDeliveryProcess { get; set; }
        public object PaymentDeliveryNotes { get; set; }
        public object ReportLocation { get; set; }
        public object DocumentType { get; set; }
        public int GenerateBillingAppointmentPerInvoice { get; set; }
        public bool InvoicePaid { get; set; }
        public string BillToClientCode { get; set; }
        public string BillToClientName { get; set; }
        public string BillToContactPerson { get; set; }
        public string ConsolidateBillingFlag { get; set; }
        public string ConsolidateBillingDeliverToOffice { get; set; }
        public string ConsolidateBillingToPersonName { get; set; }
        public string ConsolidateBillingDeliverToAddress1 { get; set; }
        public string ConsolidateBillingDeliverToAddress2 { get; set; }
        public string ConsolidateBillingDeliverToAddress3 { get; set; }
        public string ConsolidateBillingDeliverToCity { get; set; }
        public string ConsolidateBillingDeliverToState { get; set; }
        public string ConsolidateBillingDeliverToZip { get; set; }
        public string ConsolidateBillingDeliverToLocation { get; set; }
        public string ConsolidateBillingClientCode { get; set; }
        public string ConsolidateBillingCompanyName { get; set; }
        public string ConsolidateBillingToTelephone { get; set; }
        public string ConsolidateBillingToMobile { get; set; }
        public string ConsolidateBillingOtherLocation { get; set; }
        public string InvoiceStatus { get; set; }
        public string GlobalLocationNumber { get; set; }
    }
}
