﻿using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace DTasks.Extensions.DependencyInjection;

public partial class DTasksServiceCollectionExtensionsTests
{
    private static Expression<Func<ServiceDescriptor, bool>> Singleton<TService>()
    {
        return descriptor =>
            descriptor.ServiceType == typeof(TService) &&
            descriptor.Lifetime == ServiceLifetime.Singleton;
    }

    private static Expression<Func<ServiceDescriptor, bool>> Scoped<TService>()
    {
        return descriptor =>
            descriptor.ServiceType == typeof(TService) &&
            descriptor.Lifetime == ServiceLifetime.Scoped;
    }
}
