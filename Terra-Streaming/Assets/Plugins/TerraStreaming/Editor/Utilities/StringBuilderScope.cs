using System;
using System.Text;

namespace TerraStreaming.Utilities
{
	public struct StringBuilderScope : IDisposable
	{
		private static readonly StringBuilder BUILDER = new();

		public StringBuilder Builder => BUILDER;
		
		public void Dispose()
		{
			Builder.Clear();
		}
	}
}