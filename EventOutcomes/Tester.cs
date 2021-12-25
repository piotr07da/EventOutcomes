using System;
using System.Threading.Tasks;

namespace EventOutcomes
{
    public class Tester
    {
        private readonly IAdapter _adapter;

        public Tester(IAdapter adapter)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public static async Task TestAsync(Func<Test, Test> testSetup, IAdapter adapter)
        {
            var tester = new Tester(adapter);
            var gwt = testSetup(new Test());
            await tester.InternalTestAsync(gwt);
        }

        public static async Task TestAsync(Test test, IAdapter adapter)
        {
            var tester = new Tester(adapter);
            await tester.InternalTestAsync(test);
        }

        private async Task InternalTestAsync(Test test)
        {
            await _adapter.BeforeTestAsync();

            // GIVEN

            // TODO

            // WHEN

            // TODO - exceptions expectations

            foreach (var command in test.ActCommands)
            {
                await _adapter.DispatchCommandAsync(command);
            }

            // THEN

            // TODO
        }
    }
}
