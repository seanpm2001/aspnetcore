// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ObjectPool.TestResources;
using Xunit;

namespace Microsoft.Extensions.ObjectPool;

public class DependencyInjectionExtensionsTest
{
    [Fact]
    public void ConfiguresPoolOptions()
    {
        var services = new ServiceCollection()
            .Configure<PoolOptions>(typeof(TestClass).FullName, options => options.Capacity = 2048)
            .Configure<PoolOptions>(typeof(TestDependency).FullName, options => options.Capacity = 4096)
            ;
        using var provider = services.BuildServiceProvider();

        var sut = provider.GetRequiredService<IOptionsMonitor<PoolOptions>>();

        Assert.Equal(2048, sut.Get(typeof(TestClass).FullName!).Capacity);
        Assert.Equal(4096, sut.Get(typeof(TestDependency).FullName!).Capacity);
    }

    [Fact]
    public void AddPool_ServiceTypeOnly_AddsPool()
    {
        var services = new ServiceCollection().AddObjectPools();

        var sut = services.BuildServiceProvider().GetService<IObjectPool<TestDependency>>();
        using var provider = services.BuildServiceProvider();
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<PoolOptions>>();

        Assert.NotNull(sut);
        Assert.Equal(1024, optionsMonitor.Get(typeof(TestDependency).FullName).Capacity);
    }

    [Fact]
    public void AddPool_ServiceTypeOnlyWithCapacity_AddsPoolAndSetsCapacity()
    {
        var services = new ServiceCollection()
            .AddObjectPools()
            .Configure<PoolOptions>(typeof(TestDependency).FullName, options => options.Capacity = 64)
            ;

        var sut = services.BuildServiceProvider().GetService<IObjectPool<TestDependency>>();
        using var provider = services.BuildServiceProvider();
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<PoolOptions>>();

        Assert.NotNull(sut);
        Assert.Equal(64, optionsMonitor.Get(typeof(TestDependency).FullName).Capacity);
    }

    [Fact]
    public void AddPool_ServiceAndImplementationType_AddsPool()
    {
        var services = new ServiceCollection()
            .AddSingleton<TestDependency>()
            .AddScoped<ITestClass, TestClass>()
            .AddObjectPools();

        using var provider = services.BuildServiceProvider();
        var sut = provider.GetService<IObjectPool<ITestClass>>();
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<PoolOptions>>();

        Assert.NotNull(sut);
        Assert.Equal(TestDependency.Message, sut!.Get().ReadMessage());
        Assert.Equal(1024, optionsMonitor.Get(typeof(ITestClass).FullName).Capacity);
    }

    [Fact]
    public void AddPool_ServiceAndImplementationTypeWithCapacity_AddsPoolAndSetsCapacity()
    {
        var services = new ServiceCollection()
            .AddSingleton<TestDependency>()
            .AddScoped<ITestClass, TestClass>()
            .AddObjectPools().Configure<PoolOptions>(typeof(ITestClass).FullName, options => options.Capacity = 64);

        using var provider = services.BuildServiceProvider();
        var sut = provider.GetService<IObjectPool<ITestClass>>();
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<PoolOptions>>();

        Assert.NotNull(sut);
        Assert.Equal(TestDependency.Message, sut!.Get().ReadMessage());
        Assert.Equal(64, optionsMonitor.Get(typeof(ITestClass).FullName).Capacity);
    }

    [Fact]
    public void AddPool_ReturnedPooled_CallsTryReset()
    {
        var services = new ServiceCollection()
            .AddSingleton<TestDependency>()
            .AddScoped<ITestClass, TestClass>()
            .AddObjectPools();

        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<IObjectPool<ITestClass>>();

        var pooled = sut.Get();
        sut.Return(pooled);

        Assert.Equal(1, pooled.ResetCalled);
    }

    [Fact]
    public void AddPool_SingletonInstances_NotDisposed()
    {
        var services = new ServiceCollection()
            .AddSingleton<TestDependency>()
            .AddSingleton<ITestClass, TestClass>()
            .AddObjectPools()
            ;

        using var provider = services.BuildServiceProvider();

        ITestClass resolved;

        using (var scope = provider.CreateScope())
        {
            resolved = scope.ServiceProvider.GetRequiredService<IObjectPool<ITestClass>>().Get();
        }

        Assert.NotNull(resolved);
        Assert.Equal(0, resolved.DisposedCalled);
    }

    [Fact]
    public void PooledHelper_ScopedInstance_SamScope()
    {
        var services = new ServiceCollection()
            .AddSingleton<TestDependency>()
            .AddObjectPools()
            .AddScoped<TestClass>()
            .AddTransient<Pooled<TestClass>>()
            .AddScoped<ITestClass>(provider =>
            {
                var pooled = provider.GetRequiredService<Pooled<TestClass>>();
                return pooled.Object;
            });
        ;

        using var provider = services.BuildServiceProvider();

        ITestClass resolved1, resolved2;

        // Because these are scoped, resolved1 and resolved2 are the same instance
        // and a single reference is disposed.

        using (var scope = provider.CreateScope())
        {
            resolved1 = scope.ServiceProvider.GetRequiredService<ITestClass>();
            resolved2 = scope.ServiceProvider.GetRequiredService<ITestClass>();
        }

        Assert.NotNull(resolved1);
        Assert.NotNull(resolved2);
        Assert.True(resolved1 == resolved2);

        Assert.Equal(1, resolved1.DisposedCalled);
        Assert.Equal(1, resolved1.ResetCalled);
    }

    [Fact]
    public void PooledHelper_ScopedInstance_DifferentScopes()
    {
        // This is an example of what Pooled<T> could look like

        var services = new ServiceCollection()
            .AddSingleton<TestDependency>()
            .AddObjectPools()
            .AddScoped<TestClass>()
            .AddTransient<Pooled<TestClass>>()
            .AddScoped<ITestClass>(provider => provider.GetRequiredService<Pooled<TestClass>>().Object);

        using var provider = services.BuildServiceProvider();

        ITestClass resolved1, resolved2;

        using (var scope = provider.CreateScope())
        {
            resolved1 = scope.ServiceProvider.GetRequiredService<ITestClass>();
        }

        // The object should be returned and recalled in the next scope

        using (var scope = provider.CreateScope())
        {
            resolved2 = scope.ServiceProvider.GetRequiredService<ITestClass>();
        }

        Assert.NotNull(resolved1);
        Assert.NotNull(resolved2);
        Assert.Same(resolved1, resolved2);

        Assert.Equal(2, resolved1.DisposedCalled);
        Assert.Equal(2, resolved1.ResetCalled);
    }
}
