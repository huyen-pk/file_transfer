namespace RdxFileTransfer.EventBus.Enums
{
    /// <summary>
    /// Status of a transfer event (folder or file).
    /// </summary>
    public enum TransferError
    {
        FolderNotFound = 1,
        FileNotFound = 2,
        Unknown = 3,
    }
}