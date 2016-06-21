﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Riipen_SSD.AdminViewModels
{
    public class Message
    {
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public Message() { }
        public Message(string sender, string subject, string body)
        {
            Sender = sender;
            Subject = subject;
            Body = body;
        }

    }
}