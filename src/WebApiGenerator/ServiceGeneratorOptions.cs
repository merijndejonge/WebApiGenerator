using System;
using Microsoft.Extensions.Logging;
using OpenSoftware.OptionParsing;

namespace OpenSoftware.WebApiGenerator
{
    public static class OptionsExtensions
    {
        public static void Process(this OptionParser options, string[] args, ILogger logger)
        {
            try
            {
                options.Parse(args);
            }
            catch (SyntaxErrorException e)
            {
                logger.LogError("Syntax error " + e.Message);
                Environment.Exit(1);
            }
            catch (InvalidOptionValueException e)
            {
                logger.LogError("Invalid options " + e.Message);
                Environment.Exit(1);
            }
            if (options.Usage.IsDefined)
            {
                options.DisplayUsage(Console.Error);
                Environment.Exit(0);
            }
        }
    }
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