using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML
{
    /// <summary>
    /// Esta clase se ocupara para manejar los resultados de las operaciones.
    /// </summary>
    public class Result
    {
        public bool Correct { get; set; }//Propiedad para saber si la operacion fue correcta o no.
        public string Message { get; set; }//Propiedad para enviar un mensaje.
        public object Object { get; set; }//Propiedad para enviar un objeto.
        public List<object> Objects { get; set; }//Propiedad para enviar una lista de objetos.
        public Exception Ex { get; set; }//Propiedad para enviar una excepcion.
    }
}
