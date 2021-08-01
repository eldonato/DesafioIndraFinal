using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DesafioIndraFinal.Models;

namespace DesafioIndraFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContaCorrentesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContaCorrentesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ContaCorrentes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContaCorrente>>> GetContaCorrentes()
        {
            return await _context.ContaCorrentes.ToListAsync();
        }

        // GET: api/ContaCorrentes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ContaCorrente>> GetContaCorrente(int id)
        {
            var contaCorrente = await _context.ContaCorrentes.FindAsync(id);

            if (contaCorrente == null)
            {
                return NotFound();
            }

            return contaCorrente;
        }

        // DELETE: api/ContaCorrentes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContaCorrente(int id)
        {
            var contaCorrente = await _context.ContaCorrentes.FindAsync(id);
            if (contaCorrente == null)
            {
                return NotFound();
            }

            _context.ContaCorrentes.Remove(contaCorrente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContaCorrenteExists(int id)
        {
            return _context.ContaCorrentes.Any(e => e.Id == id);
        }

        [HttpPost]
        [Route("Cadastrar")]
        public async Task<ActionResult<ContaCorrente>> PostConta(int id, decimal saldo, int clienteId)
        {
            var Cliente = await _context.Clientes.FindAsync(clienteId);

            ContaCorrente contaCorrente = new ContaCorrente {
                Id = id, Saldo = saldo, ClienteId = clienteId, Cliente = Cliente};
            _context.ContaCorrentes.Add(contaCorrente);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContaCorrente", new { id = contaCorrente.Id }, contaCorrente);
        }
        
        [HttpPost("Depositar")]
        public async Task<IActionResult> DepositarContaCorrente(int id, decimal valor)
        {
            var contaCorrente = await _context.ContaCorrentes.FindAsync(id);

            if (contaCorrente == null)
            {
                return BadRequest();
            }

            contaCorrente.Depositar(valor);
            DateTime data = DateTime.UtcNow;

            Transacao T = new Transacao()
            {
                Id = 0,
                TipoConta = "ContaCorrente",
                IdConta = contaCorrente.Id,
                Tipo = "Deposito",
                Valor = valor,
                Data = data.ToString("HH:mm:ss dd/MM/yyyy")
            };

            _context.Transacoes.Add(T);
            _context.Entry(contaCorrente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContaCorrenteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok("Saldo: R$" + contaCorrente.Saldo.ToString("#.##"));
        }

        [HttpPost("Sacar")]
        public async Task<IActionResult> SaqueContaCorrente(int id, decimal valor)
        {
            var Cc = await _context.ContaCorrentes.FindAsync(id);

            if (Cc == null)
            {
                return BadRequest("Conta não existente.");
            }

            if (Cc.Sacar(valor))
            {
                DateTime data = DateTime.UtcNow;
                Transacao T = new Transacao()
                {
                    Id = 0,
                    TipoConta = "ContaCorrente",
                    IdConta = Cc.Id,
                    Tipo = "Saque",
                    Valor = -(valor + (valor * 0.002M)),
                    Data = data.ToString("HH:mm:ss dd/MM/yyyy")
                };

                _context.Transacoes.Add(T);
                _context.Entry(Cc).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContaCorrenteExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return Ok("Operação feita com sucesso!\n" +
                    "Saque: R$" + valor + " | Imposto: R$" + (valor * 0.02M) + "\n" +
                    "Saldo: R$" + Cc.Saldo.ToString("#.##"));
            }
            else
            {
                return BadRequest("Valor requerido maior que o saldo atual da conta.\n" +
                     "Valor requerido: R$" + valor +
                     "\nSaldo: R$" + Cc.Saldo.ToString("#.##"));
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
            var CcTrans = await _context.ContaCorrentes.FindAsync(IdTrans);
            var CcRec = await _context.ContaCorrentes.FindAsync(IdRec);

            if (CcTrans == null || CcRec == null)
            {
                return BadRequest("Conta não existente.");
            }

            if (CcTrans.TransferirCC(CcRec, valor))
            {
                DateTime data = DateTime.UtcNow;
                Transacao T1 = new Transacao()
                {
                    Id = 0,
                    TipoConta = "ContaCorrente",
                    IdConta = CcTrans.Id,
                    Tipo = "Transferencia",
                    Valor = -(valor + (valor * 0.002M)),
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
                _context.Entry(CcTrans).State = EntityState.Modified;
                _context.Entry(CcRec).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContaCorrenteExists(IdTrans) || !ContaCorrenteExists(IdRec))
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
                    "Saldo: R$" + CcTrans.Saldo.ToString("#.##"));
            }
            else
            {
                return BadRequest("Valor requerido maior que o saldo atual da conta.\n" +
                     "Valor requerido: R$" + valor +
                     "\nSaldo: R$" + CcTrans.Saldo.ToString("#.##"));
            };
        }

        [HttpPost("TransferirCP")]
        public async Task<IActionResult> TransferirCp(int IdTrans, int IdRec, decimal valor)
        {
            var CcTrans = await _context.ContaCorrentes.FindAsync(IdTrans);
            var CpRec = await _context.ContaPoupancas.FindAsync(IdRec);

            if (CcTrans == null || CpRec == null)
            {
                return BadRequest("Conta não existente.");
            }

            if (CcTrans.TransferirCP(CpRec, valor))
            {
                DateTime data = DateTime.UtcNow;
                Transacao T1 = new Transacao()
                {
                    Id = 0,
                    TipoConta = "ContaPoupanca",
                    IdConta = CcTrans.Id,
                    Tipo = "Transferencia",
                    Valor = -(valor + (valor * 0.002M)),
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

                _context.Entry(CcTrans).State = EntityState.Modified;
                _context.Entry(CpRec).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContaCorrenteExists(IdTrans) || !ContaCorrenteExists(IdTrans))
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
                    "Saldo: R$" + CcTrans.Saldo.ToString("#.##"));
            }
            else
            {
                return BadRequest("Valor requerido maior que o saldo atual da conta.\n" +
                     "Valor requerido: R$" + valor +
                     "\nSaldo: R$" + CcTrans.Saldo.ToString("#.##"));
            };
        }

        [HttpGet("ExtratoBancario")]
        public async Task<ActionResult<List<string>>> GetExtrato(int idConta)
        {
            var contaCorrente = await _context.ContaCorrentes.FindAsync(idConta);

            if (contaCorrente == null)
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

            extrato.Add("Saldo atual: R$" + contaCorrente.Saldo.ToString("#,##"));
            return Ok(extrato);

        }









    }
}
