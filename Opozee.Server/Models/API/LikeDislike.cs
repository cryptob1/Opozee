using opozee.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Opozee.Models.API
{
    public class LikeDislike
    {
        public int Id { get; set; }
        public int CommentedUserId { get; set; }
        public int CommentId { get; set; }
        public CommentStatus CommentStatus { get; set; }
        public int questId { get; set; }
        public string CreationDate { get; set; }
        public string ModifiedDate { get; set; }
    }
}