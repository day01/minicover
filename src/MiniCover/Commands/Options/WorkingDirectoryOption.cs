using System;
using System.IO;

namespace MiniCover.Commands.Options
{
    internal class WorkingDirectoryOption : MiniCoverOption<DirectoryInfo>
    {
        private const string DefaultValue = "./";
        protected override string Description => $"Change working directory [default: {DefaultValue}]";
        protected override string OptionTemplate => "--workdir";

        public override void Validate()
        {
            var workingDirectoryPath = Option.Value() ?? DefaultValue;
            Directory.CreateDirectory(workingDirectoryPath);

            ValueField = new DirectoryInfo(workingDirectoryPath);

            Console.WriteLine($"Changing working directory to '{ValueField.FullName}'");
            Directory.SetCurrentDirectory(ValueField.FullName);
            Validated = true;
        }
    }
}