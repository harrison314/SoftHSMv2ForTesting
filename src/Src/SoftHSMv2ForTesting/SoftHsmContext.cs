using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftHSMv2ForTesting
{
    /// <summary>
    /// The SoftHSMv2 context.
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public class SoftHsmContext : IDisposable
    {
        private readonly string softHsmBasePath;

        /// <summary>
        /// Gets the full path to SoftHSMv2 PKCS#11 library.
        /// </summary>
        /// <value>
        /// The full path to SoftHSMv2 PKCS#11 library.
        /// </value>
        public string Pkcs11LibPath
        {
            get
            {
                if (Environment.Is64BitProcess)
                {
                    return this.GetPkcs11LibPathX64();
                }
                else
                {
                    return this.GetPkcs11LibPathX86();
                }
            }
        }

        internal SoftHsmContext(string softHsmBasePath)
        {
            this.softHsmBasePath = softHsmBasePath;
        }

        /// <summary>
        /// Gets the full path to SoftHSMv2 PKCS#11 x86 library.
        /// </summary>
        /// <returns>The full path to SoftHSMv2 PKCS#11 x86 library.</returns>
        public string GetPkcs11LibPathX86()
        {
            return Path.Combine(this.softHsmBasePath, "lib", "softhsm2.dll");
        }

        /// <summary>
        /// Gets the full path to SoftHSMv2 PKCS#11 x64 library.
        /// </summary>
        /// <returns>The full path to SoftHSMv2 PKCS#11 x64 library.</returns>
        public string GetPkcs11LibPathX64()
        {
            return Path.Combine(this.softHsmBasePath, "lib", "softhsm2-x64.dll");
        }

        /// <summary>
        /// Add a new token to current SoftHSMv2.
        /// </summary>
        /// <param name="tokenLabel">Token label.</param>
        /// <param name="pin">Token user PIN.</param>
        /// <param name="soPin">Token SO PIN.</param>
        /// <exception cref="ArgumentNullException">
        /// tokenLabel
        /// or
        /// pin
        /// or
        /// soPin
        /// </exception>
        public void AddToken(string tokenLabel, string pin, string soPin)
        {
            if (tokenLabel == null)
            {
                throw new ArgumentNullException(nameof(tokenLabel));
            }

            if (pin == null)
            {
                throw new ArgumentNullException(nameof(pin));
            }

            if (soPin == null)
            {
                throw new ArgumentNullException(nameof(soPin));
            }

            string utilPath = Path.Combine(this.softHsmBasePath, "bin", "softhsm2-util.exe");
            SoftHsmInitializer.InitToken(utilPath, tokenLabel, pin, soPin);
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// Try remove SoftHSMv2 folder with databases.
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < 6; i++)
            {
                try
                {
                    Directory.Delete(this.softHsmBasePath, true);
                    return;
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    System.Threading.Thread.Sleep(250);
                }
            }
        }
    }
}
