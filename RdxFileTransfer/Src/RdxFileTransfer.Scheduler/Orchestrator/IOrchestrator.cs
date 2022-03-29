namespace RdxFileTransfer.Scheduler.Orchestrator
{
    /// <summary>
    /// Receive transfer commands and orchestrate workers.
    /// </summary>
    internal interface IOrchestrator: IDisposable
    {
        /// <summary>
        /// Start listening to user commands and orchestrate workers.
        /// </summary>
        void Start();
    }
}
