// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding <see cref="IObjectPool{T}"/> to DI container.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds object pooling services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddObjectPools(this IServiceCollection services)
    {
        services.AddOptions();

        services.TryAdd(ServiceDescriptor.Singleton<IObjectPoolFactory, ObjectPoolFactory>());
        services.TryAdd(ServiceDescriptor.Singleton(typeof(IObjectPool<>), typeof(ObjectPoolImpl<>)));

        return services;
    }
}
