using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControldeCambios.Models
{
    public class Solicitud_CambiosIndexModel
    {
        public string proyecto { get; set; }
        public class solicitudInfo
        {
            public int id;
            public string nombre;
            public string codigo;
            public int prioridad;
            public string solicitadoPor;
            public string estado;
            public string fecha;
        }
        public List<Solicitud_Cambios> solicitudes;
        public List<solicitudInfo> indexSolicitudInfoList;
        public Boolean crearRequerimientos { get; set; }
        public Boolean detallesRequerimientos { get; set; }
    }
}