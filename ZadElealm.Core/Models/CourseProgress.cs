﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Models
{
    public class CourseProgress
    {
        public float VideoProgress { get; set; }
        public float OverallProgress { get; set; }
        public int CompletedVideos { get; set; }
        public int TotalVideos { get; set; }
        public bool IsEligibleForQuiz { get; set; }
    }
}
