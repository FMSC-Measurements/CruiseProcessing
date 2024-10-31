using CruiseDAL.DataObjects;
using CruiseProcessing.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

using TreeDefaultValue = CruiseDAL.V2.Models.TreeDefaultValue;

namespace CruiseProcessing.Test
{
    public class VolumeEquations_Test : DiTestBase
    {
        public VolumeEquations_Test(ITestOutputHelper output) : base(output)
        {
        }

    }
}
