﻿using BenchmarkDotNet.Attributes;
using Runner.Setup;
using System.Collections.Generic;
using System.Linq;
using Unity;
using Unity.Extension;

namespace Runner.Tests
{
    [BenchmarkCategory("Basic")]
    [Config(typeof(BenchmarkConfiguration))]
    public class PreBuilt
    {
        IUnityContainer _container;
        object _syncRoot = new object();

        [IterationSetup]
        public virtual void SetupContainer()
        {
            _container = new UnityContainer();
            _container.AddExtension(new Diagnostic())
                      .Configure<Diagnostic>()
                      .DisableCompile();

            _container.RegisterType<Poco>();
            _container.RegisterType<IFoo, Foo>();
            _container.RegisterType<IFoo, Foo>("1");
            _container.RegisterType<IFoo>("2", Invoke.Factory(c => new Foo()));

            for (var i = 0; i < 3; i++)
            {
                _container.Resolve<object>();
                _container.Resolve<Poco>();
                _container.Resolve<IFoo>();
                _container.Resolve<IFoo>("1");
                _container.Resolve<IFoo>("2");
            }
        }

        [Benchmark(Description = "Resolve<IUnityContainer>            ")]
        public object IUnityContainer() => _container.Resolve(typeof(IUnityContainer), null);

        [Benchmark(Description = "Resolve<object> (pre-built)")]
        public object Unregistered() => _container.Resolve(typeof(object), null);

        [Benchmark(Description = "Resolve<Poco> (pre-built)")]
        public object Transient() => _container.Resolve(typeof(Poco), null);

        [Benchmark(Description = "Resolve<IService> (pre-built)")]
        public object Mapping() => _container.Resolve(typeof(IFoo), null);

        [Benchmark(Description = "Resolve<IService>   (factory)")]
        public object Factory() => _container.Resolve(typeof(IFoo), "2");

        [Benchmark(Description = "Resolve<IService[]> (pre-built)")]
        public object Array() => _container.Resolve(typeof(IFoo[]), null);

        [Benchmark(Description = "Resolve<IEnumerable<IService>> (pre-built)")]
        public object Enumerable() => _container.Resolve(typeof(IEnumerable<IFoo>), null);
    }
}