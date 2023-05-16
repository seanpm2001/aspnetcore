// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.ObjectPool;

internal sealed class DependencyInjectionPooledObjectPolicy<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDefinition> : IPooledObjectPolicy<TDefinition>
    where TDefinition : class
{
    private readonly IServiceProvider _provider;

    public DependencyInjectionPooledObjectPolicy(IServiceProvider provider)
    {
        _provider = provider;
    }

    public TDefinition Create() => _provider.GetRequiredService<TDefinition>();

    public bool Return(TDefinition obj)
    {
        if (obj is IResettable resettable)
        {
            return resettable.TryReset();
        }

        return true;
    }
}
