using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using NUnit.Framework;

namespace Vertica.Integration.Tests.Infrastructure
{
	[TestFixture]
	public class ServicesInterceptorsConfigurationTester
	{
		[Test]
		public void InterceptService_Verify_Interactions()
		{
		    var stack = new Stack<object>();

            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Interceptors(interceptors => interceptors
                        .InterceptService<SomeService, SomeServiceInterceptor>())
                    .Advanced(advanced => advanced
                        .Register<SomeService>()
                        .Register<SomeOtherService>()
                        .Register(kernel => new ExecutionLog(stack))))))
            {
                var someOtherService = context.Resolve<SomeOtherService>();
                var someService = context.Resolve<SomeService>();

                Assert.That(stack.Count, Is.Zero);

                someOtherService.Execute();
                Assert.That(stack.Count, Is.EqualTo(1));
                Assert.That(stack.Pop(), Is.EqualTo(someOtherService));

                someService.Execute();
                Assert.That(stack.Count, Is.EqualTo(2));
                Assert.That(stack.Pop(), Is.EqualTo(someService));
                Assert.That(stack.Pop(), Is.TypeOf<SomeServiceInterceptor>());
            }
		}

        [Test]
        public void Intercept_CustomCondition_Verify_Interactions()
        {
            var stack = new Stack<object>();

            using (var context = ApplicationContext.Create(application => application
                .ConfigureForUnitTest()
                .Services(services => services
                    .Interceptors(interceptors => interceptors
                        .AddInterceptor<SomeServiceInterceptor>(model => model.Implementation.Namespace == GetType().Namespace))
                    .Advanced(advanced => advanced
                        .Register<SomeService>()
                        .Register<SomeOtherService>()
                        .Register(kernel => new ExecutionLog(stack))))))
            {
                var someOtherService = context.Resolve<SomeOtherService>();
                var someService = context.Resolve<SomeService>();

                Assert.That(stack.Count, Is.Zero);

                someOtherService.Execute();
                Assert.That(stack.Count, Is.EqualTo(2));
                Assert.That(stack.Pop(), Is.EqualTo(someOtherService));
                Assert.That(stack.Pop(), Is.TypeOf<SomeServiceInterceptor>());

                someService.Execute();
                Assert.That(stack.Count, Is.EqualTo(2));
                Assert.That(stack.Pop(), Is.EqualTo(someService));
                Assert.That(stack.Pop(), Is.TypeOf<SomeServiceInterceptor>());
            }
        }

        public class ExecutionLog
	    {
	        private readonly Stack<object> _stack;

	        public ExecutionLog(Stack<object> stack)
	        {
	            if (stack == null) throw new ArgumentNullException(nameof(stack));

	            _stack = stack;
	        }

	        public void Log(object instance)
	        {
	            if (instance == null) throw new ArgumentNullException(nameof(instance));

	            _stack.Push(instance);
	        }
	    }
        
	    public class SomeService
	    {
	        private readonly ExecutionLog _counter;

	        public SomeService(ExecutionLog counter)
	        {
	            _counter = counter;
	        }

	        public virtual void Execute()
	        {
	            _counter.Log(this);
	        }
	    }

        public class SomeOtherService
        {
            private readonly ExecutionLog _counter;

            public SomeOtherService(ExecutionLog counter)
            {
                _counter = counter;
            }

            public virtual void Execute()
            {
                _counter.Log(this);
            }
        }

        public class SomeServiceInterceptor : IInterceptor
	    {
	        private readonly ExecutionLog _counter;

            public SomeServiceInterceptor(ExecutionLog counter)
            {
                _counter = counter;
            }

            public void Intercept(IInvocation invocation)
            {
                _counter.Log(this);

	            invocation.Proceed();   
	        }
	    }
	}
}