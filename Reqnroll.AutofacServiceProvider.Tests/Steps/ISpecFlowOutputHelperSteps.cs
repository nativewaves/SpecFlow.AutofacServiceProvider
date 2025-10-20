using Reqnroll;
using Xunit;

namespace NativeWaves.Reqnroll.AutofacServiceProvider.Tests.Steps
{
    [Binding]
    public class IReqnrollOutputHelperSteps
    {
        private readonly IReqnrollOutputHelper _output;

        public IReqnrollOutputHelperSteps(IReqnrollOutputHelper output)
        {
            _output = output;
        }

        [When(@"a message is output using IReqnrollOutputHelper")]
        public void WhenAMessageIsOutputUsingIReqnrollOutputHelper()
        {
            _output.WriteLine("This is output from IReqnrollOutputHelper");
        }

        [Then(@"verify that IReqnrollOutputHelper is correctly injected")]
        public void ThenVerifyThatIReqnrollOutputHelperIsCorrectlyInjected()
        {
            Assert.NotNull(_output);
        }
    }
}
