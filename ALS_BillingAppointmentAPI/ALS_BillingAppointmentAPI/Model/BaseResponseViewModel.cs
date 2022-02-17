using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ALS_BillingAppointmentAPI.Model
{
    public class BaseResponseViewModel<T>
    {
            public bool is_error { get; set; } = false;
            public string msg_alert { get; set; } = string.Empty;
            public T data { get; set; }
        
    }
}
