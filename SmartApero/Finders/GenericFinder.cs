﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApero.Finders
{
    public class GenericFinder : Finder
    {
        public override string Resolve(string text)
        {
            return text;
        }
    }
}
