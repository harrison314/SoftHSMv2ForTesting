# SoftHSMv2 (as nuget) for testing
[![NuGet Status](http://img.shields.io/nuget/v/SoftHSMv2ForTesting.svg?style=flat)](https://www.nuget.org/packages/SoftHSMv2ForTesting/)

This project pack [SoftHSMv2 v 2.5.0](https://github.com/opendnssec/SoftHSMv2) as nuget package along with minimal code for initialize and destroy SoftHSMv2.

It is designed for testing .Net projects, using _PKCS#11_ devices (e.g. smart cards, HSM, tokens,...),
in CI/CD enviroment.

## Getting started
Package manager
```
Install-Package SoftHSMv2ForTesting
```
dotnet cli
```
dotnet add package SoftHSMv2ForTesting
```

## Usage
Usage with _MS Test_ and [PKCS#11 Interop](https://github.com/Pkcs11Interop/Pkcs11Interop).
```cs
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
```
## Supported frmaeworks and platforms
Framewroks:
* .Netstandrad 2.0
* .Net Framewrok 4.0

Platforms:
* Windows x86
* Windows x64

## Contributing
Pull requests, issues and commentary welcome!

Forking the repo is probably the easiest way to get started. There is a nice list of issues, both bugs and features that is up for grabs. Or devise a feature of your own.

## Third Party Licenses
1. [SoftHSMv2](https://raw.githubusercontent.com/opendnssec/SoftHSMv2/develop/LICENSE)
1. [SoftHSM2 installer for MS Windows](https://github.com/disig/SoftHSM2-for-Windows/blob/master/LICENSE)
