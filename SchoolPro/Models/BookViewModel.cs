using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolPro.Models
{
    public class BookViewModel
    {


        public int Id { get; set; }
      
         public int UserId { get; set; }
        [Required]
        public string BookName { get; set; }

        public byte[] File { get; set; }
        public HttpPostedFileBase files { get; set; }
        public string Description { get; set; }
        [Required]
        public int Points { get; set; }
        public System.DateTime uploadDate { get; set; }
    }
}