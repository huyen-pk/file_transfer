namespace RdxFileTransfer.Scheduler
{
    internal class JobOrchestrator
    {
        public JobOrchestrator()
        {

        }

        public void ScheduleJobs(string sourceFolder, string destinationFolder)
        {
            var extensions = EnumerateExtensions(sourceFolder);
            foreach (var ext in extensions)
            {
                CreateJobForExtension(sourceFolder, destinationFolder, ext);
            }
            Task.Factory.StartNew(() =>
            {
                using var service = new TransferService(extensions.Count());
            });
        }

        public void OnJobsFinished()
        {

        }

        private void CreateJobForExtension(string sourceFolder, string destinationFolder, string extension)
        {

        }
        private List<string> EnumerateExtensions(string sourceFolder)
        {
            var files = Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories);
            return files.Select(f => Path.GetExtension(f)).ToList();
        }
    }
}
