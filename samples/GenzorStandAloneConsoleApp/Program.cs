using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Genzor;
using GenzorStandAloneConsoleApp;
using GenzorStandAloneConsoleApp.Generators;
using Microsoft.Extensions.Logging;

namespace GenzorDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var fileSystem = new FileSystem(new DirectoryInfo(Directory.GetCurrentDirectory()));
            
            using var host = new GenzorHost()
                .AddLogging(configure => configure
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Debug))
                .AddFileSystem(fileSystem);

			var mdFiles = Directory.GetFiles(Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).FullName, "MarkdownInput"));

			List<Document> mdDocuments = new List<Document>();
			foreach (var pathToFile in mdFiles)
			{
				mdDocuments.Add(new Document() { FilenameWithoutExtension = Path.GetFileNameWithoutExtension(pathToFile),
					Content = File.ReadAllText(pathToFile)
				});
			}

			ParameterViewBuilder<HelloWorldGenerator> builder = new ParameterViewBuilder<HelloWorldGenerator>() ;
			builder.Add((ss) => ss.MyProperty, 222);
			builder.Add((ss) => ss.MdDocuments, mdDocuments);

			await host.InvokeGeneratorAsync<HelloWorldGenerator>(builder.Build());
        }
    }



}
