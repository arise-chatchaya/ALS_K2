﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ALS_BillingAppointmentAPI.DB
{
    public partial class TbRCbPackage
    {
        public Guid CbPackageId { get; set; }
        public string CbPackageNo { get; set; }
        public string CbPackageName { get; set; }
        public DateTime? CbPackageDeliveryDate { get; set; }
        public string CbPackageDesc { get; set; }
        public string CbInvoiceToAddress { get; set; }
        public string CbInvoiceToPerson { get; set; }
        public string CbInvoiceToCustCode { get; set; }
        public string CbInvoiceToCompany { get; set; }
        public string CbInvoiceDeliveryPhone { get; set; }
        public string CbInvoiceDeliveryType { get; set; }
        public string CbInvoiceDeliveryLab { get; set; }
        public DateTime? CbLogisticsDeliveryDate { get; set; }
        public string CbStatusCode { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdateBy { get; set; }
    }
}