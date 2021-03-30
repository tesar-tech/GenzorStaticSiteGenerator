using System.Collections.Generic;

namespace Genzor.FileSystem
{
	public interface IFileSystem
	{
		/// <summary>
		/// Gets the root structure of the file system, which can contain zero or more <see cref="IFileSystemItem"/>.
		/// </summary>
		IReadOnlyList<IFileSystemItem> Root { get; }

		/// <summary>
		/// Adds the <paramref name="item"/> to the <see cref="Root"/> of the file system.
		/// </summary>
		/// <param name="item">Item to add to the file system.</param>
		void AddItem(IFileSystemItem item);
	}
}