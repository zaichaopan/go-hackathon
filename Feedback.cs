//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Riipen_SSD
{
    using System;
    using System.Collections.Generic;
    
    public partial class Feedback
    {
        public int ContestId { get; set; }
        public string JudgeUserId { get; set; }
        public int TeamId { get; set; }
        public string PublicComment { get; set; }
        public string PrivateComment { get; set; }
    
        public virtual ContestJudge ContestJudge { get; set; }
        public virtual Team Team { get; set; }
    }
}
