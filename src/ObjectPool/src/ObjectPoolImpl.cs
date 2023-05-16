// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.ObjectPool;

/// <summary>
/// A pool of objects.
/// </summary>
/// <typeparam name="T">The type of objects to pool.</typeparam>
internal sealed class ObjectPoolImpl<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> : IObjectPool<T> where T : class
{
    private readonly ObjectPool<T> _objectPool;

    public ObjectPoolImpl(IObjectPoolFactory factory, IServiceProvider provider, IOptions<PoolOptions> poolOptions)
    {
        _objectPool = factory.Create<T>(provider, poolOptions.Value);
    }

    public T Get() => _objectPool.Get();

    public void Return(T obj) => _objectPool.Return(obj);
}
