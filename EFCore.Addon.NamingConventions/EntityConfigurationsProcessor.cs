using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
	public class EntityConfigurationsProcessor
	{
		private static readonly Type ConfigurationInterfaceType;
		private static readonly MethodInfo ApplyGenericMethod;

		private readonly IReadOnlyCollection<ApplyMethodAndArguments> _applyMethodsAndArguments;

		static EntityConfigurationsProcessor()
		{
			ConfigurationInterfaceType = typeof(IEntityTypeConfiguration<>);
			ApplyGenericMethod = typeof(ModelBuilder).GetMethods()
				.Single(m =>
					m.IsGenericMethodDefinition
					&& m.Name == nameof(ModelBuilder.ApplyConfiguration)
					&& m.GetParameters().Length == 1
					&& m.GetParameters()[0].ParameterType.GetGenericTypeDefinition()
						== ConfigurationInterfaceType);
		}

		public EntityConfigurationsProcessor(string configurationsNamespace)
		{
			if (string.IsNullOrEmpty(configurationsNamespace))
				throw new ArgumentNullException(nameof(configurationsNamespace));

			_applyMethodsAndArguments = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && t.Namespace != null
						&& t.Namespace.StartsWith(configurationsNamespace))
				.Select(t => new
				{
					ConfigutationType = t,
					ConfigutationInterfaceType = t.GetInterfaces()
						.FirstOrDefault(it =>
							it.IsGenericType
							&& it.GetGenericTypeDefinition()
							== ConfigurationInterfaceType)
				})
				.Where(o => o.ConfigutationInterfaceType != null)
				.Select(o => new ApplyMethodAndArguments(
					ApplyGenericMethod.MakeGenericMethod(
						o.ConfigutationInterfaceType!.GetGenericArguments()[0]),
					new[] { Activator.CreateInstance(o.ConfigutationType) }))
				.ToArray();
		}

		public void ApplyConfigurations(ModelBuilder builder)
		{
			foreach (var applyMethodAndArguments in _applyMethodsAndArguments)
				applyMethodAndArguments.Method.Invoke(builder, applyMethodAndArguments.Arguments);
		}

		private readonly struct ApplyMethodAndArguments
		{
			public ApplyMethodAndArguments(MethodInfo method, object?[] arguments)
			{
				Method = method;
				Arguments = arguments;
			}

			public MethodInfo Method { get; }

			public object?[] Arguments { get; }
		}
	}
}
