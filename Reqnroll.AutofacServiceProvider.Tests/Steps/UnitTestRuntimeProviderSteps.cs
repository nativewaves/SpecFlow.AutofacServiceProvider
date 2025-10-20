using Reqnroll;
using Reqnroll.UnitTestProvider;
using Xunit;

namespace NativeWaves.Reqnroll.AutofacServiceProvider.Tests.Steps
{
    [Binding]
    public class UnitTestRuntimeProviderSteps
    {
        private readonly IUnitTestRuntimeProvider _unitTestRuntimeProvider;

        public UnitTestRuntimeProviderSteps(IUnitTestRuntimeProvider unitTestRuntimeProvider)
        {
            _unitTestRuntimeProvider = unitTestRuntimeProvider;
        }

        [Then(@"verify that IUnitTestRuntimeProvider is correctly injected")]
        public void ThenVerifyThatIUnitTestRuntimeProviderIsCorrectlyInjected()
        {
            Assert.NotNull(_unitTestRuntimeProvider);
        }
    }
}
