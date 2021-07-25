using Microsoft.EntityFrameworkCore.Infrastructure;

using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
	public static class DbContextOptionsBuilderExtensions
	{
		private static readonly IdColumnNameAtBeginning ExtensionInstance = new();

		public static DbContextOptionsBuilder UseIdColumnNameAtBeginning(
			this DbContextOptionsBuilder optionsBuilder)
		{
			if (optionsBuilder == null)
				throw new ArgumentNullException(nameof(optionsBuilder));

			((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
				.AddOrUpdateExtension(ExtensionInstance);
			return optionsBuilder;
		}
	}
}
