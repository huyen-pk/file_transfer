using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdxFileTransfer.EventBus.RabbitMq
{
    public static class Queues
    {
        public const string FOLDER_QUEUE = "transfer_jobs";
        public const string EMPTY_EXTENSION_QUEUE = "no_ext";
        public const string ERROR_QUEUE = "error";
        public const string SUCCESS_QUEUE = "success";
    }
}
