using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using dotnet5.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnet5.Services
{
    public class WarehouseService : IWarehouseService
    {

        private readonly IConfiguration _configuration;
        public WarehouseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> ExecuteOrderProcAsync(Warehouse order)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Default")))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "EXECUTE AddProductToWarehouse @IdProduct, @IdWarehouse, @Amount, @CreatedAt";
                command.Parameters.AddWithValue("@IdProduct", order.IdProduct);
                command.Parameters.AddWithValue("@IdWarehouse", order.IdWarehouse);
                command.Parameters.AddWithValue("@Amount", order.Amount);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                connection.Open();
                try {
                    var result = await command.ExecuteScalarAsync();
                    if(result != null){
                        return (int) result;
                    } else {
                        return -4;
                    }
                } catch (Exception e) {
                    switch (e.Message) {
                        case "Invalid parameter: Provided IdProduct does not exist":
                            return -4;
                        case "Invalid parameter: There is no order to fullfill":
                            return -2;
                        case "Invalid parameter: Provided IdWarehouse does not exist":
                            return -1;
                        default:
                            return -4;
                    }
                }
            }

        }

        public async Task<int> ExecuteOrderAsync(Order order)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("Default")))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "SELECT * FROM Warehouse WHERE IdWarehouse = @Id";
                command.Parameters.AddWithValue("@Id", order.IdWarehouse);
                await connection.OpenAsync();

                using (var result = await command.ExecuteReaderAsync()) {
                    if(!result.HasRows)
                    {
                        return -1; // no warehouse with this id
                    }
                }

                command.Parameters.Clear();
                command.CommandText = "SELECT * FROM \"Order\" WHERE IdProduct = @Id AND Amount = @Amount AND DATEDIFF(ms, CreatedAt, @CreatedAt) > 0";
                Console.WriteLine(command.CommandText);
                command.Parameters.AddWithValue("@Id", order.IdProduct);
                command.Parameters.AddWithValue("@Amount", order.Amount);
                command.Parameters.AddWithValue("@CreatedAt", order.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                using (var result = await command.ExecuteReaderAsync()) {
                    if (!result.HasRows)
                    {
                        return -2; // no order with this id and amount or order creation date is newer than order fulfillment date
                    }
                }

                command.Parameters.Clear();
                command.CommandText = "SELECT * FROM Product_Warehouse WHERE IdOrder = @Id";
                command.Parameters.AddWithValue("@Id", order.IdOrder);

                using (var result = await command.ExecuteReaderAsync()) {
                    if(result.HasRows)
                    {
                        return -3; // order already fulfilled
                    }
                }

                command.Parameters.Clear();
                command.CommandText = "UPDATE Order SET FulfilledAt = @FulfilledAt WHERE IdOrder = @Id";
                command.Parameters.AddWithValue("@Id", order.IdOrder);
                command.Parameters.AddWithValue("@FulfilledAt", DateTime.Now); // Shouldn't this be order.CreatedAt? (Text is misleading)
                command.ExecuteNonQuery();

                using (var result = await command.ExecuteReaderAsync()) {
                    if(!result.HasRows)
                    {
                        return -4; // generic error
                    }
                    else
                    {

                        command.Parameters.Clear();
                        command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @Id";
                        command.Parameters.AddWithValue("@Id", order.IdProduct);

                        var result2 = await command.ExecuteReaderAsync();
                        int single_product_price = result2.GetInt32(0);
                        _ = result2.CloseAsync();

                        command.Parameters.Clear();
                        command.CommandText = "INSERT INTO Product_Warehouse (IdOrder, IdWarehouse, Price) VALUES (@IdOrder, @IdWarehouse, @Price)";
                        command.Parameters.AddWithValue("@IdOrder", order.IdOrder);
                        command.Parameters.AddWithValue("@IdWarehouse", order.IdWarehouse);
                        command.Parameters.AddWithValue("@Price", order.Amount * single_product_price);
                        using (var result3 = await command.ExecuteReaderAsync()){
                            return result3.GetInt32(0);
                        }
                    }
                }
            }         

        }
    }
}