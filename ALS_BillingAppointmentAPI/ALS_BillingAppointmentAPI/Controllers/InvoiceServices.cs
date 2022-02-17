using ALS_BillingAppointmentAPI.DB;
using ALS_BillingAppointmentAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;


namespace ALS_BillingAppointmentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceServices : ControllerBase
    {
        private readonly K2_Billing_AppointmentContext db;
        private readonly IConfiguration _configuration;
        public static string baseUrl;
        public static string userName;
        public static string Password;

        public InvoiceServices(K2_Billing_AppointmentContext db, IConfiguration configuration)
        {
            this.db = db;
            _configuration = configuration;
            baseUrl = _configuration["AppSettings:BaseUrl"];
            userName = _configuration["AppSettings:Username"];
            Password = _configuration["AppSettings:Password"];
        }

        [HttpGet("GetInvoiceHeader")]
        public async Task<IActionResult> GetInvoiceHeader()
        {
            try
            {
                var getDate = DateTime.Today.ToString("yyyy-MM-dd"); //prod
                                
                var invoiceHeader = db.TbSInvoiceHeader.ToList();
                db.TbSInvoiceHeader.RemoveRange(invoiceHeader);
                db.SaveChanges();

                var url = baseUrl + $"/Default.GetInvoiceHeaders(startDate='{getDate}',endDate='{getDate}',invoiceNumber='',workorderCode='')"; //prod
                //var url = baseUrl + $"/Default.GetInvoiceHeaders(startDate='2021-09-01',endDate='2021-10-31',invoiceNumber='',workorderCode='')";
                //var url = baseUrl + $"/Default.GetInvoiceHeaders(startDate='2021-11-01',endDate='2021-12-06',invoiceNumber='',workorderCode='')"; //07/02/2022 change
                //var url = baseUrl + $"/Default.GetInvoiceHeaders(startDate='2022-01-04',endDate='2022-02-08',invoiceNumber='',workorderCode='')";//08/02/2022 change date to get data
                //var url = baseUrl + $"/Default.GetInvoiceHeaders(startDate='2022-02-04',endDate='2022-02-05',invoiceNumber='',workorderCode='')";
                var credentialsCache = new CredentialCache
            {
                {new Uri(url), "NTLM", new NetworkCredential(
                    userName,Password
                )}
            };
                var handler = new HttpClientHandler { Credentials = credentialsCache };
                var client = new HttpClient(handler);
                var res = await client.GetAsync(url);
                var result = (dynamic)null;

                InvoiceHeaderModel inv = new InvoiceHeaderModel();
                var task = client.GetAsync(url)
                  .ContinueWith((taskwithresponse) =>
                  {
                      var response = taskwithresponse.Result;
                      var jsonString = response.Content.ReadAsStringAsync();
                      jsonString.Wait();

                      var format = "dd/MM/yyyy"; // your datetime format
                      var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };

                      inv = JsonConvert.DeserializeObject<InvoiceHeaderModel>(jsonString.Result, dateTimeConverter);

                      List<TbSInvoiceHeader> entityInvoiceHeader = new List<TbSInvoiceHeader>();
                      if(inv.InoviceHD != null && inv.InoviceHD.Count > 0)
                      {
                          foreach (var item in inv.InoviceHD)
                          {
                              if (inv.InoviceHD != null && inv.InoviceHD.Count > 0)
                              {
                                  var add = (new TbSInvoiceHeader
                                  {
                                      Id = Guid.NewGuid(),
                                      InvoiceId = item.InvoiceId.ToString(),
                                      InvoiceNumber = item.InvoiceNumber,
                                      ClientCode = item.ClientCode,
                                      InvoiceIssuedDate = item.InvoiceIssuedDate,
                                      InvoiceAmount = item.InvoiceAmount.ToString(),
                                      QuoteCode = item.QuoteCode,
                                      CreateDate = DateTime.Now,
                                      CreateBy = "K2Admin",
                                  });
                                  db.TbSInvoiceHeader.Add(add);

                              }
                          }
                          db.SaveChanges();
                      }
                      

                  });
                task.Wait();

                //System.Threading.Thread.Sleep(3000);
                //var invoiceDetail = GetInvoiceDetail();

                return Ok(new BaseResponseViewModel<TbSInvoiceHeader>()
                {
                    is_error = false,
                    msg_alert = "Success",
                    data = null
                });
            }
            catch (Exception ex)
            {
                return Ok(new BaseResponseViewModel<TbSInvoiceHeader>()
                {
                    is_error = true,
                    msg_alert = "Fail" + ex.Message,
                    data = null
                });
            }

        }

        [HttpGet("GetInvoiceDetail")]
        public async Task<IActionResult> GetInvoiceDetail()
        {
            try
            {
                //List<TbSInvoiceHeader> invoiceHeader = new List<TbSInvoiceHeader>();
                var invoiceHeader = db.TbSInvoiceHeader.OrderBy(x => x.InvoiceId).ToList();
                var invoiceDT = db.TbSBillingAppointmentReportData.ToList();
                db.TbSBillingAppointmentReportData.RemoveRange(invoiceDT);
                db.SaveChanges();

                var urlGetDetail = baseUrl + $"/Default.GetBillingAppointmentReportData(invoiceNumbersList="; // prod
               //var urlGetDetail = baseUrl + $"/Default.GetBillingAppointmentReportData(invoiceNumbersList='1111275')"; //fix test
               //var urlGetDetail = baseUrl + $"/Default.GetBillingAppointmentReportData(invoiceNumbersList='1874295')"; //07022022

                if (invoiceHeader != null && invoiceHeader.Count > 0)
                {
                    foreach(var item in invoiceHeader)
                    {
                        var URLexe = urlGetDetail+ "'"+ item.InvoiceId + "')"; // prod
                        //var URLexe = urlGetDetail;


                        var credentialsCache = new CredentialCache
                        {
                        {new Uri(URLexe), "NTLM", new NetworkCredential(
                            userName,Password
                        )}
                        };
                        var handler = new HttpClientHandler { Credentials = credentialsCache };
                        var client = new HttpClient(handler);
                        var res = await client.GetAsync(URLexe);
                        if (res.IsSuccessStatusCode == false)
                        {
                            continue;
                        }

                        InvoiceDetailModel inv = new InvoiceDetailModel();
                        var task = client.GetAsync(URLexe)
                          .ContinueWith((taskwithresponse) =>
                          {
                              var response = taskwithresponse.Result;
                              var jsonString = response.Content.ReadAsStringAsync();
                              jsonString.Wait();

                              var format = "dd/MM/yyyy"; // your datetime format
                              var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };
                              inv = JsonConvert.DeserializeObject<InvoiceDetailModel>(jsonString.Result, dateTimeConverter);

                              if (inv.InvoiceDT != null && inv.InvoiceDT.Count > 0)
                              {
                                  foreach (var item in inv.InvoiceDT)
                                  {
                                      
                                      var add = (new TbSBillingAppointmentReportData
                                      {
                                          Id = Guid.NewGuid(),
                                          InvoiceId = item.InvoiceId.ToString(),
                                          InvoiceDeliveryType = ((item.InvoiceDeliveryType == null || item.InvoiceDeliveryType == "") ? null : item.InvoiceDeliveryType.ToString()),
                                          AnalysisReportDeliveryType = item.AnalysisReportDeliveryType,
                                          InvoiceDeliveryLaboratory = ((item.InvoiceDeliveryLaboratory == null|| item.InvoiceDeliveryLaboratory == "") ? null : item.InvoiceDeliveryLaboratory.ToString()),
                                          AnalysisReportDeliveryLaboratory = item.AnalysisReportDeliveryLaboratory,
                                          QuoteCode = ((item.QuoteCode == null || item.QuoteCode == "") ? null : item.QuoteCode.ToString()),
                                          CreditTerm = (item.CreditTerm == null ? null : item.CreditTerm.ToString()),
                                          InvoiceDeliveryClientCode = item.InvoiceDeliveryClientCode,
                                          AnalysisReportDeliverToClientCode = item.AnalysisReportDeliverToClientCode,
                                          AnalysisReportDeliverToClientName = item.AnalysisReportDeliverToClientName,
                                          InvoiceDeliveryClientName = item.InvoiceDeliveryClientName,
                                          InvoiceDeliverToPersonName = item.BillToClientName,
                                          InvoiceDeliverToPersonTelephone = item.InvoiceDeliverToPersonTelephone,
                                          InvoiceDeliverToPersonMobile = item.InvoiceDeliverToPersonMobile,
                                          AnalysisReportDeliverToPersonName = item.AnalysisReportDeliverToPersonName,
                                          AnalysisReportDeliverToPersonTelephone = item.AnalysisReportDeliverToPersonTelephone,
                                          AnalysisReportDeliverToPersonMobile = ((item.AnalysisReportDeliverToPersonMobile == null || item.AnalysisReportDeliverToPersonMobile == "") ? null : item.AnalysisReportDeliverToPersonMobile.ToString()),
                                          InvoiceDeliverToOffice = item.InvoiceDeliverToOffice,
                                          InvoiceDeliverToAddress1 = item.InvoiceDeliverToAddress1,
                                          InvoiceDeliverToAddress2 = item.InvoiceDeliverToAddress2,
                                          InvoiceDeliverToAddress3 = item.InvoiceDeliverToAddress3,
                                          InvoiceDeliverToCity = item.InvoiceDeliverToCity,
                                          InvoiceDeliverToState = item.InvoiceDeliverToState,
                                          InvoiceDeliverToZip = item.InvoiceDeliverToZip,
                                          InvoiceDeliverToLocation = item.InvoiceDeliverToLocation,
                                          //InvoiceDeliverToOffice = item.InvoiceDeliverToOffice,
                                          AnalysisReportDeliverToAddress1 = item.AnalysisReportDeliverToAddress1,
                                          AnalysisReportDeliverToAddress2 = item.AnalysisReportDeliverToAddress2,
                                          AnalysisReportDeliverToAddress3 = item.AnalysisReportDeliverToAddress3,
                                          AnalysisReportDeliverToCity = item.AnalysisReportDeliverToCity,
                                          AnalysisReportDeliverToState = item.AnalysisReportDeliverToState,
                                          AnalysisReportDeliverToZip = item.AnalysisReportDeliverToZip,
                                          AnalysisReportDeliverToLocation = item.AnalysisReportDeliverToLocation,
                                          InvoiceDeliveryNotes = ((item.InvoiceDeliveryNotes == null || item.InvoiceDeliveryNotes == "") ? null : item.InvoiceDeliveryNotes.ToString()),
                                          InvoiceDeliveryProcess = ((item.InvoiceDeliveryProcess == null || item.InvoiceDeliveryProcess == "") ? null : item.InvoiceDeliveryProcess.ToString()),
                                          InvoiceIssuedDate = item.InvoiceIssuedDate,
                                          InvoiceDateOverride = item.InvoiceDateOverride,
                                          InvoiceNumbersIncludedInSearch = item.InvoiceNumbersIncludedInSearch,
                                          TotalInvoiceAmountIncludingVat = item.TotalInvoiceAmountIncludingVat.ToString(),
                                          ReportToPersonName = item.ReportToPersonName,
                                          ReportToAddress1 = item.ReportToAddress1,
                                          ReportToAddress2 = item.ReportToAddress2,
                                          ReportToAddress3 = item.ReportToAddress3,
                                          ReportToCity = item.ReportToCity,
                                          ReportToState = item.ReportToState,
                                          ReportToZip = item.ReportToZip,
                                          ReportToLocation = item.ReportToLocation,
                                          //BillToClientCode = item.BillToClientCode,
                                          //BillToClientName = item.BillToClientName,
                                          //BillToContactPerson = item.BillToContactPerson,
                                          PaymentDeliveryProcess = (item.PaymentDeliveryProcess == null ? null : item.PaymentDeliveryProcess.ToString()),
                                          PaymentDeliveryNotes = (item.PaymentDeliveryNotes == null ? null : item.PaymentDeliveryNotes.ToString()),
                                          ReportLocation = ((item.ReportLocation == null || item.ReportLocation == "") ? null : item.ReportLocation.ToString()),
                                          InvoicePaid = (item.InvoicePaid == null ? null : item.InvoicePaid.ToString()),
                                          GenerateBillingAppointmentPerInvoice = (item.GenerateBillingAppointmentPerInvoice == null ? null : item.GenerateBillingAppointmentPerInvoice.ToString()),
                                          ConsolidateBillingFlag = (item.ConsolidateBillingFlag == null ? null : item.ConsolidateBillingFlag.ToString()),
                                          ConsolidateBillingToPersonName = (item.ConsolidateBillingToPersonName == null ? null : item.ConsolidateBillingToPersonName.ToString()),
                                          ConsolidateBillingDeliverToOffice = (item.ConsolidateBillingDeliverToOffice == null ? null : item.ConsolidateBillingDeliverToOffice.ToString()),
                                          ConsolidateBillingDeliverToAddress1 = (item.ConsolidateBillingDeliverToAddress1 == null ? null : item.ConsolidateBillingDeliverToAddress1.ToString()),
                                          ConsolidateBillingDeliverToAddress2 = (item.ConsolidateBillingDeliverToAddress2 == null ? null : item.ConsolidateBillingDeliverToAddress2.ToString()),
                                          ConsolidateBillingDeliverToAddress3 = (item.ConsolidateBillingDeliverToAddress3 == null ? null : item.ConsolidateBillingDeliverToAddress3.ToString()),
                                          ConsolidateBillingDeliverToCity = (item.ConsolidateBillingDeliverToCity == null ? null : item.ConsolidateBillingDeliverToCity.ToString()),
                                          ConsolidateBillingDeliverToState = (item.ConsolidateBillingDeliverToState == null ? null : item.ConsolidateBillingDeliverToState.ToString()),
                                          ConsolidateBillingDeliverToZip = (item.ConsolidateBillingDeliverToZip == null ? null : item.ConsolidateBillingDeliverToZip.ToString()),
                                          ConsolidateBillingDeliverToLocation = (item.ConsolidateBillingDeliverToLocation == null ? null : item.ConsolidateBillingDeliverToLocation.ToString()),
                                          ConsolidateBillingClientCode = (item.ConsolidateBillingClientCode == null ? null : item.ConsolidateBillingClientCode.ToString()),
                                          ConsolidateBillingCompanyName = (item.ConsolidateBillingCompanyName == null ? null : item.ConsolidateBillingCompanyName.ToString()),
                                          ConsolidateBillingToTelephone = (item.ConsolidateBillingToTelephone == null ? null : item.ConsolidateBillingToTelephone.ToString()),
                                          ConsolidateBillingToMobile = (item.ConsolidateBillingToMobile == null ? null : item.ConsolidateBillingToMobile.ToString()),
                                          ConsolidateBillingOtherLocation = (item.ConsolidateBillingOtherLocation == null ? null : item.ConsolidateBillingOtherLocation.ToString()),
                                          CeateBy = "K2Admin",
                                          CreateDate = DateTime.Now
                                      });;;
                                      db.TbSBillingAppointmentReportData.Add(add);
                                  }
                              }
                              db.SaveChanges();
                          });
                          task.Wait();
                    }
                }

                var generateBA = GenerateBA();

                return Ok(new BaseResponseViewModel<TbSBillingAppointmentReportData>()
                {
                    is_error = false,
                    msg_alert = "Success",
                    data = null
                });
            }
            catch (Exception ex)
            {
                return Ok(new BaseResponseViewModel<TbSBillingAppointmentReportData>()
                {
                    is_error = true,
                    msg_alert = "Fail" + ex.Message,
                    data = null
                });
            }

        }

        [HttpGet("GenerateBA")]
        public async Task<IActionResult> GenerateBA()
        {
            try
            {
               
                //List<TbSInvoiceHeader> invoiceHeader = new List<TbSInvoiceHeader>();
                //var invoiceHeader = db.TbSInvoiceHeader.ToList();
                //List<TbSBillingAppointmentReportData> invoiceDetail = new List<TbSBillingAppointmentReportData>();
                //var invoiceDetail = db.TbSBillingAppointmentReportData.OrderBy(x => { x.QuoteCode, x.InvoiceDeliveryClientCode, x.InvoiceDeliveryLaboratory, x.InvoiceDeliveryType, x.InvoiceIssuedDate}).ToList();
                var repoGELDetail = db.TbSBillingAppointmentReportData
                                    .OrderBy(x => x.InvoiceId)
                                    .ThenBy(x => x.InvoiceDeliveryClientCode)
                                    .ThenBy(x => x.QuoteCode)
                                    .ThenBy(x => x.InvoiceDeliveryLaboratory)
                                    .ThenBy(x => x.InvoiceDeliveryType)
                                    .ThenBy(x => x.InvoiceIssuedDate).ToList();
                
                //var custCode = (dynamic)null;
                //var quoteCode = (dynamic)null;
                //var Lab = (dynamic)null;
                //var OverrideDate = (dynamic)null;
                //var deliveryType = (dynamic)null;

                var baID = Guid.NewGuid();

                if (repoGELDetail != null && repoGELDetail.Count > 0)
                {
                    foreach (var item in repoGELDetail)
                    {
                        
                        var repoGELHeader = db.TbSInvoiceHeader.Where(x => x.InvoiceId == item.InvoiceId).FirstOrDefault();
                        //OverrideDate = (item.InvoiceDateOverride == null ? item.InvoiceIssuedDate : item.InvoiceDateOverride);
                        var HolidayList = db.TbMHolidays.ToList();
                        var Holidate = HolidayList.FirstOrDefault().HolidaysDate;
                                               
                        if(item.InvoiceDeliveryType != null )
                        {
                            //เช็คประเภทการส่ง
                            if (item.InvoiceDeliveryType == "HandDelivery")
                            {
                                //เช็ควันหยุดว่าใช่หรือไม่
                                var checkHoliday = db.TbMHolidays.Where(x => x.HolidaysDate == item.InvoiceIssuedDate).FirstOrDefault();
                                if (checkHoliday != null)
                                {
                                    DateTime IssueDate = (DateTime)checkHoliday.HolidaysDate;
                                    DayOfWeek IssueDay = IssueDate.DayOfWeek;
                                    string ConvertIssueDay = IssueDay.ToString();
                                    //เช็คว่าวันหยุดเป็นวันอะไร
                                    if (ConvertIssueDay == "Monday" || ConvertIssueDay == "Tuesday" || ConvertIssueDay == "Wednesday" || ConvertIssueDay == "Thursday")
                                    {
                                        DateTime DelayIssueDate = (DateTime)item.InvoiceIssuedDate;
                                        var CalIssueDate = DelayIssueDate.AddDays(2);
                                        item.InvoiceIssuedDate = CalIssueDate;
                                    }
                                    else if(ConvertIssueDay == "Friday")
                                    {
                                        DateTime DelayIssueDate = (DateTime)item.InvoiceIssuedDate;
                                        var CalIssueDate = DelayIssueDate.AddDays(4);
                                        item.InvoiceIssuedDate = CalIssueDate;
                                    }
                                    
                                    
                                }
                                else
                                {
                                    DateTime IssueDate = (DateTime)item.InvoiceIssuedDate;
                                    DayOfWeek IssueDay = IssueDate.DayOfWeek;
                                    string ConvertIssueDay = IssueDay.ToString();
                                    if (ConvertIssueDay == "Monday" || ConvertIssueDay == "Tuesday" || ConvertIssueDay == "Wednesday" || ConvertIssueDay == "Thursday")
                                    {
                                        DateTime DelayIssueDate = (DateTime)item.InvoiceIssuedDate;
                                        var CalIssueDate = DelayIssueDate.AddDays(1);
                                        item.InvoiceIssuedDate = CalIssueDate;

                                    }
                                    else if(ConvertIssueDay == "Friday" || ConvertIssueDay == "Saturday")
                                    {
                                        DateTime DelayIssueDate = (DateTime)item.InvoiceIssuedDate;
                                        var CalIssueDate = DelayIssueDate.AddDays(3);
                                        item.InvoiceIssuedDate = CalIssueDate;
                                    }
                                    if(ConvertIssueDay == "Sunday")
                                    {
                                        DateTime DelayIssueDate = (DateTime)item.InvoiceIssuedDate;
                                        var CalIssueDate = DelayIssueDate.AddDays(3);
                                        item.InvoiceIssuedDate = CalIssueDate;
                                    }
                                }
                                //POST DELIVERY
                            } else 
                                {
                                //เช็คว่าใช่วันหยุดหรือไม่
                                var checkHoliday = db.TbMHolidays.Where(x => x.HolidaysDate == item.InvoiceIssuedDate).FirstOrDefault();
                                if (checkHoliday != null)
                                {
                                    DateTime IssueDate = (DateTime)checkHoliday.HolidaysDate;
                                    DayOfWeek IssueDay = IssueDate.DayOfWeek;
                                    string ConvertIssueDay = IssueDay.ToString();
                                    //เช็คว่าวันหยุดเป็นวันอะไร
                                    if (ConvertIssueDay == "Monday" || ConvertIssueDay == "Tuesday" || ConvertIssueDay == "Wednesday" || ConvertIssueDay == "Thursday")
                                    {
                                        DateTime DelayIssueDate = (DateTime)item.InvoiceIssuedDate;
                                        var CalIssueDate = DelayIssueDate.AddDays(2);
                                        item.InvoiceIssuedDate = CalIssueDate;

                                    }
                                    else if(ConvertIssueDay == "Friday")
                                    {
                                        DateTime DelayIssueDate = (DateTime)item.InvoiceIssuedDate;
                                        var CalIssueDate = DelayIssueDate.AddDays(4);
                                        item.InvoiceIssuedDate = CalIssueDate;
                                    }
                                    //item.InvoiceIssuedDate = checkHoliday.HolidaysDate;
                                }
                                else
                                {
                                    DateTime IssueDate = (DateTime)item.InvoiceIssuedDate;
                                    DayOfWeek IssueDay = IssueDate.DayOfWeek;
                                    string ConvertIssueDay = IssueDay.ToString();
                                    if (ConvertIssueDay == "Monday" || ConvertIssueDay == "Tuesday" || ConvertIssueDay == "Wednesday" || ConvertIssueDay == "Thursday" || ConvertIssueDay == "Friday")
                                    {
                                        DateTime DelayIssueDate = (DateTime)item.InvoiceIssuedDate;
                                        var CalIssueDate = DelayIssueDate.AddDays(1);
                                        item.InvoiceIssuedDate = CalIssueDate;

                                    }
                                    else if(ConvertIssueDay == "Saturday" )
                                    {
                                        DateTime DelayIssueDate = (DateTime)item.InvoiceIssuedDate;
                                        var CalIssueDate = DelayIssueDate.AddDays(2);
                                        item.InvoiceIssuedDate = CalIssueDate;
                                    }
                                }
                            }
                        }
                        
                        
                        var repoInv = db.TbRInvoice.Where(x => x.CreateDate == DateTime.Today
                                                    && x.CustCode == repoGELHeader.ClientCode
                                                    && x.QuoteCode == item.QuoteCode
                                                    && x.DeliveryLab == Constant.FindLabBranchConstant(item.InvoiceDeliveryLaboratory)
                                                    && x.DeliveryType == Constant.FindDeliveryTypeConstant(item.InvoiceDeliveryType)
                                                    && x.InvoiceIssueDate == item.InvoiceIssuedDate
                                                    && x.InvoiceDateOverride == item.InvoiceDateOverride)
                                                    .FirstOrDefault();
                        if (repoInv != null)
                        {
                            var add = new TbRInvoice();

                            add.InvoiceId = Guid.NewGuid();
                            add.BaId = baID;
                            add.CustCode = repoGELHeader.ClientCode;
                            add.QuoteCode = ((item.QuoteCode == null || item.QuoteCode == "") ? null : item.QuoteCode);
                            add.InvoiceNo = repoGELHeader.InvoiceNumber;
                            add.BillToCompany = ((item.InvoiceDeliveryClientName == null || item.InvoiceDeliveryClientName == "") ? null : item.InvoiceDeliveryClientName);
                            add.DeliveryAddress = ((item.InvoiceDeliverToAddress1 == null || item.InvoiceDeliveryType == "") ? null : item.InvoiceDeliverToAddress1);
                            add.DeliveryToPerson = ((item.InvoiceDeliverToPersonName == null || item.InvoiceDeliverToPersonName == "") ? null : item.InvoiceDeliverToPersonName);
                            add.DeliveryToTel = ((item.InvoiceDeliverToPersonTelephone == null || item.InvoiceDeliverToPersonTelephone == "") ? null : item.InvoiceDeliverToPersonTelephone);
                            add.DeliveryType = Constant.FindDeliveryTypeConstant(item.InvoiceDeliveryType);
                            add.DeliveryLab = Constant.FindLabBranchConstant(item.InvoiceDeliveryLaboratory);
                            add.ReportToComany = ((item.ReportToPersonName == null || item.ReportToPersonName == "") ? null : item.ReportToPersonName);
                            add.ReportsToAddress = ((item.ReportToAddress1 == null || item.ReportToAddress1 == "") ? null : item.ReportToAddress1);
                            add.ReportsToTel = ((item.AnalysisReportDeliverToPersonTelephone == null || item.AnalysisReportDeliverToPersonTelephone == "") ? null : item.AnalysisReportDeliverToPersonTelephone);
                            add.InvoiceIssueDate = (item.InvoiceIssuedDate == null ? null : item.InvoiceIssuedDate);
                            add.InvoiceDateOverride = ((item.InvoiceDateOverride == null) ? null : item.InvoiceDateOverride);
                            add.InvoiceNote = ((item.InvoiceDeliveryNotes == null || item.InvoiceDeliveryNotes == "") ? null : item.InvoiceDeliveryNotes);
                            add.InvoiceProcess = ((item.InvoiceDeliveryProcess == null || item.InvoiceDeliveryProcess == "") ? null : item.InvoiceDeliveryProcess);
                            add.TotalInvoiceAmountIncVat = Convert.ToDecimal(item.TotalInvoiceAmountIncludingVat);
                            add.CreditTerm = item.CreditTerm;
                            add.StatusCode = "8";
                            add.GenerateBillingAppointmentPerInvoice = item.GenerateBillingAppointmentPerInvoice;
                            add.ConsolidateBillingFlag = ((item.ConsolidateBillingFlag == null || item.ConsolidateBillingFlag == "") ? null : item.ConsolidateBillingFlag);
                            add.ConsolidateBillingToPersonName = ((item.ConsolidateBillingToPersonName == null || item.ConsolidateBillingToPersonName == "") ? null : item.ConsolidateBillingToPersonName);
                            add.ConsolidateBillingDeliverToOffice = ((item.ConsolidateBillingDeliverToOffice == null || item.ConsolidateBillingDeliverToOffice == "") ? null : item.ConsolidateBillingDeliverToOffice);
                            add.ConsolidateBillingDeliverToAddress1 = ((item.ConsolidateBillingDeliverToAddress1 == null || item.ConsolidateBillingDeliverToAddress1 == "") ? null : item.ConsolidateBillingDeliverToAddress1);
                            add.ConsolidateBillingDeliverToAddress2 = ((item.ConsolidateBillingDeliverToAddress2 == null || item.ConsolidateBillingDeliverToAddress2 == "") ? null : item.ConsolidateBillingDeliverToAddress2);
                            add.ConsolidateBillingDeliverToAddress3 = ((item.ConsolidateBillingDeliverToAddress3 == null || item.ConsolidateBillingDeliverToAddress3 == "") ? null : item.ConsolidateBillingDeliverToAddress3);
                            add.ConsolidateBillingDeliverToCity = ((item.ConsolidateBillingDeliverToCity == null || item.ConsolidateBillingDeliverToCity == "") ? null : item.ConsolidateBillingDeliverToCity);
                            add.ConsolidateBillingDeliverToState = ((item.ConsolidateBillingDeliverToState == null || item.ConsolidateBillingDeliverToState == "") ? null : item.ConsolidateBillingDeliverToState);
                            add.ConsolidateBillingDeliverToZip = ((item.ConsolidateBillingDeliverToZip == null || item.ConsolidateBillingDeliverToZip == "") ? null : item.ConsolidateBillingDeliverToZip);
                            add.ConsolidateBillingDeliverToLocation = ((item.ConsolidateBillingDeliverToLocation == null || item.ConsolidateBillingDeliverToLocation == "") ? null : item.ConsolidateBillingDeliverToLocation);
                            add.ConsolidateBillingClientCode = ((item.ConsolidateBillingClientCode == null || item.ConsolidateBillingClientCode == "") ? null : item.ConsolidateBillingClientCode);
                            add.ConsolidateBillingCompanyName = ((item.ConsolidateBillingCompanyName == null || item.ConsolidateBillingCompanyName == "") ? null : item.ConsolidateBillingCompanyName);
                            add.ConsolidateBillingToTelephone = ((item.ConsolidateBillingToTelephone == null || item.ConsolidateBillingToTelephone == "") ? null : item.ConsolidateBillingToTelephone);
                            add.ConsolidateBillingToMobile = ((item.ConsolidateBillingToMobile == null || item.ConsolidateBillingToMobile == "") ? null : item.ConsolidateBillingToMobile);
                            add.ConsolidateBillingOtherLocation = ((item.ConsolidateBillingOtherLocation == null || item.ConsolidateBillingOtherLocation == "") ? null : item.ConsolidateBillingOtherLocation);
                            add.CreateDate = DateTime.Today;
                            add.CreateBy = "K2Admin";

                            db.TbRInvoice.Add(add);
                            db.SaveChanges();
                        }
                        else if (repoInv == null)
                        {
                            baID = Guid.NewGuid();
                            var add = new TbRInvoice();

                            add.InvoiceId = Guid.NewGuid();
                            add.BaId = baID;
                            add.CustCode = repoGELHeader.ClientCode;
                            add.QuoteCode = ((item.QuoteCode == null || item.QuoteCode == "") ? null : item.QuoteCode);
                            add.InvoiceNo = repoGELHeader.InvoiceNumber;
                            add.BillToCompany = ((item.InvoiceDeliveryClientName == null || item.InvoiceDeliveryClientName == "") ? null : item.InvoiceDeliveryClientName);
                            add.DeliveryAddress = ((item.InvoiceDeliverToAddress1 == null || item.InvoiceDeliveryType == "") ? null : item.InvoiceDeliverToAddress1);
                            add.DeliveryToPerson = ((item.InvoiceDeliverToPersonName == null || item.InvoiceDeliverToPersonName == "") ? null : item.InvoiceDeliverToPersonName);
                            add.DeliveryToTel = ((item.InvoiceDeliverToPersonTelephone == null || item.InvoiceDeliverToPersonTelephone == "") ? null : item.InvoiceDeliverToPersonTelephone);
                            add.DeliveryType = Constant.FindDeliveryTypeConstant(item.InvoiceDeliveryType);
                            add.DeliveryLab = Constant.FindLabBranchConstant(item.InvoiceDeliveryLaboratory);
                            add.ReportToComany = ((item.ReportToPersonName == null || item.ReportToPersonName == "") ? null : item.ReportToPersonName);
                            add.ReportsToAddress = ((item.ReportToAddress1 == null || item.ReportToAddress1 == "") ? null : item.ReportToAddress1);
                            add.ReportsToTel = ((item.AnalysisReportDeliverToPersonTelephone == null || item.AnalysisReportDeliverToPersonTelephone == "") ? null : item.AnalysisReportDeliverToPersonTelephone);
                            add.InvoiceIssueDate = (item.InvoiceIssuedDate == null ? null : item.InvoiceIssuedDate);
                            add.InvoiceDateOverride = ((item.InvoiceDateOverride == null) ? null : item.InvoiceDateOverride);
                            add.InvoiceNote = ((item.InvoiceDeliveryNotes == null || item.InvoiceDeliveryNotes == "") ? null : item.InvoiceDeliveryNotes);
                            add.InvoiceProcess = ((item.InvoiceDeliveryProcess == null || item.InvoiceDeliveryProcess == "") ? null : item.InvoiceDeliveryProcess);
                            add.TotalInvoiceAmountIncVat = Convert.ToDecimal(item.TotalInvoiceAmountIncludingVat);
                            add.CreditTerm = item.CreditTerm;
                            add.StatusCode = "8";
                            add.GenerateBillingAppointmentPerInvoice = item.GenerateBillingAppointmentPerInvoice;
                            add.ConsolidateBillingFlag = ((item.ConsolidateBillingFlag == null || item.ConsolidateBillingFlag == "") ? null : item.ConsolidateBillingFlag);
                            add.ConsolidateBillingToPersonName = ((item.ConsolidateBillingToPersonName == null || item.ConsolidateBillingToPersonName == "") ? null : item.ConsolidateBillingToPersonName);
                            add.ConsolidateBillingDeliverToOffice = ((item.ConsolidateBillingDeliverToOffice == null || item.ConsolidateBillingDeliverToOffice == "") ? null : item.ConsolidateBillingDeliverToOffice);
                            add.ConsolidateBillingDeliverToAddress1 = ((item.ConsolidateBillingDeliverToAddress1 == null || item.ConsolidateBillingDeliverToAddress1 == "") ? null : item.ConsolidateBillingDeliverToAddress1);
                            add.ConsolidateBillingDeliverToAddress2 = ((item.ConsolidateBillingDeliverToAddress2 == null || item.ConsolidateBillingDeliverToAddress2 == "") ? null : item.ConsolidateBillingDeliverToAddress2);
                            add.ConsolidateBillingDeliverToAddress3 = ((item.ConsolidateBillingDeliverToAddress3 == null || item.ConsolidateBillingDeliverToAddress3 == "") ? null : item.ConsolidateBillingDeliverToAddress3);
                            add.ConsolidateBillingDeliverToCity = ((item.ConsolidateBillingDeliverToCity == null || item.ConsolidateBillingDeliverToCity == "") ? null : item.ConsolidateBillingDeliverToCity);
                            add.ConsolidateBillingDeliverToState = ((item.ConsolidateBillingDeliverToState == null || item.ConsolidateBillingDeliverToState == "") ? null : item.ConsolidateBillingDeliverToState);
                            add.ConsolidateBillingDeliverToZip = ((item.ConsolidateBillingDeliverToZip == null || item.ConsolidateBillingDeliverToZip == "") ? null : item.ConsolidateBillingDeliverToZip);
                            add.ConsolidateBillingDeliverToLocation = ((item.ConsolidateBillingDeliverToLocation == null || item.ConsolidateBillingDeliverToLocation == "") ? null : item.ConsolidateBillingDeliverToLocation);
                            add.ConsolidateBillingClientCode = ((item.ConsolidateBillingClientCode == null || item.ConsolidateBillingClientCode == "") ? null : item.ConsolidateBillingClientCode);
                            add.ConsolidateBillingCompanyName = ((item.ConsolidateBillingCompanyName == null || item.ConsolidateBillingCompanyName == "") ? null : item.ConsolidateBillingCompanyName);
                            add.ConsolidateBillingToTelephone = ((item.ConsolidateBillingToTelephone == null || item.ConsolidateBillingToTelephone == "") ? null : item.ConsolidateBillingToTelephone);
                            add.ConsolidateBillingToMobile = ((item.ConsolidateBillingToMobile == null || item.ConsolidateBillingToMobile == "") ? null : item.ConsolidateBillingToMobile);
                            add.ConsolidateBillingOtherLocation = ((item.ConsolidateBillingOtherLocation == null || item.ConsolidateBillingOtherLocation == "") ? null : item.ConsolidateBillingOtherLocation);
                            add.CreateDate = DateTime.Today;
                            add.CreateBy = "K2Admin";

                            db.TbRInvoice.Add(add);
                            db.SaveChanges();
                        }


                        //if (item.InvoiceDeliveryClientCode == custCode)
                        //{
                        //    if (item.QuoteCode == quoteCode)
                        //    {
                        //        if (item.AnalysisReportDeliveryLaboratory == Lab)
                        //        {
                        //            if (item.InvoiceIssuedDate == OverrideDate || item.InvoiceDateOverride == OverrideDate)
                        //            {
                        //                if (item.InvoiceDeliveryType == deliveryType)
                        //                {
                        //                    #region // same baid
                        //                    var add = new TbRInvoice();

                        //                    add.InvoiceId = Guid.NewGuid();
                        //                    add.BaId = baID;
                        //                    add.CustCode = repoGELHeader.ClientCode;
                        //                    add.BillToCompany = item.InvoiceDeliveryClientName;
                        //                    add.DeliveryAddress = ((item.InvoiceDeliverToAddress1 == null || item.InvoiceDeliveryType == "") ? null : item.InvoiceDeliverToAddress1);
                        //                    add.DeliveryToPerson = ((item.InvoiceDeliverToPersonName == null || item.InvoiceDeliverToPersonName == "") ? null : item.InvoiceDeliverToPersonName);
                        //                    add.DeliveryToTel = ((item.InvoiceDeliverToPersonTelephone == null || item.InvoiceDeliverToPersonTelephone == "") ? null : item.InvoiceDeliverToPersonTelephone);
                        //                    add.DeliveryType = Constant.FindDeliveryTypeConstant(item.InvoiceDeliveryType);
                        //                    add.DeliveryLab = Constant.FindLabBranchConstant(item.InvoiceDeliveryLaboratory);
                        //                    add.ReportToComany = ((item.ReportToPersonName == null || item.ReportToPersonName == "") ? null : item.ReportToPersonName);
                        //                    add.ReportsToAddress = ((item.ReportToAddress1 == null || item.ReportToAddress1 == "") ? null : item.ReportToAddress1);
                        //                    add.ReportsToTel = ((item.AnalysisReportDeliverToPersonTelephone == null || item.AnalysisReportDeliverToPersonTelephone == "") ? null : item.AnalysisReportDeliverToPersonTelephone);
                        //                    add.InvoiceIssueDate = OverrideDate;
                        //                    add.InvoiceDateOverride = ((item.InvoiceDateOverride == null) ? null : item.InvoiceDateOverride);
                        //                    add.InvoiceNote = ((item.InvoiceDeliveryNotes == null || item.InvoiceDeliveryNotes == "") ? null : item.InvoiceDeliveryNotes);
                        //                    add.InvoiceProcess = ((item.InvoiceDeliveryProcess == null || item.InvoiceDeliveryProcess == "") ? null : item.InvoiceDeliveryProcess);
                        //                    add.TotalInvoiceAmountIncVat = Convert.ToDecimal(item.TotalInvoiceAmountIncludingVat);
                        //                    add.CreditTerm = item.CreditTerm;
                        //                    add.StatusCode = "8";
                        //                    add.CreateDate = DateTime.Today;
                        //                    add.CreateBy = "K2Admin";

                        //                    db.TbRInvoice.Add(add);
                        //                    db.SaveChanges();
                        //                    #endregion
                        //                }
                        //                else
                        //                {
                        //                    baID = Guid.NewGuid();
                        //                    deliveryType = item.InvoiceDeliveryType;

                        //                    #region 
                        //                    var add = new TbRInvoice();

                        //                    add.InvoiceId = Guid.NewGuid();
                        //                    add.BaId = baID;
                        //                    add.CustCode = repoGELHeader.ClientCode;
                        //                    add.BillToCompany = item.InvoiceDeliveryClientName;
                        //                    add.DeliveryAddress = ((item.InvoiceDeliverToAddress1 == null || item.InvoiceDeliveryType == "") ? null : item.InvoiceDeliverToAddress1);
                        //                    add.DeliveryToPerson = ((item.InvoiceDeliverToPersonName == null || item.InvoiceDeliverToPersonName == "") ? null : item.InvoiceDeliverToPersonName);
                        //                    add.DeliveryToTel = ((item.InvoiceDeliverToPersonTelephone == null || item.InvoiceDeliverToPersonTelephone == "") ? null : item.InvoiceDeliverToPersonTelephone);
                        //                    add.DeliveryType = Constant.FindDeliveryTypeConstant(item.InvoiceDeliveryType);
                        //                    add.DeliveryLab = Constant.FindLabBranchConstant(item.InvoiceDeliveryLaboratory);
                        //                    add.ReportToComany = ((item.ReportToPersonName == null || item.ReportToPersonName == "") ? null : item.ReportToPersonName);
                        //                    add.ReportsToAddress = ((item.ReportToAddress1 == null || item.ReportToAddress1 == "") ? null : item.ReportToAddress1);
                        //                    add.ReportsToTel = ((item.AnalysisReportDeliverToPersonTelephone == null || item.AnalysisReportDeliverToPersonTelephone == "") ? null : item.AnalysisReportDeliverToPersonTelephone);
                        //                    add.InvoiceIssueDate = OverrideDate;
                        //                    add.InvoiceDateOverride = ((item.InvoiceDateOverride == null) ? null : item.InvoiceDateOverride);
                        //                    add.InvoiceNote = ((item.InvoiceDeliveryNotes == null || item.InvoiceDeliveryNotes == "") ? null : item.InvoiceDeliveryNotes);
                        //                    add.InvoiceProcess = ((item.InvoiceDeliveryProcess == null || item.InvoiceDeliveryProcess == "") ? null : item.InvoiceDeliveryProcess);
                        //                    add.TotalInvoiceAmountIncVat = Convert.ToDecimal(item.TotalInvoiceAmountIncludingVat);
                        //                    add.CreditTerm = item.CreditTerm;
                        //                    add.StatusCode = "8";
                        //                    add.CreateDate = DateTime.Today;
                        //                    add.CreateBy = "K2Admin";

                        //                    db.TbRInvoice.Add(add);
                        //                    db.SaveChanges();
                        //                    #endregion
                        //                }
                        //            }
                        //            else
                        //            {
                        //                baID = Guid.NewGuid();
                        //                OverrideDate = (item.InvoiceDateOverride == null ? item.InvoiceIssuedDate : item.InvoiceDateOverride);

                        //                #region 
                        //                var add = new TbRInvoice();

                        //                add.InvoiceId = Guid.NewGuid();
                        //                add.BaId = baID;
                        //                add.CustCode = repoGELHeader.ClientCode;
                        //                add.BillToCompany = item.InvoiceDeliveryClientName;
                        //                add.DeliveryAddress = ((item.InvoiceDeliverToAddress1 == null || item.InvoiceDeliveryType == "") ? null : item.InvoiceDeliverToAddress1);
                        //                add.DeliveryToPerson = ((item.InvoiceDeliverToPersonName == null || item.InvoiceDeliverToPersonName == "") ? null : item.InvoiceDeliverToPersonName);
                        //                add.DeliveryToTel = ((item.InvoiceDeliverToPersonTelephone == null || item.InvoiceDeliverToPersonTelephone == "") ? null : item.InvoiceDeliverToPersonTelephone);
                        //                add.DeliveryType = Constant.FindDeliveryTypeConstant(item.InvoiceDeliveryType);
                        //                add.DeliveryLab = Constant.FindLabBranchConstant(item.InvoiceDeliveryLaboratory);
                        //                add.ReportToComany = ((item.ReportToPersonName == null || item.ReportToPersonName == "") ? null : item.ReportToPersonName);
                        //                add.ReportsToAddress = ((item.ReportToAddress1 == null || item.ReportToAddress1 == "") ? null : item.ReportToAddress1);
                        //                add.ReportsToTel = ((item.AnalysisReportDeliverToPersonTelephone == null || item.AnalysisReportDeliverToPersonTelephone == "") ? null : item.AnalysisReportDeliverToPersonTelephone);
                        //                add.InvoiceIssueDate = OverrideDate;
                        //                add.InvoiceDateOverride = ((item.InvoiceDateOverride == null) ? null : item.InvoiceDateOverride);
                        //                add.InvoiceNote = ((item.InvoiceDeliveryNotes == null || item.InvoiceDeliveryNotes == "") ? null : item.InvoiceDeliveryNotes);
                        //                add.InvoiceProcess = ((item.InvoiceDeliveryProcess == null || item.InvoiceDeliveryProcess == "") ? null : item.InvoiceDeliveryProcess);
                        //                add.TotalInvoiceAmountIncVat = Convert.ToDecimal(item.TotalInvoiceAmountIncludingVat);
                        //                add.CreditTerm = item.CreditTerm;
                        //                add.StatusCode = "8";
                        //                add.CreateDate = DateTime.Today;
                        //                add.CreateBy = "K2Admin";

                        //                db.TbRInvoice.Add(add);
                        //                db.SaveChanges();
                        //                #endregion
                        //            }
                        //        }
                        //        else if (item.InvoiceDeliveryLaboratory != Lab)
                        //        {
                        //            baID = Guid.NewGuid();
                        //            Lab = item.InvoiceDeliveryLaboratory;

                        //            #region 
                        //            var add = new TbRInvoice();

                        //            add.InvoiceId = Guid.NewGuid();
                        //            add.BaId = baID;
                        //            add.CustCode = repoGELHeader.ClientCode;
                        //            add.BillToCompany = item.InvoiceDeliveryClientName;
                        //            add.DeliveryAddress = ((item.InvoiceDeliverToAddress1 == null || item.InvoiceDeliveryType == "") ? null : item.InvoiceDeliverToAddress1);
                        //            add.DeliveryToPerson = ((item.InvoiceDeliverToPersonName == null || item.InvoiceDeliverToPersonName == "") ? null : item.InvoiceDeliverToPersonName);
                        //            add.DeliveryToTel = ((item.InvoiceDeliverToPersonTelephone == null || item.InvoiceDeliverToPersonTelephone == "") ? null : item.InvoiceDeliverToPersonTelephone);
                        //            add.DeliveryType = Constant.FindDeliveryTypeConstant(item.InvoiceDeliveryType);
                        //            add.DeliveryLab = Constant.FindLabBranchConstant(item.InvoiceDeliveryLaboratory);
                        //            add.ReportToComany = ((item.ReportToPersonName == null || item.ReportToPersonName == "") ? null : item.ReportToPersonName);
                        //            add.ReportsToAddress = ((item.ReportToAddress1 == null || item.ReportToAddress1 == "") ? null : item.ReportToAddress1);
                        //            add.ReportsToTel = ((item.AnalysisReportDeliverToPersonTelephone == null || item.AnalysisReportDeliverToPersonTelephone == "") ? null : item.AnalysisReportDeliverToPersonTelephone);
                        //            add.InvoiceIssueDate = OverrideDate;
                        //            add.InvoiceDateOverride = ((item.InvoiceDateOverride == null) ? null : item.InvoiceDateOverride);
                        //            add.InvoiceNote = ((item.InvoiceDeliveryNotes == null || item.InvoiceDeliveryNotes == "") ? null : item.InvoiceDeliveryNotes);
                        //            add.InvoiceProcess = ((item.InvoiceDeliveryProcess == null || item.InvoiceDeliveryProcess == "") ? null : item.InvoiceDeliveryProcess);
                        //            add.TotalInvoiceAmountIncVat = Convert.ToDecimal(item.TotalInvoiceAmountIncludingVat);
                        //            add.CreditTerm = item.CreditTerm;
                        //            add.StatusCode = "8";
                        //            add.CreateDate = DateTime.Today;
                        //            add.CreateBy = "K2Admin";

                        //            db.TbRInvoice.Add(add);
                        //            db.SaveChanges();
                        //            #endregion
                        //        }
                        //    }
                        //    else if (item.QuoteCode != quoteCode)
                        //    {
                        //        baID = Guid.NewGuid();
                        //        quoteCode = item.QuoteCode;

                        //        #region 
                        //        var add = new TbRInvoice();

                        //        add.InvoiceId = Guid.NewGuid();
                        //        add.BaId = baID;
                        //        add.CustCode = repoGELHeader.ClientCode;
                        //        add.BillToCompany = item.InvoiceDeliveryClientName;
                        //        add.DeliveryAddress = ((item.InvoiceDeliverToAddress1 == null || item.InvoiceDeliveryType == "") ? null : item.InvoiceDeliverToAddress1);
                        //        add.DeliveryToPerson = ((item.InvoiceDeliverToPersonName == null || item.InvoiceDeliverToPersonName == "") ? null : item.InvoiceDeliverToPersonName);
                        //        add.DeliveryToTel = ((item.InvoiceDeliverToPersonTelephone == null || item.InvoiceDeliverToPersonTelephone == "") ? null : item.InvoiceDeliverToPersonTelephone);
                        //        add.DeliveryType = Constant.FindDeliveryTypeConstant(item.InvoiceDeliveryType);
                        //        add.DeliveryLab = Constant.FindLabBranchConstant(item.InvoiceDeliveryLaboratory);
                        //        add.ReportToComany = ((item.ReportToPersonName == null || item.ReportToPersonName == "") ? null : item.ReportToPersonName);
                        //        add.ReportsToAddress = ((item.ReportToAddress1 == null || item.ReportToAddress1 == "") ? null : item.ReportToAddress1);
                        //        add.ReportsToTel = ((item.AnalysisReportDeliverToPersonTelephone == null || item.AnalysisReportDeliverToPersonTelephone == "") ? null : item.AnalysisReportDeliverToPersonTelephone);
                        //        add.InvoiceIssueDate = OverrideDate;
                        //        add.InvoiceDateOverride = ((item.InvoiceDateOverride == null) ? null : item.InvoiceDateOverride);
                        //        add.InvoiceNote = ((item.InvoiceDeliveryNotes == null || item.InvoiceDeliveryNotes == "") ? null : item.InvoiceDeliveryNotes);
                        //        add.InvoiceProcess = ((item.InvoiceDeliveryProcess == null || item.InvoiceDeliveryProcess == "") ? null : item.InvoiceDeliveryProcess);
                        //        add.TotalInvoiceAmountIncVat = Convert.ToDecimal(item.TotalInvoiceAmountIncludingVat);
                        //        add.CreditTerm = item.CreditTerm;
                        //        add.StatusCode = "8";
                        //        add.CreateDate = DateTime.Today;
                        //        add.CreateBy = "K2Admin";

                        //        db.TbRInvoice.Add(add);
                        //        db.SaveChanges();
                        //        #endregion
                        //    }
                        //}
                        //else if (item.InvoiceDeliveryClientCode != custCode)
                        //{
                        //    baID = Guid.NewGuid();
                        //    custCode = item.InvoiceDeliveryClientCode;

                        //    #region 
                        //    var add = new TbRInvoice();

                        //    add.InvoiceId = Guid.NewGuid();
                        //    add.BaId = baID;
                        //    add.CustCode = repoGELHeader.ClientCode;
                        //    add.BillToCompany = item.InvoiceDeliveryClientName;
                        //    add.DeliveryAddress = ((item.InvoiceDeliverToAddress1 == null || item.InvoiceDeliveryType == "") ? null : item.InvoiceDeliverToAddress1);
                        //    add.DeliveryToPerson = ((item.InvoiceDeliverToPersonName == null || item.InvoiceDeliverToPersonName == "") ? null : item.InvoiceDeliverToPersonName);
                        //    add.DeliveryToTel = ((item.InvoiceDeliverToPersonTelephone == null || item.InvoiceDeliverToPersonTelephone == "") ? null : item.InvoiceDeliverToPersonTelephone);
                        //    add.DeliveryType = Constant.FindDeliveryTypeConstant(item.InvoiceDeliveryType);
                        //    add.DeliveryLab = Constant.FindLabBranchConstant(item.InvoiceDeliveryLaboratory);
                        //    add.ReportToComany = ((item.ReportToPersonName == null || item.ReportToPersonName == "") ? null : item.ReportToPersonName);
                        //    add.ReportsToAddress = ((item.ReportToAddress1 == null || item.ReportToAddress1 == "") ? null : item.ReportToAddress1);
                        //    add.ReportsToTel = ((item.AnalysisReportDeliverToPersonTelephone == null || item.AnalysisReportDeliverToPersonTelephone == "") ? null : item.AnalysisReportDeliverToPersonTelephone);
                        //    add.InvoiceIssueDate = OverrideDate;
                        //    add.InvoiceDateOverride = ((item.InvoiceDateOverride == null) ? null : item.InvoiceDateOverride);
                        //    add.InvoiceNote = ((item.InvoiceDeliveryNotes == null || item.InvoiceDeliveryNotes == "") ? null : item.InvoiceDeliveryNotes);
                        //    add.InvoiceProcess = ((item.InvoiceDeliveryProcess == null || item.InvoiceDeliveryProcess == "") ? null : item.InvoiceDeliveryProcess);
                        //    add.TotalInvoiceAmountIncVat = Convert.ToDecimal(item.TotalInvoiceAmountIncludingVat);
                        //    add.CreditTerm = item.CreditTerm;
                        //    add.StatusCode = "8";
                        //    add.CreateDate = DateTime.Today;
                        //    add.CreateBy = "K2Admin";

                        //    db.TbRInvoice.Add(add);
                        //    db.SaveChanges();
                        //    #endregion 
                        //}
                    }
                }

                //--------------------------------------BA--------------------------------------//


                //var newBAID = db.TbRInvoice.Where(x => x.CreateDate == DateTime.Today).OrderBy(x => x.CreateDate).Distinct();
                //var newBAID = db.TbRInvoice.Select(x => x.BaId).Distinct().ToList();

                //var BAinvoiceToCompany = (dynamic)null;
                //var BAinvoiceToPerson = (dynamic)null;
                //var BAinvoiceToAddress = (dynamic)null;
                //var BAdeliveryType = (dynamic)null;
                //var BALab = (dynamic)null;
                //var BAbaid = (dynamic)null;

                //var pack_ID = Guid.NewGuid();

                //var newBAID = db.TbRInvoice.Where(x => x.CreateDate == DateTime.Today && x.BaId != null)
                //                            .OrderBy(x => x.BaId)
                //                            .ThenBy(x => x.CustCode)
                //                            .Select(x => x.BaId).ToList();
                //var newBAID = db.TbRInvoice.Where(x => x.CreateDate == DateTime.Today)
                //                            .OrderBy(x => x.BillToCompany)
                //                            .ThenBy(x => x.DeliveryToPerson)
                //                            .ThenBy(x => x.DeliveryAddress)
                //                            .ThenBy(x => x.DeliveryType)
                //                            .ThenBy(x => x.DeliveryLab)
                //                            .Select(x => x.BaId).Distinct().ToList();
                var newBAID = db.TbRInvoice.Where(x => x.CreateDate == DateTime.Today)
                                            .Select(x => x.BaId).Distinct().ToList();
                //var tbRInvoice = db.TbRInvoice.Where(x => x.CreateDate == DateTime.Today && x.BaId != null)
                //                                .OrderBy(x => x.BaId)
                //                                .ToList();
                if (newBAID != null && newBAID.Count > 0)
                {
                    foreach (var item in newBAID)
                    {

                        var repoInv = db.TbRInvoice.Where(x => x.BaId == item.Value).FirstOrDefault();
                        //var repoGELHeader = db.TbSInvoiceHeader.Where(x => x.ClientCode == repoInv.CustCode).FirstOrDefault();

                        var newBA_NO = new K2_Billing_AppointmentContextProcedures(db);
                        var rsSp = await newBA_NO.usp_GetNewRunningNoAsync("True", null, null);

                        var add = new TbRBa();
                        add.BaId = item.Value;
                        add.BaNo = rsSp.FirstOrDefault().NewRunningNo;
                        //add.PackageId = pack_ID;
                        add.QuoteNo = ((repoInv.QuoteCode == null || repoInv.QuoteCode == "") ? null : repoInv.QuoteCode);
                        add.InvoiceToComany = ((repoInv.BillToCompany == null || repoInv.BillToCompany == "") ? null : repoInv.BillToCompany);
                        add.InvoiceToPerson = ((repoInv.DeliveryToPerson == null || repoInv.DeliveryToPerson == "") ? null : repoInv.DeliveryToPerson);
                        add.InvoiceToAddress = ((repoInv.DeliveryAddress == null || repoInv.DeliveryAddress == "") ? null : repoInv.DeliveryAddress);
                        add.InvoiceCustCode = ((repoInv.CustCode == null || repoInv.CustCode == "") ? null : repoInv.CustCode);
                        add.InvoiceToTel = ((repoInv.DeliveryToTel == null || repoInv.DeliveryToTel == "") ? null : repoInv.DeliveryToTel);
                        add.DeliveryType = ((repoInv.DeliveryType == null || repoInv.DeliveryType == "") ? null : repoInv.DeliveryType);
                        add.DeliveryLab = ((repoInv.DeliveryLab == null || repoInv.DeliveryLab == "") ? null : repoInv.DeliveryLab);
                        add.InvoiceNote = ((repoInv.InvoiceNote == null || repoInv.InvoiceNote == "") ? null : repoInv.InvoiceNote);
                        add.ReportToComany = ((repoInv.ReportToComany == null || repoInv.ReportToComany == "") ? null : repoInv.ReportToComany);
                        add.ReportsToAddress = ((repoInv.ReportsToAddress == null || repoInv.ReportsToAddress == "") ? null : repoInv.ReportsToAddress);
                        add.ReportsToTel = ((repoInv.ReportsToTel == null || repoInv.ReportsToTel == "") ? null : repoInv.ReportsToTel);
                        add.StatusCode = "5";
                        add.CreateBy = "K2Admin";
                        add.CreateDate = DateTime.Today;

                        db.TbRBa.Add(add);
                        db.SaveChanges();

                        var repoBA = db.TbRBa.Where(x => x.BaId != item.Value
                                                    && x.InvoiceToComany == add.InvoiceToComany
                                                    && x.InvoiceToPerson == add.InvoiceToPerson
                                                    && x.InvoiceToAddress == add.InvoiceToAddress
                                                    && x.DeliveryType == add.DeliveryType
                                                    && x.DeliveryLab == add.DeliveryLab).FirstOrDefault();

                        if (repoBA == null)
                        {
                            add.PackageId = Guid.NewGuid();

                            db.TbRBa.Update(add);
                            db.SaveChanges();
                        }
                        else if (repoBA != null)
                        {
                            add.PackageId = repoBA.PackageId;

                            db.TbRBa.Update(add);
                            db.SaveChanges();
                        }

                    }
                }


                //--------------------------------------PACKAGE--------------------------------------//


                var newPackageID = db.TbRBa.Where(x => x.CreateDate == DateTime.Today)
                                        .Select(x => x.PackageId)
                                        .Distinct().ToList();
                if (newPackageID != null)
                {
                    foreach (var item in newPackageID)
                    {
                        var repoBA = db.TbRBa.Where(x => x.PackageId == item.Value).FirstOrDefault();
                        if(repoBA.DeliveryType == null || repoBA.DeliveryType == "")
                        {
                            repoBA.DeliveryType = Constant.FindDeliveryTypeConstant("HandDelivery");
                        }
                        if(repoBA.DeliveryLab == null || repoBA.DeliveryLab == "")
                        {
                            repoBA.DeliveryLab = Constant.FindLabBranchConstant("BK");
                        }

                        var newPAC_NO = new K2_Billing_AppointmentContextProcedures(db);
                        var rsSp = await newPAC_NO.usp_GetNewRunningNoAsync("False", repoBA.DeliveryType, repoBA.DeliveryLab);

                        var add = (new TbRPackage
                        {
                            PackageId = item.Value,
                            PackageNo = rsSp.FirstOrDefault().NewRunningNo,
                            InvoiceToAddress = ((repoBA.InvoiceToAddress == null || repoBA.InvoiceToAddress == "") ? null : repoBA.InvoiceToAddress),
                            InvoiceToPerson = ((repoBA.InvoiceToPerson == null || repoBA.InvoiceToPerson == "") ? null : repoBA.InvoiceToPerson),
                            InvoiceToCustCode = ((repoBA.InvoiceCustCode == null || repoBA.InvoiceCustCode == "") ? null : repoBA.InvoiceCustCode),
                            InvoiceToCompany = ((repoBA.InvoiceToComany == null || repoBA.InvoiceToComany == "") ? null : repoBA.InvoiceToComany),
                            InvoiceDeliveryPhone = ((repoBA.InvoiceToTel == null || repoBA.InvoiceToTel == "") ? null : repoBA.InvoiceToTel),
                            InvoiceDeliveryType = repoBA.DeliveryType,
                            InvoiceDeliveryLab = repoBA.DeliveryLab,
                            StatusCode = "2",
                            CreateBy = "K2Admin",
                            CreateDate = DateTime.Today
                        });
                        db.TbRPackage.Add(add);
                        db.SaveChanges();
                    }
                }

                return Ok(new BaseResponseViewModel<TbSBillingAppointmentReportData>()
                {
                    is_error = false,
                    msg_alert = "Success",
                    data = null
                });
            }
            catch (Exception ex)
            {
                return Ok(new BaseResponseViewModel<TbSBillingAppointmentReportData>()
                {
                    is_error = true,
                    msg_alert = "Fail" + ex.Message,
                    data = null
                });
            }

        }

        [HttpGet("GetInvoiceHeaderManual")]
        public async Task<IActionResult> GetInvoiceHeaderManual(string startDate, string endDate)
        {
            try
            {
                //var getDate = DateTime.Today.ToString("yyyy-MM-dd"); //prod

                //var invoiceHeader = db.TbSInvoiceHeader.ToList();
                //db.TbSInvoiceHeader.RemoveRange(invoiceHeader);
                //db.SaveChanges();

                //var url = baseUrl + $"/Default.GetInvoiceHeaders(startDate='{getDate}',endDate='{getDate}',invoiceNumber='',workorderCode='')"; //prod
                var url = baseUrl + $"/Default.GetInvoiceHeaders(startDate='{startDate}',endDate='{endDate}',invoiceNumber='',workorderCode='')";
                var credentialsCache = new CredentialCache
            {
                {new Uri(url), "NTLM", new NetworkCredential(
                    userName,Password
                )}
            };
                var handler = new HttpClientHandler { Credentials = credentialsCache };
                var client = new HttpClient(handler);
                var res = await client.GetAsync(url);
                var result = (dynamic)null;

                InvoiceHeaderModel inv = new InvoiceHeaderModel();
                var task = client.GetAsync(url)
                  .ContinueWith((taskwithresponse) =>
                  {
                      var response = taskwithresponse.Result;
                      var jsonString = response.Content.ReadAsStringAsync();
                      jsonString.Wait();

                      var format = "dd/MM/yyyy"; // your datetime format
                      var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };

                      inv = JsonConvert.DeserializeObject<InvoiceHeaderModel>(jsonString.Result, dateTimeConverter);

                      List<TbSInvoiceHeader> entityInvoiceHeader = new List<TbSInvoiceHeader>();
                      if (inv.InoviceHD != null && inv.InoviceHD.Count > 0)
                      {
                          foreach (var item in inv.InoviceHD)
                          {
                              if (inv.InoviceHD != null && inv.InoviceHD.Count > 0)
                              {
                                  var add = (new TbSInvoiceHeader
                                  {
                                      Id = Guid.NewGuid(),
                                      InvoiceId = item.InvoiceId.ToString(),
                                      InvoiceNumber = item.InvoiceNumber,
                                      ClientCode = item.ClientCode,
                                      InvoiceIssuedDate = item.InvoiceIssuedDate,
                                      InvoiceAmount = item.InvoiceAmount.ToString(),
                                      QuoteCode = item.QuoteCode,
                                      CreateDate = DateTime.Now,
                                      CreateBy = "K2Admin",
                                  });
                                  db.TbSInvoiceHeader.Add(add);

                              }
                          }
                          db.SaveChanges();
                      }


                  });
                task.Wait();

                //System.Threading.Thread.Sleep(3000);
                //var invoiceDetail = GetInvoiceDetail();

                return Ok(new BaseResponseViewModel<TbSInvoiceHeader>()
                {
                    is_error = false,
                    msg_alert = "Success",
                    data = null
                });
            }
            catch (Exception ex)
            {
                return Ok(new BaseResponseViewModel<TbSInvoiceHeader>()
                {
                    is_error = true,
                    msg_alert = "Fail" + ex.Message,
                    data = null
                });
            }

        }



    }
}
