using System;
using System.Collections.Generic;
using System.Text;

namespace Flipdish.Recruiting.WebHookReceiver.Models
{
    public class OrderCreatedWebHook
    {
        public string Type { get; set; }

        public DateTime CreateTime { get;set;}
        public OrderCreatedEvent Body { get; set; }
    }
}
