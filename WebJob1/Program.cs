﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace WebJob1
{
    class Program
    {
        static void Main()
        {

            var host = new JobHost();
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }

        
    }
}
