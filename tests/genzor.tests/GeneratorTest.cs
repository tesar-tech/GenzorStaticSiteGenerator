using System;
using System.Threading.Tasks;
using FluentAssertions;
using Genzor.FileSystem;
using Genzor.TestGenerators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Genzor
{
	public class GeneratorTest
	{
		private IServiceProvider Services { get; }

		public GeneratorTest(ITestOutputHelper outputHelper)
		{
			var services = new ServiceCollection();
			services.AddLogging((builder) => builder.AddXUnit(outputHelper).SetMinimumLevel(LogLevel.Debug));
			services.AddSingleton<FakeFileSystem>();
			services.AddSingleton<IFileSystem>(s => s.GetRequiredService<FakeFileSystem>());
			services.AddSingleton<Generator>();

			Services = services.BuildServiceProvider();
		}

		private Generator CreateSut() => Services.GetRequiredService<Generator>();

		[Fact(DisplayName = "when invoking generator with HelloWorldGenerator, " +
							"then a HelloWorld.txt file is added to file system")]
		public async Task Test001()
		{
			var fileSystem = Services.GetRequiredService<IFileSystem>();

			var sut = CreateSut();

			await sut.InvokeGeneratorAsync<HelloWorldGenerator>();

			fileSystem.Root
				.Should()
				.ContainSingle()
				.Subject
				.Should()
				.BeAssignableTo<IFile>()
				.Subject
				.Name
				.Should()
				.Be("HelloWorld.txt");
		}

		[Fact(DisplayName = "when invoking generator that throws exception, " +
							"then the exception is re-thrown to caller")]
		public void Test002()
		{
			var sut = CreateSut();

			Func<Task> throwingAction = () => sut.InvokeGeneratorAsync<ThrowingGenereator>();

			throwingAction
				.Should()
				.Throw<ThrowingGenereator.ThrowingGenereatorException>();
		}

		// passing parameters to generators
	}
}
