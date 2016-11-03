using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControldeCambios.Models
{
    public class SprintInfoModel
    {
        public string sprintFechaInicio { get; set; }
        public string sprintFechaFinal { get; set; }
        public string sprintNumero { get; set; }
        public int sprintModulos { get; set; }
        public int sprintEsfuerzo { get; set; }
    }
}