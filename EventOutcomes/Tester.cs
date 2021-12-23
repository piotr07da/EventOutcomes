using System;
using System.Threading.Tasks;

namespace EventOutcomes
{
    public class Tester
    {
        public static async Task TestAsync(Func<Test, Test> testSetup, IAdapter adapter)
        {
            throw new NotImplementedException();
        }

        public static async Task TestAsync(Test test, IAdapter adapter)
        {
            throw new NotImplementedException();
        }
    }
}
