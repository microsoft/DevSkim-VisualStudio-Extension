using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DevSkim.VSExtension
{
    public class Problem
    {
        public bool Actionable { get; set; }
        public Issue Issue { get; set; }
    }
}
