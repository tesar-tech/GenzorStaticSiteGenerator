using Microsoft.AspNetCore.Components;

namespace Genzor.Components
{
	public interface IFileComponent : IComponent
	{
		/// <summary>
		/// Gets the name of the file the component renders.
		/// </summary>
		string Name { get; }
	}
}
