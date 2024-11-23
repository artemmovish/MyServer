using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFApplication.Models
{
    public class RequestResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int? StatusCode { get; set; }
    }
}
