﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpeachRecognition
{
    public static class Windows
    {
        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();
    }
}
