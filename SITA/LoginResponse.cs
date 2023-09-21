using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SITA
{
    public class LoginResponse
    {
        public string application_id { get; set; }
        public string version { get; set; }
        public string type { get; set; }
        public int message_id_number { get; set; }
        public int data_length { get; set; }
    }
}
