﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMT.Common.Rest.Router
{
    class ParamPatternSegment : PatternSegment
    {
        protected override bool IsParam { get { return true; } }

        public ParamPatternSegment(string value) : base(value) { }

        protected override bool IsMatch(string value)
        {
            return true;
        }
    }
}
