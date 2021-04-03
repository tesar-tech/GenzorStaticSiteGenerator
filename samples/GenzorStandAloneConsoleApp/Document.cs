using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenzorStandAloneConsoleApp
{
	public class Document
	{
		public string OutputFileName => $"{FilenameWithoutExtension.Replace(" ", "_")}.html";
		public string Content { get; set; } = "";
		public string FilenameWithoutExtension { get; set; } = "";
	}
}
