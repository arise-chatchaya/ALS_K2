using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ALS_BillingAppointmentAPI.Model
{
    public class Constant
    {
        public static string FindDeliveryTypeConstant(string type)
        {
            //if ((type == null) || (type == ""))
            //    return null;
            string result = string.Empty;
            switch (type)
            {
                case null:
                    result = "4A2FD55D-A17A-4870-A27F-81C1A49A6203";
                    break;
                case "":
                    result = "4A2FD55D-A17A-4870-A27F-81C1A49A6203";
                    break;
                case "HandDelivery":
                    result = "4A2FD55D-A17A-4870-A27F-81C1A49A6203";
                    break;
                case "Pickup":
                    result = "41C92D23-43DB-4BD5-B1F1-7E2376DADF4F";
                    break;
                case "Post":
                    result = "3BBC4181-B275-4085-9D81-321F7EC45B88";
                    break;
                case "Courier":
                    result = "393AFE73-F531-4C0D-86A4-3272725485C3";
                    break;
            }
            return result;
        }

        public static string FindLabBranchConstant(string type)
        {
            if (type == null)
                return null;
            string result = string.Empty;
            switch (type)
            {
                case null:
                    result = "0E2AFBE2-7F69-437B-A59D-67BB0A026CF9";
                    break;
                case "":
                    result = "0E2AFBE2-7F69-437B-A59D-67BB0A026CF9";
                    break;
                case "BK":
                    result = "0E2AFBE2-7F69-437B-A59D-67BB0A026CF9";
                    break;
                case "RA":
                    result = "E1302CD5-F7A4-45C9-96F2-30205A7B4E21";
                    break;
                case "SO":
                    result = "F95199CA-F4E1-4436-8A47-D099E7BD094D";
                    break;
                case "CH":
                    result = "DEF081EB-5504-45DD-A87E-A48BF269BE93";
                    break;
                case "NK":
                    result = "40647163-9F58-412D-BA23-87D4A76B1761";
                    break;
                case "NN":
                    result = "44A6DB3B-B3EC-4D15-8C62-40176984CED2";
                    break;
                case "PH":
                    result = "51110998-485F-418E-9942-33D2AE102000";
                    break;
                case "SR":
                    result = "CE1C3211-4CC0-4067-B2F8-F57F5BC36F05";
                    break;
            }
            return result;
        }
    }
}
