﻿using System.Threading.Tasks;
using Xunit;

namespace Stashbox.Tests.IssueTests
{

    public class ScopedLifetimeThreadSafeTests
    {
        [Fact]
        public void ScopedLifetime_thread_safety()
        {
            using IStashboxContainer container = new StashboxContainer();
            container.RegisterScoped<Test>();
            for (var i = 0; i < 1000; i++)
            {
                using var scope = container.BeginScope();
                Parallel.For(0, 50, _ => 
                {
                    var inst = scope.Resolve<Test>();
                    Assert.Same(inst, scope.Resolve<Test>());
                });
            }
        }

        [Fact]
        public void ScopedLifetime_thread_safety_generic()
        {
            using IStashboxContainer container = new StashboxContainer();
            container.Register(typeof(TestG<>), c => c.WithScopedLifetime());
            for (var i = 0; i < 1000; i++)
            {
                using var scope = container.BeginScope();
                Parallel.For(0, 50, _ =>
                {
                    var inst = scope.Resolve<TestG<int>>();
                    Assert.Same(inst, scope.Resolve<TestG<int>>());
                });
            }
        }

        class Test { }

        class TestG<T> { }
    }
}
