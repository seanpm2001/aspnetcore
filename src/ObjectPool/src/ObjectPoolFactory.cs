// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.ObjectPool;

internal interface IObjectPoolFactory
{
    ObjectPool<T> Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IServiceProvider provider, PoolOptions options) where T : class;
}

internal sealed class ObjectPoolFactory : IObjectPoolFactory
{
    public ObjectPool<T> Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IServiceProvider provider, PoolOptions options)
        where T : class
    {
        var policy = new DependencyInjectionPooledObjectPolicy<T>(provider);

        if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
        {
            return new DisposableObjectPool<T>(policy, options.Capacity);
        }

        return new DefaultObjectPool<T>(policy, options.Capacity);
    }
}

