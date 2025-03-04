using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class SerializationXGTest : SerializationTestBase<F1XGFixture>
    {
        public SerializationXGTest(F1XGFixture fixture)
            : base(fixture)
        {
        }
    }
}
