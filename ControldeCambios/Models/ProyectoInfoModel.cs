using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControldeCambios.Models
{
    public class ProyectoInfoModel
    {
        public Proyecto proyecto { get; set; }

        public List<Sprint> sprints { get; set; }

        public class sprintInfo
        {
            public string numero;
            public string modulos;
            public int puntaje;
            public string fechaInicio;
            public string fechaFinal;
        }

        public List<sprintInfo> indexSprintInfoList { get; set; }

        public List<Modulo> modulos { get; set; }

        public class moduloInfo
        {
            public string numero;
            public string nombre;
            public string requerimientos;
            public int puntaje;
        }

        public List<moduloInfo> indexModuloInfoList { get; set; }

        public Boolean crearSprints { get; set; }

        public Boolean detallesSprints { get; set; }

        public Boolean crearModulos { get; set; }

        public Boolean detallesModulos { get; set; }
    }
}