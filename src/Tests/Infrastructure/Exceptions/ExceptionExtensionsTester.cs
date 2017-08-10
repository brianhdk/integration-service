using System;
using NUnit.Framework;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Tests.Infrastructure.Exceptions
{
    [TestFixture]
    public class ExceptionExtensionsTester
    {
        [Test]
        public void DestructMessage_DifferentException_ActualExceptionMessage()
        {
            var invalidOperationException = new InvalidOperationException("InvalidOperation");

            string actual = invalidOperationException.DestructMessage();

            Assert.AreEqual(invalidOperationException.Message, actual);
        }

        [Test]
        public void DestructMessage_AggregateExceptionWithZeroInnerExceptions_ActualExceptionMessage()
        {
            var aggregateException = new AggregateException();

            string actual = aggregateException.DestructMessage();

            Assert.AreEqual(aggregateException.Message, actual);
        }

        [Test]
        public void DestructMessage_AggregateExceptionWithOneInnerException_ContainsMessages()
        {
            var divideByZeroException = new DivideByZeroException("DivideByZero");

            var aggregateException = new AggregateException(
                divideByZeroException);

            string actual = aggregateException.DestructMessage();

            StringAssert.Contains(aggregateException.Message, actual);
            StringAssert.Contains(divideByZeroException.Message, actual);
        }

        [Test]
        public void DestructMessage_AggregateExceptionWithTwoInnerExceptions_ContainsMessages()
        {
            var divideByZeroException = new DivideByZeroException("DivideByZero");
            var invalidOperationException = new InvalidOperationException("InvalidOperation");

            var aggregateException = new AggregateException(
                divideByZeroException,
                invalidOperationException);

            string actual = aggregateException.DestructMessage();

            StringAssert.Contains(aggregateException.Message, actual);
            StringAssert.Contains(divideByZeroException.Message, actual);
            StringAssert.Contains(invalidOperationException.Message, actual);
        }
    }
}