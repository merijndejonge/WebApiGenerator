using OpenSoftware.OptionParsing;

namespace OpenSoftware.WebApiGenerator
{
    public class ServiceGeneratorOptions : OptionParser
    {
        public override string Name => "webapi";
        public override string Description => "Asp.Net Core on-the-fly Web service generator.";

        [Option(Name = "--urls", Description = "Semi-colon separated list of urls to listen to. E.g., http://0.0.0.0:2700")]
        public StringOption Urls { get; set; }

        [Option(Name = "--service", Description = "Assembly containing service implementation", IsRequired = true)]
        public StringOption ServiceAssembly { get; set; }

        [Option(Name = "--startup", Description = "Assembly containing Asp.Net Core startup class. If absent, a default is used")]
        public StringOption StartupAssembly { get; set; }

        [Option(Name="--dry", Description = "Print generated controller (C#) code to standard output, don't actually start the service itself.")]
        public BoolOption DryRun { get; set; }
    }
}