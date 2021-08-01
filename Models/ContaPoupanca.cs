using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioIndraFinal.Models
{
    public class ContaPoupanca
    {
        public int Id { get; set; }
        public decimal Saldo { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public Boolean Depositar(decimal quantia)
        {
            Saldo += quantia;
            return true;
        }

        public Boolean Sacar(decimal quantia)
        {
            if (Saldo >= quantia)
            {
                Saldo -= quantia;

                return true;
            }

            return false;
        }

        public Boolean TransferirCC(ContaCorrente Cc, decimal Quantia)
        {
            if (this.Sacar(Quantia))
            {
                Cc.Depositar(Quantia);
                return true;
            }
            else
            {
                return false;
            }

        }
        public Boolean TransferirCP(ContaPoupanca Cp, decimal Quantia)
        {
            if (this.Sacar(Quantia))
            {
                Cp.Depositar(Quantia);
                return true;
            }
            else
            {
                return false;
            }

        }

        public decimal GerarRendimento()
        {
            decimal juros = 0.02M;
            decimal rendimento = this.Saldo * juros;
            this.Saldo =+ rendimento ;
            return rendimento;
        }


    }
}
