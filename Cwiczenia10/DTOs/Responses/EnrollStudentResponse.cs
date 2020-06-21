using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cwiczenia5.DTOs.Responses
{
    public class EnrollStudentResponse
    {
        public string IndexNumber { get; set; }
        public int Semester { get; set; }
        public string Message { get; set; }
    }
}
