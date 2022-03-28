using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Storage;

namespace RdxFileTransfer.Scheduler
{
    internal class TransferService: IDisposable
    {
        private BackgroundJobServer _server;

        /// <summary>
        /// Create and start background processing service to transfer files.
        /// </summary>
        /// <param name="dop">Degree of parallelism (number of workers).</param>
        public TransferService(int dop)
        {
            Start(dop);
        }

        private void Start(int dop = 1)
        {

            GlobalConfiguration.Configuration
                .UseMemoryStorage();

            var options = new BackgroundJobServerOptions { WorkerCount = dop};
            _server = new BackgroundJobServer(options);
            DeleteOldRecurringJobs();
        }

        private static void DeleteOldRecurringJobs()
        {
            var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            foreach (var job in recurringJobs)
            {
                RecurringJob.RemoveIfExists(job.Id);
            }
        }

        public void Dispose()
        {
            _server.Stop();
            _server.Dispose();
        }
    }
}
