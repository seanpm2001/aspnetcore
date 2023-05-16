// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.ObjectPool;

/// <summary>
/// A pool of objects.
/// </summary>
/// <typeparam name="T">The type of objects to pool.</typeparam>
public interface IObjectPool<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> where T : class
{
    /// <summary>
    /// Gets an object from the pool if one is available, otherwise creates one.
    /// </summary>
    /// <returns>A <typeparamref name="T"/>.</returns>
    T Get();

    /// <summary>
    /// Return an object to the pool.
    /// </summary>
    /// <param name="obj">The object to add to the pool.</param>
    void Return(T obj);
}

