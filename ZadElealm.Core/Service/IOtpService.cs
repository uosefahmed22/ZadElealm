﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Service
{
    public interface IOtpService
    {
        public string GenerateOtp(string email);
        public bool IsValidOtp(string email, string otp);
    }
}
