﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Models;
using PizzaEntities;

namespace PizzaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("Origins")]
    public class PizzasController : ControllerBase
    {
        private readonly PizzaAppContext _context;

        public PizzasController(PizzaAppContext context)
        {
            _context = context;
        }

        // GET: api/Pizzas
        [HttpGet]
        public IEnumerable<Pizza> GetPizza()
        {
            return _context.Pizza;
        }

        // GET: api/Pizzas/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPizza([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pizza = await _context.Pizza.FindAsync(id);

            if (pizza == null)
            {
                return NotFound();
            }

            return Ok(pizza);
        }

        [HttpGet]
        [Route("GetPrice/{pizzaId}")]
        public async Task<decimal> getPizzaPrice([FromRoute] int pizzaId)
        {
            if (!ModelState.IsValid)
            {
                return 0.0m;
            }
            var ourPizza = await _context.Pizza.FirstOrDefaultAsync(n => n.id == pizzaId);
            if (ourPizza == null)
            {
                return 0.0m;
            }
            return await _context.getPizzaPriceAsync(pizzaId);
        }

        // GetPizzaOrders
        [HttpGet]
        [Route("GetOrderPizzas/{orderId}")]
        public IEnumerable<Pizza> getOrderPizzas([FromRoute] int orderId)
        {
            var pizzasInOrder = _context.Pizza.Where(n => n.OrderId == orderId);
            return pizzasInOrder;
        }

        // PUT: api/Pizzas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPizza([FromRoute] int id, [FromBody] Pizza pizza)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != pizza.id)
            {
                return BadRequest();
            }

            _context.Entry(pizza).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PizzaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Pizzas
        [HttpPost]
        public async Task<IActionResult> PostPizza([FromBody] Pizza pizza)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Pizza.Add(pizza);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PizzaExists(pizza.id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPizza", new { id = pizza.id }, pizza);
        }

        // DELETE: api/Pizzas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePizza([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pizza = await _context.Pizza.FindAsync(id);
            if (pizza == null)
            {
                return NotFound();
            }

            _context.Pizza.Remove(pizza);
            await _context.SaveChangesAsync();

            return Ok(pizza);
        }

        [HttpDelete("{orderId}")]
        [Route("RemoveOrderPizzas/{orderId}")]
        public async Task<IActionResult> removeOrderPizzas([FromRoute] int orderId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var pizzasToRemove = _context.Pizza.Where(n => n.OrderId == orderId);
            if ( pizzasToRemove == null)
            {
                return NotFound();
            }
            var thePizzas = await pizzasToRemove.ToListAsync();
            foreach(Pizza pizza in thePizzas)
            {
                _context.Pizza.Remove(pizza);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool PizzaExists(int id)
        {
            return _context.Pizza.Any(e => e.id == id);
        }
    }
}