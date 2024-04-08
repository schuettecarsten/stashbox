﻿using Stashbox.Configuration;
using System;
using Xunit;

namespace Stashbox.Tests.IssueTests;

public class ResolvingFunUseWrongConstructor
{
    [Fact]
    public void Ensure_Good_Constructor_Selected()
    {
        var container = new StashboxContainer();
        container.RegisterAssemblyContaining<ClassA>();

        var funcA = container.Resolve<Func<ClassA, InjectedClass>>();
        var classA = container.Resolve<ClassA>();
        var injectedA = funcA(classA);

        var funcB = container.Resolve<Func<ClassB, InjectedClass>>();
        var classB = container.Resolve<ClassB>();
        var injectedB = funcB(classB);

        Assert.Equal(classA, injectedA.ClassA);
        Assert.Equal(classB, injectedB.ClassB);
    }

    class ClassA;

    class ClassB;

    class InjectedClass
    {
        public ClassA ClassA;
        public ClassB ClassB;

        public InjectedClass(ClassA classA)
        {
            ClassA = classA;
        }

        public InjectedClass(ClassB classB)
        {
            ClassB = classB;
        }
    }

    [Fact]
    public void Ensure_Good_Constructor_Selected_Deeper_In_The_Tree()
    {
        var container = new StashboxContainer();
        container.Register<A>().Register<B>().Register<Subject1>().Register<Subject2>();

        var fa = container.Resolve<Func<A, Subject2>>();
        var a = container.Resolve<A>();
        var instA = fa(a);

        var fb = container.Resolve<Func<B, Subject2>>();
        var b = container.Resolve<B>();
        var instB = fb(b);

        Assert.Equal(a, instA.Subject1.A);
        Assert.Equal(b, instB.Subject1.B);
    }

    [Fact]
    public void Ensure_Constructor_Most_Params_Selector_Respects_Func_Param()
    {
        var container = new StashboxContainer();
        container.Register<A>().Register<B>().Register<Subject3>();

        var fb = container.Resolve<Func<B, Subject3>>();
        var b = container.Resolve<B>();
        var instB = fb(b);

        Assert.Equal(b, instB.B);
        Assert.NotNull(instB.A);
    }

    [Fact]
    public void Ensure_Constructor_Least_Params_Selector_Respects_Func_Param()
    {
        var container = new StashboxContainer();
        container.Register<A>().Register<B>().Register<Subject3>(c => c.WithConstructorSelectionRule(Rules.ConstructorSelection.PreferLeastParameters));

        var fb = container.Resolve<Func<B, Subject3>>();
        var b = container.Resolve<B>();
        var instB = fb(b);

        Assert.Equal(b, instB.B);
        Assert.Null(instB.A);
    }

    class A;

    class B;

    class Subject1
    {
        public Subject1(A a)
        {
            A = a;
        }

        public Subject1(B b)
        {
            B = b;
        }

        public A A { get; }
        public B B { get; }
    }

    class Subject2
    {
        public Subject2(Subject1 subject1)
        {
            Subject1 = subject1;
        }

        public Subject1 Subject1 { get; }
    }

    class Subject3
    {
        public Subject3(A a)
        {
            A = a;
        }

        public Subject3(B b)
        {
            B = b;
        }

        public Subject3(B b, A a)
        {
            B = b;
            A = a;
        }

        public B B { get; }
        public A A { get; }
    }
}