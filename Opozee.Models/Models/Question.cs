//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Opozee.Models.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Question
    {
        public int Id { get; set; }
        public string PostQuestion { get; set; }
        public int OwnerUserID { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public string TaggedUser { get; set; }
        public string HashTags { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}
