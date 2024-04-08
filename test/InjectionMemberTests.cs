﻿using Stashbox.Attributes;
using Stashbox.Exceptions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Stashbox.Tests;

public class InjectionMemberTests
{
    [Fact]
    public void InjectionMemberTests_Resolve()
    {
        using var container = new StashboxContainer();
        container.Register<ITest, Test>();
        container.Register<ITest1, Test1>();

        var inst = container.Resolve<ITest1>();

        Assert.NotNull(inst);
        Assert.NotNull(inst.Test);
        Assert.IsType<Test1>(inst);
        Assert.IsType<Test>(inst.Test);

        Assert.NotNull(((Test1)inst).TestFieldProperty);
        Assert.IsType<Test>(((Test1)inst).TestFieldProperty);
    }

    [Fact]
    public void InjectionMemberTests_Resolve_WithoutRegistered()
    {
        using var container = new StashboxContainer();
        var test1 = new Test1();
        container.WireUp<ITest1>(test1);

        Assert.Throws<ResolutionFailedException>(() => container.Resolve<ITest1>());
    }

    [Fact]
    public void InjectionMemberTests_WireUp()
    {
        using var container = new StashboxContainer();
        container.Register<ITest, Test>();

        var test1 = new Test1();
        container.WireUp<ITest1>(test1);

        var inst = container.Resolve<ITest1>();

        Assert.NotNull(inst);
        Assert.NotNull(inst.Test);
        Assert.IsType<Test1>(inst);
        Assert.IsType<Test>(inst.Test);

        Assert.NotNull(((Test1)inst).TestFieldProperty);
        Assert.IsType<Test>(((Test1)inst).TestFieldProperty);
    }

    [Fact]
    public void InjectionMemberTests_Resolve_InjectionParameter()
    {
        var container = new StashboxContainer();
        container.Register<ITest2, Test2>(context =>
            context.WithInjectionParameters(new KeyValuePair<string, object>("Name", "test")));

        var inst = container.Resolve<ITest2>();

        Assert.NotNull(inst);
        Assert.IsType<Test2>(inst);
        Assert.Equal("test", inst.Name);
    }

    [Fact]
    public void InjectionMemberTests_Resolve_InjectionParameter_WithNull()
    {
        var container = new StashboxContainer(c => c.WithDefaultValueInjection());
        container.Register<ITest2, Test2>();

        var inst = container.Resolve<ITest2>();

        Assert.NotNull(inst);
        Assert.IsType<Test2>(inst);
        Assert.Null(inst.Name);
    }

    [Fact]
    public void InjectionMemberTests_Inject_With_Config()
    {
        var container = new StashboxContainer();
        container.Register<Test3>(context => context.WithDependencyBinding("Test1", "test1").WithDependencyBinding("test2", "test2"))
            .Register<ITest, TestM1>(context => context.WithName("test1"))
            .Register<ITest, TestM2>(context => context.WithName("test2"));

        var inst = container.Resolve<Test3>();

        Assert.NotNull(inst.Test1);
        Assert.NotNull(inst.Test2);
        Assert.IsType<TestM1>(inst.Test1);
        Assert.IsType<TestM2>(inst.Test2);
    }

    [Fact]
    public void InjectionMemberTests_Inject_With_Invalid_Config()
    {
        var container = new StashboxContainer();
        container.Register<Test3>(context => context.WithDependencyBinding("Test3"));

        var inst = container.Resolve<Test3>();

        Assert.Null(inst.Test1);
        Assert.Null(inst.Test2);
    }

    [Fact]
    public void InjectionMemberTests_Inject_With_Config_Generic()
    {
        var container = new StashboxContainer();
        container.Register<Test3>(context => context.WithDependencyBinding(x => x.Test1, "test2"))
            .Register<ITest, TestM2>(context => context.WithName("test2"));

        var inst = container.Resolve<Test3>();

        Assert.NotNull(inst.Test1);
        Assert.Null(inst.Test2);
        Assert.IsType<TestM2>(inst.Test1);
    }

    [Fact]
    public void InjectionMemberTests_Inject_With_Config_Generic_Throws()
    {
        using var container = new StashboxContainer();
        Assert.Throws<ArgumentException>(() => container.Register<Test3>(context => context.WithDependencyBinding(x => 50)));
    }

    [Fact]
    public void InjectionMemberTests_Exclude_Globally()
    {
        var inst = new StashboxContainer(config =>
                config.WithUnknownTypeResolution()
                    .WithAutoMemberInjection(filter: info => info.Name != "Test4"))
            .Activate<Test6>();

        Assert.Null(inst.Test4);
        Assert.NotNull(inst.Test5);
    }

    [Fact]
    public void InjectionMemberTests_Exclude_PerReg()
    {
        var inst = new StashboxContainer(config =>
                config.WithUnknownTypeResolution())
            .Register<Test6>(config => config.WithAutoMemberInjection(filter: info => info.Name != "Test4"))
            .Resolve<Test6>();

        Assert.Null(inst.Test4);
        Assert.NotNull(inst.Test5);
    }

    [Fact]
    public void InjectionMemberTests_Throws_Field()
    {
        using var container = new StashboxContainer()
            .Register<Test7>();

        Assert.Throws<ResolutionFailedException>(() => container.Resolve<Test7>());
    }
    
#if HAS_REQUIRED
    [Fact]
    public void InjectionMemberTests_AutoInject_Required()
    {
        using var container = new StashboxContainer()
            .Register<Test8>()
            .Register<Test5>()
            .Register<Test4>();

        var inst = container.Resolve<Test8>();
        
        Assert.NotNull(inst.Test4);
        Assert.NotNull(inst.Test5);
    }
    
    [Fact]
    public void InjectionMemberTests_AutoInject_Required_Disabled_Global()
    {
        using var container = new StashboxContainer(c => c.WithRequiredMemberInjection(false))
            .Register<Test8>()
            .Register<Test5>()
            .Register<Test4>();

        var inst = container.Resolve<Test8>();
        
        Assert.Null(inst.Test4);
        Assert.Null(inst.Test5);
    }
    
    [Fact]
    public void InjectionMemberTests_AutoInject_Required_Disabled_Reg()
    {
        using var container = new StashboxContainer()
            .Register<Test8>(c => c.WithRequiredMemberInjection(false))
            .Register<Test5>()
            .Register<Test4>();

        var inst = container.Resolve<Test8>();
        
        Assert.Null(inst.Test4);
        Assert.Null(inst.Test5);
    }
    
    [Fact]
    public void InjectionMemberTests_AutoInject_Required_Disabled_Global_Enabled_Reg()
    {
        using var container = new StashboxContainer(c => c.WithRequiredMemberInjection(false))
            .Register<Test8>(c => c.WithRequiredMemberInjection())
            .Register<Test5>()
            .Register<Test4>();

        var inst = container.Resolve<Test8>();
        
        Assert.NotNull(inst.Test4);
        Assert.NotNull(inst.Test5);
    }
#endif

    interface ITest;

    interface ITest1 { ITest Test { get; } }

    class Test : ITest;

    class TestM1 : ITest;

    class TestM2 : ITest;

    class Test1 : ITest1
    {
        [Dependency]
        private ITest testField = null;

        public ITest TestFieldProperty => this.testField;

        [Dependency]
        public ITest Test { get; set; }
    }

    interface ITest2 { string Name { get; set; } }

    class Test2 : ITest2
    {
        [Dependency]
        public string Name { get; set; }
    }

    class Test3
    {
        public ITest Test1 { get; set; }

        private ITest test2 = null;

        public ITest Test2 => this.test2;

        public void Test() { }
    }

    class Test4;

    class Test5;

    class Test6
    {
        public Test4 Test4 { get; set; }

        public Test5 Test5 { get; set; }
    }

    class Test7
    {
        [Dependency]
#pragma warning disable 169
        private Test4 test4;
#pragma warning restore 169
    }
#if HAS_REQUIRED
    class Test8
    {
        public required Test4 Test4 { get; init; }
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        public required Test5 Test5;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    }    
#endif
}