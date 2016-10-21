using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControldeCambios.Models
{
    public class RequerimientosIndexModel
    {
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public string lider { get; set; }
        public string cliente { get; set; }
        public List<Requerimiento> reqs { get; set; }
    }
}