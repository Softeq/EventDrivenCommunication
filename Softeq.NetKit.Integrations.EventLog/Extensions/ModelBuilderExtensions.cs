// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Softeq.NetKit.Integrations.EventLog.Abstract;

namespace Softeq.NetKit.Integrations.EventLog.Extensions
{
	internal static class ModelBuilderExtensions
	{
		private static IEnumerable<Type> GetBuilderTypes(this Assembly assembly, Type builderInterface)
		{
			return assembly.GetTypes()
				.Where(x => !x.IsAbstract && x.GetInterfaces().Any(y => y == builderInterface));
		}

		public static void AddEntityConfigurationsFromAssembly<T>(this ModelBuilder modelBuilder, Assembly assembly)
		{
			var builderTypes = assembly.GetBuilderTypes(typeof(T));
			foreach (var config in builderTypes.Select(Activator.CreateInstance).Cast<IDomainModelBuilder>())
			{
				config.Build(modelBuilder);
			}
		}
	}
}
