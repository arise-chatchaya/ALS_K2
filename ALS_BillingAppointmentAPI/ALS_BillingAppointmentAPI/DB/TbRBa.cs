﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ALS_BillingAppointmentAPI.DB
{
    public partial class TbRBa
    {
        public Guid BaId { get; set; }
        public Guid? PackageId { get; set; }
        public string BaNo { get; set; }
        public string QuoteNo { get; set; }
        public string InvoiceToComany { get; set; }
        public string InvoiceToPerson { get; set; }
        public string InvoiceToAddress { get; set; }
        public string InvoiceCustCode { get; set; }
        public string InvoiceNote { get; set; }
        public string ReportToComany { get; set; }
        public string ReportsToAddress { get; set; }
        public string ReportsToTel { get; set; }
        public string InvoiceToTel { get; set; }
        public string DeliveryType { get; set; }
        public string DeliveryLab { get; set; }
        public string StatusCode { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }
}