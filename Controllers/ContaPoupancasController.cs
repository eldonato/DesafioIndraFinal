using DesafioIndraFinal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesafioIndraFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContaPoupancasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContaPoupancasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ContaPoupancas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContaPoupanca>>> GetContaPoupancas()
        {
            return await _context.ContaPoupancas.ToListAsync();
        }

        // GET: api/ContaPoupancas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ContaPoupanca>> GetContaPoupanca(int id)
        {
            var contaPoupanca = await _context.ContaPoupancas.FindAsync(id);

            if (contaPoupanca == null)
            {
                return NotFound();
            }

            return contaPoupanca;
        }

        

        // DELETE: api/ContaPoupancas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContaPoupanca(int id)
        {
            var contaPoupanca = await _context.ContaPoupancas.FindAsync(id);
            if (contaPoupanca == null)
            {
                return NotFound();
            }

            _context.ContaPoupancas.Remove(contaPoupanca);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContaPoupancaExists(int id)
        {
            return _context.ContaPoupancas.Any(e => e.Id == id);
        }

        // POST: api/ContaPoupancas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ContaPoupanca>> PostContaPoupanca(ContaPoupanca contaPoupanca)
        {
            _context.ContaPoupancas.Add(contaPoupanca);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContaPoupanca", new { id = contaPoupanca.Id }, contaPoupanca);
        }

        [HttpPost]
        [Route("Cadastrar")]
        public async Task<ActionResult<ContaPoupanca>> PostConta(int id, decimal saldo, int clienteId)
        {
            ContaPoupanca contaPoupanca = new ContaPoupanca{
                Id = id, Saldo = saldo, ClienteId = clienteId
            };
            _context.ContaPoupancas.Add(contaPoupanca);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContaPoupanca", new { id = contaPoupanca.Id }, contaPoupanca);

            //var Cliente = await _context.Clientes.FindAsync(clienteId);

            //ContaPoupanca Cp = new()
            //{
            //    Id = id,
            //    Saldo = saldo,
            //    ClienteId = clienteId,
            //    Cliente = Cliente
            //};

            //_context.ContaPoupancas.Add(Cp);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction("GetContaCorrente", new { id = Cp.Id }, Cp);
        }

        [HttpPost("Depositar")]
        public async Task<IActionResult> DepositarContaCorrente(int id, decimal valor)
        {
            var contaPoupanca = await _context.ContaPoupancas.FindAsync(id);

            

            if (contaPoupanca == null)
            {
                return BadRequest();
            }

            contaPoupanca.Depositar(valor);
            DateTime data = DateTime.UtcNow;

            Transacao T = new Transacao()
            {
                Id = 0,
                TipoConta = "ContaPoupanca",
                IdConta = contaPoupanca.Id,
                Tipo = "Deposito",
                Valor = valor,
                Data = data.ToString("HH:mm:ss dd/MM/yyyy")
            };

            _context.Transacoes.Add(T);
            _context.Entry(contaPoupanca).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaPoupancaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok("Saldo: R$" + contaPoupanca.Saldo.ToString("#.##"));
        }

        [HttpPost("Sacar")]
        public async Task<IActionResult> SaqueContaPoupanca(int id, decimal valor)
        {
            var Cp = await _context.ContaPoupancas.FindAsync(id);

            if (Cp == null)
            {
                return BadRequest("Conta não existente.");
            }

            if (Cp.Sacar(valor))
            {
                DateTime data = DateTime.UtcNow;
                Transacao T = new Transacao()
                {
                    Id = 0,
                    TipoConta = "ContaPoupanca",
                    IdConta = Cp.Id,
                    Tipo = "Saque",
                    Valor = -valor,
                    Data = data.ToString("HH:mm:ss dd/MM/yyyy")
                };

                _context.Transacoes.Add(T);
                _context.Entry(Cp).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContaPoupancaExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok("Operação feita com sucesso!\n" +
                    "Saque: R$" + valor + "\n" +
                    "Saldo: R$" + Cp.Saldo.ToString("#.##"));
            }
            else
            {
                return BadRequest("Valor requerido maior que o saldo atual da conta.\n" +
                     "Valor requerido: R$" + valor +
                     "\nSaldo: R$" + Cp.Saldo.ToString("#.##"));
            };

            //_context.Entry(contaCorrente).State = EntityState.Modified;

            //try
            //{
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!ContaCorrenteExists(id))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}

            //return Ok("Operação feita com sucesso!\n" +
            //    "Saque: R$" + valor + " | Imposto: R$" + (valor * 0.02M) + "\n" +
            //    "Saldo: R$" + contaCorrente.Saldo.ToString("#.##"));
        }

        [HttpPost("TransferirCC")]
        public async Task<IActionResult> TransferirCc(int IdTrans, int IdRec, decimal valor)
        {
            var CpTrans = await _context.ContaPoupancas.FindAsync(IdTrans);
            var CcRec = await _context.ContaCorrentes.FindAsync(IdRec);

            if (CpTrans == null || CcRec == null)
            {
                return BadRequest("Conta não existente.");
            }

            if (CpTrans.TransferirCC(CcRec, valor))
            {
                DateTime data = DateTime.UtcNow;
                Transacao T1 = new Transacao()
                {
                    Id = 0,
                    TipoConta = "ContaPoupanca",
                    IdConta = CpTrans.Id,
                    Tipo = "Transferencia",
                    Valor = -valor,
                    Data = data.ToString("HH:mm:ss dd/MM/yyyy")
                };
                Transacao T2 = new Transacao()
                {
                    Id = 0,
                    TipoConta = "ContaCorrente",
                    IdConta = CcRec.Id,
                    Tipo = "Deposito",
                    Valor = valor,
                    Data = data.ToString("HH:mm:ss dd/MM/yyyy")
                };

                _context.Transacoes.Add(T1);
                _context.Transacoes.Add(T2);
                _context.Entry(CpTrans).State = EntityState.Modified;
                _context.Entry(CcRec).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContaPoupancaExists(IdTrans) || !ContaPoupancaExists(IdRec))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok("Operação feita com sucesso!\n" +
                    "Quantia: R$" + valor + " | Imposto: R$" + (valor * 0.02M) + "\n" +
                    "Saldo: R$" + CpTrans.Saldo.ToString("#.##"));
            }
            else
            {
                return BadRequest("Valor requerido maior que o saldo atual da conta.\n" +
                     "Valor requerido: R$" + valor +
                     "\nSaldo: R$" + CpTrans.Saldo.ToString("#.##"));
            };
        }

        [HttpPost("TransferirCP")]
        public async Task<IActionResult> TransferirCp(int IdTrans, int IdRec, decimal valor)
        {
            var CpTrans = await _context.ContaPoupancas.FindAsync(IdTrans);
            var CpRec = await _context.ContaPoupancas.FindAsync(IdRec);

            if (CpTrans == null || CpRec == null)
            {
                return BadRequest("Conta não existente.");
            }

            if (CpTrans.TransferirCP(CpRec, valor))
            {

                DateTime data = DateTime.UtcNow;
                Transacao T1 = new Transacao()
                {
                    Id = 0,
                    TipoConta = "ContaPoupanca",
                    IdConta = CpTrans.Id,
                    Tipo = "Transferencia",
                    Valor = -valor,
                    Data = data.ToString("HH:mm:ss dd/MM/yyyy")
                };
                Transacao T2 = new Transacao()
                {
                    Id = 0,
                    TipoConta = "ContaPoupanca",
                    IdConta = CpRec.Id,
                    Tipo = "Deposito",
                    Valor = valor,
                    Data = data.ToString("HH:mm:ss dd/MM/yyyy")
                };

                _context.Transacoes.Add(T1);
                _context.Transacoes.Add(T2);


                _context.Entry(CpTrans).State = EntityState.Modified;
                _context.Entry(CpRec).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContaPoupancaExists(IdTrans) || !ContaPoupancaExists(IdRec))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok("Operação feita com sucesso!\n" +
                    "Quantia: R$" + valor + " | Imposto: R$" + (valor * 0.02M) + "\n" +
                    "Saldo: R$" + CpTrans.Saldo.ToString("#.##"));
            }
            else
            {
                return BadRequest("Valor requerido maior que o saldo atual da conta.\n" +
                     "Valor requerido: R$" + valor +
                     "\nSaldo: R$" + CpTrans.Saldo.ToString("#.##"));
            };
        }

        [HttpPost("GerarRendimento")]
        public async Task<IActionResult> GerarRendimento()
        {
            List<ContaPoupanca> ListaContas = await _context.ContaPoupancas.ToListAsync();

            DateTime data = DateTime.UtcNow;

            foreach (ContaPoupanca Cp in ListaContas)
            {
                decimal rendimento = Cp.GerarRendimento();

               
                Transacao T = new Transacao()
                {
                    Id = 0,
                    TipoConta = "ContaPoupanca",
                    IdConta = Cp.Id,
                    Tipo = "Rendimento mensal",
                    Valor = rendimento,
                    Data = data.ToString("HH:mm:ss dd/MM/yyyy")
                };

                _context.Transacoes.Add(T);
                _context.Entry(Cp).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContaPoupancaExists(Cp.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }

            return Ok("Todas as contas poupança foram ajustadas com base no rendimento atual." );
           
        }

        [HttpGet("ExtratoBancario")]
        public async Task<ActionResult<List<string>>> GetExtrato(int idConta)
        {
            var contaPoupanca = await _context.ContaPoupancas.FindAsync(idConta);
            
            if(contaPoupanca == null)
            {
                return NotFound();
            }

            var transacoes = _context.Transacoes.Where(p => p.IdConta == idConta);

            if (transacoes == null)
            {
                return NotFound("Não há operações na c");
            }

            List<string> extrato = new List<string>();

            foreach (Transacao t in transacoes)
            {
                string transacao = (t.Tipo + " | R$" + t.Valor.ToString("#,##") + " | " + t.Data);
                extrato.Add(transacao);
            }

            extrato.Add("Saldo atual: R$" + contaPoupanca.Saldo.ToString("#,##"));
            return Ok(extrato);

        }
















    }
}
