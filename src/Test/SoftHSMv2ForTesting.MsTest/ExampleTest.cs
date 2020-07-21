using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SoftHSMv2ForTesting;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.IO;
using System.Linq;

namespace SoftHSMv2ForTesting.MsTest
{
    [TestClass]
    public class AssemblyInitializedTest
    {
        public const string TokenName = "TestCardToken";
        public const string TokenSoPin = "abcdef";
        public const string TokenUserPin = "abc123*!~";

        private static SoftHsmContext softHsmContext = null;

        public static string Pkcs11LibPath
        {
            get => softHsmContext.Pkcs11LibPath;
        }


        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            softHsmContext = SoftHsmInitializer.Init(opt =>
            {
#if NETCOREAPP
                opt.DeployFolder = Path.Combine(Path.GetTempPath(), $"SoftHSMv2-{Guid.NewGuid():D}");
#else
                opt.DeployFolder = Path.Combine(context.DeploymentDirectory, "SoftHSMv2");
#endif

                opt.LabelName = TokenName;
                opt.Pin = TokenUserPin;
                opt.SoPin = TokenSoPin;
            });

            // Add another Token
            softHsmContext.AddToken("SecondToken", "abc456", "hello");
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            softHsmContext?.Dispose();
        }
    }

    [TestClass]
    public class ExampleTests
    {
        public TestContext TestContext
        {
            get;
            set;
        }

        [TestMethod]
        public void GetTokenSerial()
        {
            using (Pkcs11 pkcs11 = new Pkcs11(AssemblyInitializedTest.Pkcs11LibPath, AppType.MultiThreaded))
            {
                Slot slot = pkcs11.GetSlotList(SlotsType.WithTokenPresent)
                     .Single(t => t.GetTokenInfo().Label == AssemblyInitializedTest.TokenName);

                using (Session session = slot.OpenSession(SessionType.ReadOnly))
                {
                    session.Login(CKU.CKU_USER, AssemblyInitializedTest.TokenUserPin);

                    string serialNumber = slot.GetTokenInfo().SerialNumber;
                    this.TestContext.WriteLine("Token has serial: {0}", serialNumber);
                }
            }
        }
    }
}
