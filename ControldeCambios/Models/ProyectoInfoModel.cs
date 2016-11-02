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

        public List<Sprint> indexSprintInfoList { get; set; }

        public List<Modulo> modulos { get; set; }

        public Boolean crearSprints { get; set; }

        public Boolean detallesSprints { get; set; }

        public Boolean crearModulos { get; set; }

        public Boolean detallesModulos { get; set; }
    }
}