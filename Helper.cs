using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FHA_Demo
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Helper
    {
            [JsonProperty(PropertyName = "content")]
            public string content { get; set; }

            [JsonProperty(PropertyName = "sessionID")]
            public string sessionID { get; set; }
        }
}
