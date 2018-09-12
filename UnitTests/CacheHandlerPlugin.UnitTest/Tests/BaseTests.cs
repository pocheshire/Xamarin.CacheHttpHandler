using System;

namespace CacheHandlerPlugin.UnitTest.Tests
{
    public class BaseTests
    {
        protected Uri RequestUri => new Uri("http://google.com");

        protected UnitTestFixture Fixture;

        public BaseTests(UnitTestFixture fixture) =>
            Fixture = fixture;
    }
}
