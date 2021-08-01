using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioIndraFinal.Models
{
    public class Transacao
    {
        public int Id { get; set; }
        public string TipoConta { get; set; }
        public int IdConta { get; set; }
        public string Tipo { get; set; } 
        public decimal Valor { get; set; }
        public string Data { get; set; }
    }
}
