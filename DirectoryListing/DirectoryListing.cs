using CliWrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DirectoryListing
{
    internal class DirectoryListing
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args).Build();

            var configuration = host.Services.GetService(typeof(IConfiguration)) as IConfiguration;

            var directoriesToList = configuration?.GetSection("directoriesToList").Get<Dictionary<string, string>>();

            if (directoriesToList != null)
            {
                var volumeLabelToDrive = DriveInfo
                    .GetDrives()
                    .Where(d => d.IsReady && !string.IsNullOrWhiteSpace(d.VolumeLabel))
                    .GroupBy(d => d.VolumeLabel)
                    .ToDictionary(d => d.Key, d => d.First().Name);

                var dateTime = DateTime.Now.ToString("yyyy-MM-dd");

                foreach (var directoryKvp in directoriesToList)
                {
                    var name = directoryKvp.Key;
                    var directory = directoryKvp.Value;

                    if (!Directory.Exists(directory)
                        && !volumeLabelToDrive.TryGetValue(directory, out directory))
                    {
                        Console.WriteLine($"{directory} does not exist");

                        continue;
                    }

                    Console.WriteLine($"Getting listing for {directory} ({name})...");

                    var _ = await Cli.Wrap("cmd")
                                    .WithArguments("/C dir /s")
                                    .WithWorkingDirectory(directory)
                                    .WithStandardOutputPipe(PipeTarget.ToFile($"{name}-{dateTime}.txt"))
                                    .ExecuteAsync();
                }
            }

            Console.WriteLine("Done! Press the enter key to exit.");
            Console.ReadLine();
        }
    }
}