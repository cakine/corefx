using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;

namespace System.Tests
{
    public class UnloadingAndProcessExitTests : RemoteExecutorTestBase
    {
        [Fact]
        public void UnloadingEventMustHappenBeforeProcessExitEvent()
        {
            string fileName = GetTestFilePath();

            File.WriteAllText(fileName, string.Empty);

            Func<string, int> otherProcess = f =>
            {
                Action<int> OnUnloading = i => File.AppendAllText(f, string.Format("u{0}", i));
                Action<int> OnProcessExit = i => File.AppendAllText(f, string.Format("e{0}", i));

                AppDomain.CurrentDomain.ProcessExit += (sender, e) => OnProcessExit(0);
                System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += acl => OnUnloading(0);
                AppDomain.CurrentDomain.ProcessExit += (sender, e) => OnProcessExit(1);
                System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += acl => OnUnloading(1);

                return SuccessExitCode;
            };

            using (var remote = RemoteInvoke(otherProcess, fileName))
            {
            }

            Assert.Equal(File.ReadAllText(fileName), "u0u1e0e1");
        }

        [Fact]
        public void myTest()
        {
            bool crashed = false;

            Func<int> otherProcess = () =>
            {
                EventHandler onProcessExit = (e, sender) =>
                {
                    throw new Exception();
                };

                AppDomain.CurrentDomain.ProcessExit += onProcessExit;

                return SuccessExitCode;
            };

            try
            {
                using (var remote = RemoteInvoke(otherProcess))
                {
                }
            }
            catch (Exception)
            {
                crashed = true;
            }

            //if (!crashed)
                //Assert.Equal(1, 2);
        }
    }
}
