//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Um.DataServices.Web
{
    using System;
    using System.Collections.Generic;
    
    public partial class Sector
    {
        public int ID { get; set; }
        public Nullable<int> sector_code { get; set; }
        public string sector_name_uk { get; set; }
        public Nullable<int> category_code { get; set; }
        public string category_name_uk { get; set; }
        public string parent_code { get; set; }
        public string parent_name_uk { get; set; }
        public string sector_name_dk { get; set; }
        public string category_name_dk { get; set; }
        public string parent_name_dk { get; set; }
    }
}
