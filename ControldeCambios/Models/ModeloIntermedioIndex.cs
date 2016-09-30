using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ControldeCambios.Models
{
    public class ModeloIntermedioIndex
    {
        public List<AspNetUsers> listaAspUser { get; set; }
        public List<Usuarios> listaUsuarios { get; set; }
        public List<AspNetRoles> listaAspRoles { get; set; }

    }
}