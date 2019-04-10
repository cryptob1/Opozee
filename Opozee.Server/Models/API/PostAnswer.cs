using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Opozee.Models.API
{
    public class PostAnswer
    {
        public int Id { get; set; }
        public int QuestId { get; set; }
        public string Comment { get; set; }
        public int CommentedUserId { get; set; }
        public string CreationDate { get; set; }
        public string ModifiedDate { get; set; }
        public int Likes { get; set; }
        public bool OpinionAgreeStatus { get; set; }
        public int Dislikes { get; set; }
    }
}