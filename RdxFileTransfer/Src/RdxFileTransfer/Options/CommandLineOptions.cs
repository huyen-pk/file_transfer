using CommandLine;
using RdxFileTransfer.EventBus;

namespace RdxFileTransfer
{
    internal class CommandLineOptions
    {
        [Option('s', "source", Required = true, HelpText = "Source folder to transfer files from.")]
        public string SourceFolder { get; set; }

        [Option('d', "destination", Required = true, HelpText = "Destination folder to transfer files to.")]
        public string DestinationFolder { get; set; }
    }
}
