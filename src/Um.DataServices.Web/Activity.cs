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
    
    public partial class Activity
    {
        public int Id { get; set; }
        public int iati_activitiesId { get; set; }
        public string ActivityId { get; set; }
        public string C_ref { get; set; }
        public string value__currency { get; set; }
        public System.DateTime value__value_date { get; set; }
        public long value_text__ { get; set; }
        public string transaction_type__code { get; set; }
        public string transaction_type__xml_lang { get; set; }
        public string transaction_type_text__ { get; set; }
        public string provider_org__ref { get; set; }
        public string provider_org__provider_activity_id { get; set; }
        public string provider_org_text__ { get; set; }
        public string receiver_org__ref { get; set; }
        public string receiver_org__receiver_activity_id { get; set; }
        public string receiver_org_text__ { get; set; }
        public System.DateTime transaction_date__iso_date { get; set; }
        public string transaction_date_text__ { get; set; }
        public int flow_type__code { get; set; }
        public string flow_type__xml_lang { get; set; }
        public string flow_type_text__ { get; set; }
        public string aid_type__code { get; set; }
        public string aid_type__xml_lang { get; set; }
        public string aid_type_text__ { get; set; }
        public int finance_type__code { get; set; }
        public string finance_type__xml_lang { get; set; }
        public string finance_type_text__ { get; set; }
        public int tied_status__code { get; set; }
        public string tied_status__xml_lang { get; set; }
        public string tied_status_text__ { get; set; }
        public int disbursement_channel__code { get; set; }
        public string disbursement_channel__xml_lang { get; set; }
        public string disbursement_channel_text__ { get; set; }
        public string country_code { get; set; }
        public string country_code_iati { get; set; }
        public string C_cod { get; set; }
        public string orgid { get; set; }
        public Nullable<int> secid { get; set; }
        public string subcompanycode { get; set; }
        public Nullable<int> regid { get; set; }
    }
}
