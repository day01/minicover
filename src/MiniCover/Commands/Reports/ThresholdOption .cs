using MiniCover.Commands.Options;

namespace MiniCover.Commands.Reports
{
    public class ThresholdOption : MiniCoverOption<float>
    {
        protected const float DefaultValue = 90;
        protected string CoverageFilePath;
        protected override string Description => $"Coverage percentage threshold [default: {DefaultValue}]";
        protected override string OptionTemplate => "--threshold";

        public override void Validate()
        {
            if (!float.TryParse(Option.Value(), out var proposalThreshold))
            {
                proposalThreshold = DefaultValue;
            }

            ValueField = proposalThreshold / 100;
            Validated = true;
        }
    }
}