using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftHSMv2ForTesting
{
    /// <summary>
    /// The SoftHSMv2 settings for initializer.
    /// </summary>
    /// <see cref="SoftHsmInitializer"/>
    public class SoftHsmSettings
    {
        /// <summary>
        /// Gets or sets the deploy folder.
        /// </summary>
        /// <value>
        /// The deploy folder.
        /// </value>
        public string DeployFolder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the label.
        /// </summary>
        /// <value>
        /// The name of the label.
        /// </value>
        public string LabelName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the SO PIN.
        /// </summary>
        /// <value>
        /// The SO PIN.
        /// </value>
        public string SoPin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user PIN.
        /// </summary>
        /// <value>
        /// The user PIN.
        /// </value>
        public string Pin
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoftHsmSettings"/> class.
        /// </summary>
        public SoftHsmSettings()
        {

        }
    }
}
