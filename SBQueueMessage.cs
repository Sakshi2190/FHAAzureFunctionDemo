using System;
using System.Collections.Generic;
using System.Text;

namespace FHA_Demo
{
    public class SBQueueMessage
    {
        public string QueueName { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
        public string SessionID { get; set; }
        public string ConnectionString { get; set; }
        public Dictionary<string, string> UserProperties { get; set; }

    }
}
