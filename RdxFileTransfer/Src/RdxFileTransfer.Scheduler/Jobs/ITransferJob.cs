namespace RdxFileTransfer.Scheduler.Jobs
{
    public interface ITransferJob
    {
        void Transfer(string sourceFolder, string destinationFolder, string extension);
    }
}
