using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.Data
{
    public class CartRepository
    {
        private readonly ILogger _logger;
        public CartRepository(ILogger<CartModel> logger)
        {
            _logger = logger;
        }

        public async Task<List<CartModel>> GetCartData()
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Cart";
            var products = new List<CartModel>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new CartModel
                {
                    Id = reader.GetInt32(0),
                    ProductServerId = reader.GetInt32(1),
                    ProductCode = reader.GetString(2),
                    Quantity = reader.GetInt32(3),
                    Price = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    PriceIncl = reader.GetDecimal(5),
                    HasImage = reader.GetBoolean(6),
                    Description = reader.GetString(7),
                    UnitOfSale = reader.GetString(8),
                    TaxPercentage = reader.GetDecimal(9),
                    TotalInc = reader.GetDecimal(10),
                    lineTotal = reader.GetDecimal(11),
                    NettPrice = reader.GetDecimal(12),
                    VatTotal = reader.GetDecimal(13),
                   
                });
            }
            return products;
        }

        public async Task<int> CheckProductExist(int ProductServerId)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = $"SELECT ProductServerId FROM Cart where ProductServerId = {ProductServerId}";
            var products = new List<CartModel>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new CartModel
                {
                    ProductServerId = reader.GetInt32(0),
                    //ProductServerId = reader.GetInt32(1),
                    //ProductCode = reader.GetString(2),
                    //Quantity = reader.GetInt32(3),
                    //Price = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    //PriceIncl = reader.GetDecimal(5),
                    //HasImage = reader.GetBoolean(6),
                    //Description = reader.GetString(7),
                    //UnitOfSale = reader.GetString(8),
                    //TaxPercentage = reader.GetDecimal(9),
                    //TotalInc = reader.GetDecimal(10),
                    //lineTotal = reader.GetDecimal(11),
                    //NettPrice = reader.GetDecimal(12),
                    //VatTotal = reader.GetDecimal(13),

                });
            }
            if(products.Count > 0)
            {
               var product = products.FirstOrDefault();
                if (product != null)
                {
                    return ProductServerId;
                }
            }
            return 0;
        }

        public async Task DeleteCartItem(int ProductServerId)
        {

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @$"Delete From Cart where ProductServerId = {ProductServerId}";
                         
                   
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "Error Delete From Cart table");
                throw;
            }

        }

        public async Task CreateTableCart()
        {

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
                         
                                  CREATE TABLE Cart (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProductServerId INTEGER NOT NULL UNIQUE,
                            ProductCode TEXT NOT NULL,
                            Quantity INTEGER NOT NULL,
                            Price NUMERIC NOT NULL,
                            PriceIncl NUMERIC NOT NULL,
                            HasImage INTEGER NOT NULL DEFAULT 0,
                            Description TEXT NOT NULL,
                            UnitOfSale TEXT NOT NULL,
                            TaxPercentage NUMERIC NOT NULL,
                            TotalInc NUMERIC NOT NULL DEFAULT 0,
                            lineTotal NUMERIC NOT NULL DEFAULT 0,
                            NettPrice NUMERIC NOT NULL DEFAULT 0,
                            VatTotal NUMERIC NOT NULL DEFAULT 0
                        );
                    ";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "Error creating Cart table");
                throw;
            }

        }

        public async Task InsertCart(CartModel cartModel)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();
            var insertCommand = connection.CreateCommand();

            var productServerId = await CheckProductExist(cartModel.ProductServerId);
            if (productServerId == 0)
            {
                insertCommand.CommandText = @"
                    
                    INSERT INTO Cart (
                        ProductServerId,
                        ProductCode,
                        Quantity,
                        Price,
                        PriceIncl,
                        HasImage,
                        Description,
                        UnitOfSale,
                        TaxPercentage,
                        TotalInc,
                        lineTotal,
                        NettPrice,
                        VatTotal
                    )
                    VALUES (
                        @ProductServerId,
                        @ProductCode,
                        @Quantity,
                        @Price,
                        @PriceIncl,
                        @HasImage,
                        @Description,
                        @UnitOfSale,
                        @TaxPercentage,
                        @TotalInc,
                        @lineTotal,
                        @NettPrice,
                        @VatTotal
                    );
                ";
            }
            else
            {
                insertCommand.CommandText = @"
                    
                    Update Cart SET
                        Quantity = @Quantity,
                        TotalInc = @TotalInc,
                        lineTotal = @lineTotal,
                        NettPrice = @NettPrice,
                        VatTotal = @VatTotal
                        Where ProductServerId = @ProductServerId
                ";
            }
           

            insertCommand.Parameters.AddWithValue("@ProductServerId", DbType.Int32).Value = cartModel.ProductServerId;
            insertCommand.Parameters.AddWithValue("@ProductCode", DbType.String).Value = cartModel.ProductCode;
            insertCommand.Parameters.AddWithValue("@Quantity", DbType.Int32).Value = cartModel.Quantity;
            insertCommand.Parameters.AddWithValue("@Price", DbType.Int32).Value = cartModel.Price;
            insertCommand.Parameters.AddWithValue("@PriceIncl", DbType.Int32).Value = cartModel.PriceIncl;
            insertCommand.Parameters.AddWithValue("@HasImage", DbType.Int32).Value = cartModel.HasImage ? 1 : 0;
            insertCommand.Parameters.AddWithValue("@Description", DbType.String).Value =
                (object?)cartModel.Description ?? DBNull.Value;
            insertCommand.Parameters.AddWithValue("@UnitOfSale", DbType.String).Value =
                (object?)cartModel.UnitOfSale ?? DBNull.Value;
            insertCommand.Parameters.AddWithValue("@TaxPercentage", DbType.Double).Value =
                (object?)cartModel.TaxPercentage ?? DBNull.Value;
            insertCommand.Parameters.AddWithValue("@TotalInc", DbType.Double).Value =
                (object?)cartModel.TotalInc ?? DBNull.Value;
            insertCommand.Parameters.AddWithValue("@lineTotal", DbType.Double).Value =
                (object?)cartModel.lineTotal ?? DBNull.Value;
            insertCommand.Parameters.AddWithValue("@NettPrice", DbType.Double).Value =
                (object?)cartModel.NettPrice ?? DBNull.Value;
            insertCommand.Parameters.AddWithValue("@VatTotal", DbType.Double).Value =
                (object?)cartModel.VatTotal ?? DBNull.Value;

            await insertCommand.ExecuteNonQueryAsync();
        }
    }
}
