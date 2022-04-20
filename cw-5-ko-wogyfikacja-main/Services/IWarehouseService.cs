using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet5.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnet5.Services
{
    public interface IWarehousesService
    {

        public Task<int> ExecuteOrderProcAsync(Warehouse order);
        public Task<int> ExecuteOrderAsync(Warehouse order);
    }
}