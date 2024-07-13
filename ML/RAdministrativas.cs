using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML
{
    public class RAdministrativas
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Revision { get; set; }
        public bool CargadoAElastic { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaCarga { get; set; }
        public int IdPadre { get; set; }

    }
}
