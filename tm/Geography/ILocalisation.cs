﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace tm
{
    public interface ILocalisation
    {

        List<Tournament> Tournaments();
        string Name();

        Association GetContinentalAssociation();
    }
}
