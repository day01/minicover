using MiniCover.Commands.Options;
using System.IO;

namespace MiniCover.Commands.Reports
{
    internal class HtmlOutputFolderOption : MiniCoverOption<DirectoryInfo>
    {
        private const string DefaultValue = "./coverage-html";
        protected override string Description => $"Output folder for html report [default: {DefaultValue}]";
        protected override string OptionTemplate => "--output";

        public override void Validate()
        {
            var workingDirectoryPath = Option.Value() ?? DefaultValue;
            Directory.CreateDirectory(workingDirectoryPath);

            var workingDirectory = new DirectoryInfo(workingDirectoryPath);
            ValueField = workingDirectory;
            Validated = true;
        }
    }
}