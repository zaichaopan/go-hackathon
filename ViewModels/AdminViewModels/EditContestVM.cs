﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Riipen_SSD.AdminViewModels
{
    public class EditContestVM
    {
        public int ContestID { get; set; }

        [Required]
        [Display(Name = "Name")]
        public String ContestName { get; set; }
        [Required]
        [Display(Name = "Date")]
        public DateTime? StartTime { get; set; }
        public String Location { get; set; }
        public IEnumerable<ParticipantVM> Participants { get; set; }
        public IEnumerable<CriteriaVM> Criteria { get; set; }
        public IEnumerable<JudgeVM> Judges { get; set; }
    }

}
