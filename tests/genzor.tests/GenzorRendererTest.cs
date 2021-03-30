using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Genzor.Components;
using Genzor.FileSystem;
using Genzor.TestGenerators;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Genzor
{
	public class GenzorRendererTest : GenzorTestBase
	{
		public GenzorRenderer SUT => Services.GetRequiredService<GenzorRenderer>();

		public IFileSystem FileSystem => Services.GetRequiredService<IFileSystem>();

		public GenzorRendererTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

		[Fact(DisplayName = "when invoking a generator which throws an exception, " +
							"then the exception is re-thrown to caller")]
		public void Test102()
		{
			Func<Task> throwingAction = () => SUT.InvokeGeneratorAsync<ThrowingGenereator>();

			throwingAction
				.Should()
				.Throw<ThrowingGenereator.ThrowingGenereatorException>();
		}

		[Fact(DisplayName = "given generator that creates a file, " +
							"when invoking generator, " +
							"then a generated file is added to file system")]
		public async Task Test001()
		{
			await SUT.InvokeGeneratorAsync<StaticFileGenerator>();

			FileSystem
				.Should()
				.ContainSingleTextFile()
				.WithName(StaticFileGenerator.NameText)
				.And
				.WithContent(StaticFileGenerator.ContentText);
		}

		[AutoData]
		[Theory(DisplayName = "given generator that takes parameters, " +
							  "when invoking generator with parameters, " +
							  "then parameters are passed to generator")]
		public async Task Test011(string filename, string content)
		{
			await SUT.InvokeGeneratorAsync<GenericFileGenerator>(
				CreateParametersView(
					("Name", filename),
					("ChildContent", content)));

			FileSystem
				.Should()
				.ContainSingleTextFile()
				.WithName(filename)
				.And
				.WithContent(content);
		}

		[Fact(DisplayName = "given file generator which has a directory as its content, " +
							"when invoking generator, " +
							"then an InvalidGeneratorComponentContentException is thrown")]
		public async Task Test012()
		{
			Func<Task> throwingAction = () => SUT.InvokeGeneratorAsync<FileWithDirectoryGenerator>();

			await throwingAction
				.Should()
				.ThrowAsync<InvalidGeneratorComponentContentException>()
				.WithMessage($"A directory component ({nameof(IDirectoryComponent)}) cannot be the child of a file component ({nameof(IFileComponent)}). Name of misplaced directory: {FileWithDirectoryGenerator.DirectoryName}");
		}

		[Fact(DisplayName = "given file generator with child components as its content, " +
							"when invoking generator, " +
							"then content from child components are part of generated files content")]
		public async Task Test013()
		{
			await SUT.InvokeGeneratorAsync<StaticFileWithChildComponentGenerator>();

			FileSystem
				.Should()
				.ContainSingleTextFile()
				.WithContent(StaticFileWithChildComponentGenerator.ChildComponentText);
		}

		[Fact(DisplayName = "given file generator with multiple nested child components as its content, " +
							"when invoking generator, " +
							"then content from all child components are part of generated files content")]
		public async Task Test014()
		{
			var expectedContent = $"{StaticFileWithMultipleNestedChildComponentsGenerator.Child1ComponentText}" +
								  $"{StaticFileWithMultipleNestedChildComponentsGenerator.Child2ComponentText}";

			await SUT.InvokeGeneratorAsync<StaticFileWithMultipleNestedChildComponentsGenerator>();

			FileSystem
				.Should()
				.ContainSingleTextFile()
				.WithContent(expectedContent);
		}

		[Fact(DisplayName = "given generator with multiple levels of generic component wrapping a file component, " +
							"when invoking generator, " +
							"then wrapped file component is added to file system")]
		public async Task Test015()
		{
			await SUT.InvokeGeneratorAsync<StaticWithMultipleNestedComponentsWrappingFileGenerator>();

			FileSystem
				.Should()
				.ContainSingleTextFile()
				.WithName(StaticWithMultipleNestedComponentsWrappingFileGenerator.NestedFileName);
		}

		[Fact(DisplayName = "given generator that creates multiple file, " +
							"when invoking generator, " +
							"then generated files is added to file system in generated order")]
		public async Task Test021()
		{
			await SUT.InvokeGeneratorAsync<TwoFileGenerator>();

			FileSystem
				.Should()
				.HaveTextFiles(2)
				.Which
				.Should()
				.BeEquivalentTo(new[]
				{
					new { Name = TwoFileGenerator.FirstFilesName },
					new { Name = TwoFileGenerator.SecondFilesName },
				});
		}

		[Fact(DisplayName = "given generator that creates a directory, " +
							"when invoking generator, " +
							"then generated directory is added to file system")]
		public async Task Test031()
		{
			await SUT.InvokeGeneratorAsync<StaticDirectoryGenerator>();

			FileSystem
				.Should()
				.ContainSingleDirectory()
				.WithName(StaticDirectoryGenerator.NameText);
		}

		[Fact(DisplayName = "given generator that creates multiple directories, " +
							"when invoking generator, " +
							"then generated directories is added to file system")]
		public async Task Test032()
		{
			await SUT.InvokeGeneratorAsync<TwoDirectoryGenerator>();

			FileSystem
				.Should()
				.HaveDirectories(2)
				.Which
				.Should()
				.BeEquivalentTo(new[]
				{
					new { Name = TwoDirectoryGenerator.FirstDirectoryName },
					new { Name = TwoDirectoryGenerator.SecondDirectoryName },
				});
		}

		[Fact(DisplayName = "given directory generator that has file generator as its child, " +
							"when invoking generator, " +
							"then the generated file is added to the generated directory")]
		public async Task Test033()
		{
			await SUT.InvokeGeneratorAsync<DirectoryWithFileGenerator>();

			FileSystem
				.Should()
				.ContainSingleDirectory()
				.Which
				.Should()
				.ContainSingleTextFile()
				.WithName(DirectoryWithFileGenerator.ChildFileName);
		}

		//[Fact(DisplayName = "given directory generator which none file system component as child content, " +
		//					"when invoking generator, " +
		//					"then an InvalidGeneratorComponentContentException is thrown")]
		//public async Task Test033()
		//{
		//	Func<Task> throwingAction = () => SUT.InvokeGeneratorAsync<DirectoryWithNoneFileSystemComponentGenerator>();

		//	await throwingAction
		//		.Should()
		//		.ThrowAsync<InvalidGeneratorComponentContentException>()
		//		.WithMessage($"A directory component ({nameof(IDirectoryComponent)}) can only have other directory components or file components ({nameof(IFileComponent)}) as its children. Type of misplaced component: {DirectoryWithNoneFileSystemComponentGenerator.ChildComponent.FullName}");
		//}

		// TODO: throw exception on invalid file or directory names (verify using Path.GetInvalidFileNameChars())
	}
}
