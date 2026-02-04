using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using RestSharp;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.Data
{
    public class ProductRepository
    {
        private readonly ILogger _logger;
        public ProductRepository(ILogger<ProductsWithQuantity> logger)
        {
            _logger = logger;
        }
        public async Task<List<ProductsWithQuantity>> GetProductsFromAPICall()
        {
            string token = Preferences.Default.Get("token", "null");
            int customerId = Preferences.Default.Get("customerId", 0);

            var options = new RestClientOptions("https://orders.lumarfoods.co.za:20603")
            {
                // MaxTimeout = -1,
            };
            var client = new RestClient(options);
            if(customerId == 0)
            {
                return new List<ProductsWithQuantity>();
            }
            var deliveryDate = DateTime.Now.ToString("yyyy-MM-dd");
            var request = new RestRequest($"/api/Products/GetAllProducts?CustomerID={customerId}&Deliverydate={deliveryDate}", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            if (response.Content != null)
            {
                var userResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductsWithQuantity>>(response.Content);
                if (userResponse != null)
                {
                    return userResponse; //.Take(5).ToList();
                }
            }
            return new List<ProductsWithQuantity>();
        }

        public async Task<List<ProductsWithQuantity>> GetProductsLocally()
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Products";
            var products = new List<ProductsWithQuantity>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new ProductsWithQuantity
                {
                    Id = reader.GetInt32(0),
                    Code = reader.GetString(1),
                    Description = reader.GetString(2),
                    QuantityOnHand = reader.GetDecimal(3),
                    HasImage = reader.GetBoolean(4),
                    Image = reader.IsDBNull(5) ? null : (byte[])reader[5],
                    Price = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                    PriceIncl = reader.GetDecimal(7),
                    OnSpecial = reader.GetBoolean(8),
                    SpecialPrice = reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                    TypicalOrderQuantity = reader.IsDBNull(10) ? null : reader.GetDecimal(10),
                    TaxPercentage = reader.GetDecimal(11),
                    UOM = reader.IsDBNull(12) ? null : reader.GetString(12),

                    Category1 = reader.IsDBNull(13) ? null : reader.GetString(13),
                    Category2 = reader.IsDBNull(14) ? null : reader.GetString(14),
                    Category3 = reader.IsDBNull(15) ? null : reader.GetString(15),
                    Category4 = reader.IsDBNull(16) ? null : reader.GetString(16),
                    Category5 = reader.IsDBNull(17) ? null : reader.GetString(17),
                    Category6 = reader.IsDBNull(18) ? null : reader.GetString(18),
                    Category7 = reader.IsDBNull(19) ? null : reader.GetString(19),
                    Category8 = reader.IsDBNull(20) ? null : reader.GetString(20),

                    isFavoured = reader.GetBoolean(21),
                    InfoApproved = reader.GetBoolean(22),
                    CategoryId = reader.GetInt32(23),
                    Category = reader.GetString(24),
                    Quantity = reader.GetInt32(25),
                    IsPromoted = reader.GetBoolean(26)
                });
            }

            return products;
        }

        public async Task<List<TblPromoPicturesSet>> GetSlotsFromAPICall()
        {
            string token = Preferences.Default.Get("token", "null");
            int customerId = Preferences.Default.Get("customerId", 0);

            var options = new RestClientOptions("https://orders.lumarfoods.co.za:20603")
            {
                // MaxTimeout = -1,
            };
            var client = new RestClient(options);
            if (customerId == 0)
            {
                return new List<TblPromoPicturesSet>();
            }
            var deliveryDate = DateTime.Now.ToString("yyyy-MM-dd");
            var request = new RestRequest($"/api/Products/GetPromos", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            if (response.Content != null)
            {
                var Response = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TblPromoPicturesSet>>(response.Content);
                if (Response != null)
                {
                    return Response;
                }
            }
            return new List<TblPromoPicturesSet>();
        }

        public async Task CreateTableProductsLocally()
        {

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
          
              
           CREATE TABLE Products (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,

                        Code TEXT NOT NULL,
                        Description TEXT NOT NULL,

                        QuantityOnHand REAL NOT NULL,
                        HasImage INTEGER NOT NULL,
                        Image BLOB,

                        Price REAL,
                        PriceIncl REAL NOT NULL,

                        OnSpecial INTEGER NOT NULL,
                        SpecialPrice REAL,
                        TypicalOrderQuantity REAL,

                        TaxPercentage REAL NOT NULL,

                        UOM TEXT,

                        Category1 TEXT,
                        Category2 TEXT,
                        Category3 TEXT,
                        Category4 TEXT,
                        Category5 TEXT,
                        Category6 TEXT,
                        Category7 TEXT,
                        Category8 TEXT,

                        isFavoured INTEGER NOT NULL,
                        InfoApproved INTEGER NOT NULL,

                        CategoryId INTEGER NOT NULL,
                        Category TEXT NOT NULL,
                        Quantity INTEGER NOT NULL,
                       IsPromoted INTEGER NOT NULL
                    );
                    ";
          
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Project table");
                throw;
            }

        }

            public async Task InsertProduct(ProductsWithQuantity product)
                {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();
            var insertProductCommand = connection.CreateCommand();
            insertProductCommand.CommandText = @"
                    INSERT INTO Products (
                        Code,
                        Description,
                        QuantityOnHand,
                        HasImage,
                        Image,
                        Price,
                        PriceIncl,
                        OnSpecial,
                        SpecialPrice,
                        TypicalOrderQuantity,
                        TaxPercentage,
                        UOM,
                        Category1,
                        Category2,
                        Category3,
                        Category4,
                        Category5,
                        Category6,
                        Category7,
                        Category8,
                        isFavoured,
                        InfoApproved,
                        CategoryId,
                        Category,
                        Quantity,
                       IsPromoted
                    )
                    VALUES (
                        @Code,
                        @Description,
                        @QuantityOnHand,
                        @HasImage,
                        @Image,
                        @Price,
                        @PriceIncl,
                        @OnSpecial,
                        @SpecialPrice,
                        @TypicalOrderQuantity,
                        @TaxPercentage,
                        @UOM,
                        @Category1,
                        @Category2,
                        @Category3,
                        @Category4,
                        @Category5,
                        @Category6,
                        @Category7,
                        @Category8,
                        @isFavoured,
                        @InfoApproved,
                        @CategoryId,
                        @Category,
                        @Quantity,
                       @IsPromoted
                    );
                ";

            insertProductCommand.Parameters.AddWithValue("@Code", DbType.String).Value = product.Code;
            insertProductCommand.Parameters.AddWithValue("@Description", DbType.String).Value = product.Description;
            insertProductCommand.Parameters.AddWithValue("@QuantityOnHand", DbType.Double).Value = product.QuantityOnHand;
            insertProductCommand.Parameters.AddWithValue("@HasImage", DbType.Int32).Value = product.HasImage ? 1 : 0;
            insertProductCommand.Parameters.AddWithValue("@Image", DbType.Binary).Value =
                        (object?)product.Image ?? DBNull.Value;

                    insertProductCommand.Parameters.AddWithValue("@Price", DbType.Double).Value =
                        (object?)product.Price ?? DBNull.Value;

                    insertProductCommand.Parameters.AddWithValue("@PriceIncl", DbType.Double).Value = product.PriceIncl;
                    insertProductCommand.Parameters.AddWithValue("@OnSpecial", DbType.Int32).Value = product.OnSpecial ? 1 : 0;

                    insertProductCommand.Parameters.AddWithValue("@SpecialPrice", DbType.Double).Value =
                        (object?)product.SpecialPrice ?? DBNull.Value;

                    insertProductCommand.Parameters.AddWithValue("@TypicalOrderQuantity", DbType.Double).Value =
                        (object?)product.TypicalOrderQuantity ?? DBNull.Value;

                    insertProductCommand.Parameters.AddWithValue("@TaxPercentage", DbType.Double).Value = product.TaxPercentage;
                    insertProductCommand.Parameters.AddWithValue("@UOM", DbType.String).Value =
                        (object?)product.UOM ?? DBNull.Value;

                    insertProductCommand.Parameters.AddWithValue("@Category1", DbType.String).Value =
                        (object?)product.Category1 ?? DBNull.Value;
                    insertProductCommand.Parameters.AddWithValue("@Category2", DbType.String).Value =
                        (object?)product.Category2 ?? DBNull.Value;
                    insertProductCommand.Parameters.AddWithValue("@Category3", DbType.String).Value =
                        (object?)product.Category3 ?? DBNull.Value;
                    insertProductCommand.Parameters.AddWithValue("@Category4", DbType.String).Value =
                        (object?)product.Category4 ?? DBNull.Value;
                    insertProductCommand.Parameters.AddWithValue("@Category5", DbType.String).Value =
                        (object?)product.Category5 ?? DBNull.Value;
                    insertProductCommand.Parameters.AddWithValue("@Category6", DbType.String).Value =
                        (object?)product.Category6 ?? DBNull.Value;
                    insertProductCommand.Parameters.AddWithValue("@Category7", DbType.String).Value =
                        (object?)product.Category7 ?? DBNull.Value;
                    insertProductCommand.Parameters.AddWithValue("@Category8", DbType.String).Value =
                        (object?)product.Category8 ?? DBNull.Value;

                    insertProductCommand.Parameters.AddWithValue("@isFavoured", DbType.Int32).Value =
                        product.isFavoured ? 1 : 0;

                    insertProductCommand.Parameters.AddWithValue("@InfoApproved", DbType.Int32).Value =
                        product.InfoApproved ? 1 : 0;

                    insertProductCommand.Parameters.AddWithValue("@CategoryId", DbType.Int32).Value = product.CategoryId;
                    insertProductCommand.Parameters.AddWithValue("@Category", DbType.String).Value = product.Category;
                    insertProductCommand.Parameters.AddWithValue("@Quantity", DbType.Int32).Value = product.Quantity;
                    insertProductCommand.Parameters.AddWithValue("@IsPromoted", DbType.Int32).Value =
                        product.IsPromoted ? 1 : 0;
            await  insertProductCommand.ExecuteNonQueryAsync();
                }

}
}
