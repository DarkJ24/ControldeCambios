using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControldeCambios.Models
{
    public class SprintModelo
    {
          
        [Display(Name = "Proyecto")]
        public string proyecto { get; set; }

        [Display(Name = "Numero")]
        public int numero { get; set; }

        [Display(Name = "Fecha inicio")]
        public string fechaInicio { get; set; }

        [Display(Name = "Fecha final")]
        public string fechaFinal { get; set; }


        public List<string> requerimientos { get; set; }
         
    }
}