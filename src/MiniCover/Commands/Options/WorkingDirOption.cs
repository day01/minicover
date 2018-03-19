﻿using System;
using System.IO;

namespace MiniCover.Commands.Options
{
    internal class WorkingDirOption : MiniCoverOption<DirectoryInfo>
    {
        private const string DefaultValue = "./";
        protected override string Description => $"Solution directory [default: {DefaultValue}]";
        protected override string OptionTemplate => "--workdir";

        public override void Validate()
        {
            var solutionPath = Option.Value() ?? DefaultValue;
            if (!Directory.Exists(solutionPath))
            {
                throw new ArgumentException($"Solution directory does not exist '{solutionPath}'");
            }

            Validated = true;
            ValueField = new DirectoryInfo(solutionPath);
        }
    }
}