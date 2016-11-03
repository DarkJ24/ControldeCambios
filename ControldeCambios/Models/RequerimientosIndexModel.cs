using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControldeCambios.Models
{
    public class RequerimientosIndexModel
    {
        public string proyecto { get; set; }
        public List<Requerimiento> reqs { get; set; }
        public Boolean crearRequerimientos { get; set; }
        public Boolean detallesRequerimientos { get; set; }
    }
}