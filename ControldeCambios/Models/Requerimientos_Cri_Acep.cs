//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ControldeCambios.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Requerimientos_Cri_Acep
    {
        public string reqCodigo { get; set; }
        public int reqVersion { get; set; }
        public string criterio { get; set; }
    
        public virtual Requerimiento Requerimiento { get; set; }
    }
}
