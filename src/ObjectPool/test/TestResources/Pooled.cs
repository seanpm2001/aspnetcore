// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.ObjectPool.TestResources;

public class Pooled<TDefinition> : IDisposable
    where TDefinition : class
{
    private readonly ObjectPool<TDefinition> _pool;

    public TDefinition Object { get; }

    public Pooled(ObjectPool<TDefinition> pool)
    {
        _pool = pool;
        Object = pool.Get();
    }

    public void Dispose()
    {
        _pool.Return(Object);
    }
}
