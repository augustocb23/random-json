using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using JsonStores;
using JsonStores.NamingStrategies;

namespace SampleIO
{
    internal static class Program
    {
        private const string FilePath = "out";

        private static int Main(string[] args)
        {
            var rootCommand = new RootCommand("Random data generator")
            {
                new Argument<int>("filesCount", "Number of files to generate."),
                new Argument<int>("itemsPerFile", () => 200, "Number of items per file.")
            };
            rootCommand.Handler = CommandHandler.Create((int filesCount, int itemsPerFile, bool verbose) =>
            {
                if (verbose) PrintMessage($"Creating {filesCount} files with {itemsPerFile} items...", ConsoleColor.Green);
                var fullFilePath = Path.Join(Environment.CurrentDirectory, FilePath);

                for (var fileIndex = 0; fileIndex < filesCount; fileIndex++)
                {
                    if (verbose) PrintMessage($"Generating data for file {fileIndex}...");
                    var items = new JsonRepository<FileItem, int>(new JsonStoreOptions
                    {
                        NamingStrategy = new StaticNamingStrategy(GetRandomString()),
                        Location = fullFilePath,
                        ThrowOnSavingChangedFile = false
                    });

                    for (var itemIndex = 0; itemIndex < itemsPerFile; itemIndex++)
                        items.AddAsync(new FileItem {Id = itemIndex, Data = GetRandomString()}).Wait();

                    if (verbose) PrintMessage($"Data generated. Saving file {fileIndex}...");
                    items.SaveChangesAsync().Wait();

                    if (verbose) PrintMessage($"File {fileIndex} saved.");
                }

                if (verbose) PrintMessage($"{filesCount} files created on {fullFilePath}.", ConsoleColor.Green);
                return 0;
            });

            return new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .AddGlobalOption(new Option<bool>("--verbose", "Show additional messages"))
                .Build().InvokeAsync(args).Result;
        }

        private static string GetRandomString() => Guid.NewGuid().ToString("N");

        private static void PrintMessage(string message, ConsoleColor color = default)
        {
            if (color != default) Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}