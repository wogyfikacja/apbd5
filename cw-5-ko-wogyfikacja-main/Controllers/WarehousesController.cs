using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dotnet5.Models;
using dotnet5.Controllers;

namespace dotnet5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Warehouses2Controller : ControllerBase
    {
        IWarehouseService _warehouseService;

        public Warehouses2Controller(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(Warehouse order)
        {
            int status = await _warehouseService.ExecuteOrderAsync(order);

            if(status == -1)
            {
                return NotFound("No warehouse exists with this id");
            }
            else if(status == -2)
            {
                return NotFound("No order exists with these products for these warehouses");
            }
            else if(status == -3)
            {
                return BadRequest("Order is already fulfilled");
            }
            else if(status == -4)
            {
                return BadRequest(); // unknown error
            }
            else
            {
                return Ok(status);
            }
        }
    }
}