using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdxFileTransfer.EventBus.RabbitMq
{
    public class RabbitMqConfig
    {
        public string ServerUri { get; set; }
        public string ExchangeKey { get; set; }
    }
}
