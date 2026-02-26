using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using WebshopMobileApp.Models;

namespace WebshopMobileApp.Data
{
    public class ProductRepository
    {
        private readonly ILogger _logger;
        public string API_URL = Constants.API_URL;
        private readonly CartRepository cartRepository;
        private static readonly HttpClient _httpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            return new HttpClient(handler);
        }
        public ProductRepository(ILogger<ProductsWithQuantity> logger, CartRepository cartRepository)
        {
            _logger = logger;
            this.cartRepository = cartRepository;
        }
        public async Task<List<ProductsWithQuantity>> GetProductsFromAPICall()
        {
            string token = Preferences.Default.Get("token", "null");
            int customerId = Preferences.Default.Get("customerId", 0);

            var options = new RestClientOptions(API_URL)
            {
                // MaxTimeout = -1,
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true
            };
            var client = new RestClient(options);
            if(customerId == 0)
            {
                return new List<ProductsWithQuantity>();
            }
            var deliveryDate = DateTime.Now.ToString("yyyy-MM-dd");
            var request = new RestRequest($"/api/Products/GetAllProductsForMobile?CustomerID={customerId}&Deliverydate={deliveryDate}", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            if (response.Content != null)
            {
                var products = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductsWithQuantity>>(response.Content);
                if (products != null)
                {
                    if (products.Count > 0)
                    {
                        //products = products.Take(10).ToList();
                        await CreateTableProductsLocally();
                        // await ProductFileUrls(products);
                        //var products1 = products.Take(20).ToList();
                        await InsertProducts(products);
                        //foreach (var product in products)
                        //{
                        //    if(product.Price != null)
                        //    {
                        //        await InsertProduct(product);
                        //    }
                        //}
                       // await ProductFileUrls(products);
                        //var cati = products.Where(p => !string.IsNullOrEmpty(p.Category))
                        //            .GroupBy(p => new { p.CategoryId, p.Category })
                        //            .Select(g => (g.Key.CategoryId, g.Key.Category)).OrderBy(x => x.Category).ToList();
                        //if(cati.Count > 0)
                        //{
                        //    await CreateTableCategoriesLocally();
                        //    foreach (var cat in cati)
                        //    {
                        //        var realcat = new Category();
                        //        realcat.CategoryId = cat.CategoryId;
                        //        realcat.CategoryName = cat.Category;
                        //        realcat.FileUrl = Constants.API_URL + "/categories/" + cat.CategoryId + ".png";
                        //        realcat.FileUrl = await DownloadImageAsync(realcat.FileUrl, cat.CategoryId + ".png");
                        //        await InsertCategory(realcat);
                        //    }
                        //}
                    }
                    return products; //.Take(5).ToList();
                }
            }
            return new List<ProductsWithQuantity>();
        }
        public async Task<bool> GetCategoriesFromAPICall()
        {
            string token = Preferences.Default.Get("token", "null");
            int customerId = Preferences.Default.Get("customerId", 0);

            var options = new RestClientOptions(API_URL)
            {
                // MaxTimeout = -1,
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true
            };
            var client = new RestClient(options);
            if (customerId == 0)
            {
                return false;
            }
            var deliveryDate = DateTime.Now.ToString("yyyy-MM-dd");
            var request = new RestRequest($"/api/Products/GetAllCategoriesForMobile", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            if (response.Content != null)
            {
                var categories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Category>>(response.Content);
                if (categories != null)
                {
                    if (categories.Count > 0)
                    {
                       // await CategoryFileUrls(categories);
                        await CreateTableCategoriesLocally();
                        await InsertCategory(categories);
                           
                    }
                    return true; 
                }
            }
            return false;
        }


        private async Task CategoryFileUrls(List<Category> categories)
        {
            foreach (var x in categories)
            {
                var fileurl = x.FileUrl;
                x.FileUrl = await DownloadImageAsync(fileurl!, x.CategoryName!.ToString() + ".png");
            }
        }

        public async Task<string> DownloadImageAsync(string imageUrl, string fileName)
        {
            var options = new RestClientOptions(imageUrl)
            {
                // MaxTimeout = -1,
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true
            };

            var client = new RestClient(options);
            var request = new RestRequest();

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful || response.RawBytes == null)
                throw new Exception("Image download failed");

            var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await File.WriteAllBytesAsync(localPath, response.RawBytes);

            return localPath;
        }

        public async Task<List<Category>> GetCategoriesLocally()
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Categories";
            var products = new List<Category>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    CategoryId = reader.GetInt32(1),
                    CategoryName = reader.GetString(2),
                    FileUrl = reader.GetString(3)
                });
            }

            return products;
        }

        public async Task<int> GetTotalProductCount()
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Products;";

            var result = await command.ExecuteScalarAsync();

            return result != null && result != DBNull.Value
                ? Convert.ToInt32(result)
                : 0;
        }

        public async Task<int> GetTotalProductByCategoryCount(int CategoryId)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = @$"SELECT COUNT(*) FROM Products where CategoryId = {CategoryId};";

            var result = await command.ExecuteScalarAsync();

            return result != null && result != DBNull.Value
                ? Convert.ToInt32(result)
                : 0;
        }

        public async Task<int> GetTotalProductBySearchCount(string SearchWord)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = @$"SELECT COUNT(*) FROM Products where where Code like '%{SearchWord}%' or Description like '%{SearchWord}%'";

            var result = await command.ExecuteScalarAsync();

            return result != null && result != DBNull.Value
                ? Convert.ToInt32(result)
                : 0;
        }

        public async Task<List<ProductsWithQuantity>> GetProductsLocally(int PageSize, int PageNumber)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @$"SELECT * FROM Products LIMIT {PageSize} OFFSET ({PageNumber} - 1) * {PageSize}";
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
                    Price = reader.GetDecimal(6),
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
                    IsPromoted = reader.GetBoolean(26),
                    ProductServerId = reader.GetInt32(27),
                    FileUrl = reader.GetString(28),
                    ImageName = reader.GetString(29)
                });
            }

            return products;
        }

        public async Task<List<TblPromoPicturesSet>> GetPromosLocally()
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Promos";
            var promos = new List<TblPromoPicturesSet>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                promos.Add(new TblPromoPicturesSet
                {
                    Id = reader.GetInt32(0),
                    Url = reader.GetString(1)
                });
            }

            return promos;
        }
        public async Task<List<ProductsWithQuantity>> GetRecommendedProducts()
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Products limit 15";
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
                    Price = reader.GetDecimal(6),
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
                    IsPromoted = reader.GetBoolean(26),
                    ProductServerId = reader.GetInt32(27),
                    FileUrl = reader.GetString(28),
                    ImageName = reader.GetString(29)
                });
            }

            return products;
        }
        public async Task<List<ProductsWithQuantity>> GetProductsByCategory(int PageSize, int PageNumber, int CategoryId)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = $"SELECT * FROM Products where CategoryId = {CategoryId} LIMIT {PageSize} OFFSET ({PageNumber} - 1) * {PageSize}";
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
                    Price = reader.GetDecimal(6),
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
                    IsPromoted = reader.GetBoolean(26),
                    ProductServerId = reader.GetInt32(27),
                    FileUrl = reader.GetString(28),
                    ImageName = reader.GetString(29)
                });
            }

            return products;
        }
        public async Task<List<ProductsWithQuantity>> GetProductsBySearchWord(int PageSize, int PageNumber, string SearchWord)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = $"SELECT * FROM Products where Code like '%{SearchWord}%' or Description like '%{SearchWord}%'  LIMIT {PageSize} OFFSET ({PageNumber} - 1) * {PageSize}";
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
                    Price = reader.GetDecimal(6),
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
                    IsPromoted = reader.GetBoolean(26),
                    ProductServerId = reader.GetInt32(27),
                    FileUrl = reader.GetString(28),
                    ImageName = reader.GetString(29)
                });
            }

            return products;
        }
        public async Task<List<TblPromoPicturesSet>> GetSlotsFromAPICall()
        {
            string token = Preferences.Default.Get("token", "null");
            int customerId = Preferences.Default.Get("customerId", 0);

            var options = new RestClientOptions(API_URL)
            {
                // MaxTimeout = -1,
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true
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
                var promos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TblPromoPicturesSet>>(response.Content);
                if (promos != null)
                {
                    if (promos.Count > 0)
                    {
                        await CreateTablePromosLocally();
                        await InsertPromos(promos);
                    }
                    return promos;
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
          
                DROP TABLE IF EXISTS Products;
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
                           IsPromoted INTEGER NOT NULL,
                         ProductServerId INTEGER NOT NULL UNIQUE,
                         FileUrl TEXT,
                         ImageName TEXT
                    );
                    ";
          
                await createTableCmd.ExecuteNonQueryAsync();
                await connection.CloseAsync();
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
            await using var transaction = await connection.BeginTransactionAsync();
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
                       IsPromoted,
                       ProductServerId,
                       FileUrl,
                        ImageName
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
                       @IsPromoted,
                       @ProductServerId,
                       @FileUrl,
                       @ImageName
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
                   insertProductCommand.Parameters.AddWithValue("@ProductServerId", DbType.Int32).Value = product.Id;
                 insertProductCommand.Parameters.AddWithValue("@FileUrl", DbType.String).Value = "";
                 //       (object?)product.FileUrl ?? DBNull.Value;
                 insertProductCommand.Parameters.AddWithValue("@ImageName", DbType.String).Value =
                        (object?)product.ImageName ?? DBNull.Value;
                   await insertProductCommand.ExecuteNonQueryAsync();


            await transaction.CommitAsync();
        }

        public async Task DeleteProducts(string ids)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var insertProductCommand = connection.CreateCommand();
            // insertProductCommand.Transaction = transaction;

            insertProductCommand.CommandText = @$" Delete from Products where ProductServerId = {ids}";

            await insertProductCommand.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

        public async Task InsertProducts(List<ProductsWithQuantity> products)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();
            var imageMap = await DownloadImagesForProductsAsync(products);
            await using var transaction = await connection.BeginTransactionAsync();

            var insertProductCommand = connection.CreateCommand();
            // insertProductCommand.Transaction = transaction;

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
            IsPromoted,
            ProductServerId,
            FileUrl,
            ImageName
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
            @IsPromoted,
            @ProductServerId,
            @FileUrl,
            @ImageName
        );
    ";

            // Create parameters ONCE
            var pCode = insertProductCommand.Parameters.Add("@Code", SqliteType.Text);
            var pDescription = insertProductCommand.Parameters.Add("@Description", SqliteType.Text);
            var pQuantityOnHand = insertProductCommand.Parameters.Add("@QuantityOnHand", SqliteType.Real);
            var pHasImage = insertProductCommand.Parameters.Add("@HasImage", SqliteType.Integer);
            var pImage = insertProductCommand.Parameters.Add("@Image", SqliteType.Blob);
            var pPrice = insertProductCommand.Parameters.Add("@Price", SqliteType.Real);
            var pPriceIncl = insertProductCommand.Parameters.Add("@PriceIncl", SqliteType.Real);
            var pOnSpecial = insertProductCommand.Parameters.Add("@OnSpecial", SqliteType.Integer);
            var pSpecialPrice = insertProductCommand.Parameters.Add("@SpecialPrice", SqliteType.Real);
            var pTypicalOrderQuantity = insertProductCommand.Parameters.Add("@TypicalOrderQuantity", SqliteType.Real);
            var pTaxPercentage = insertProductCommand.Parameters.Add("@TaxPercentage", SqliteType.Real);
            var pUOM = insertProductCommand.Parameters.Add("@UOM", SqliteType.Text);
            var pCategory1 = insertProductCommand.Parameters.Add("@Category1", SqliteType.Text);
            var pCategory2 = insertProductCommand.Parameters.Add("@Category2", SqliteType.Text);
            var pCategory3 = insertProductCommand.Parameters.Add("@Category3", SqliteType.Text);
            var pCategory4 = insertProductCommand.Parameters.Add("@Category4", SqliteType.Text);
            var pCategory5 = insertProductCommand.Parameters.Add("@Category5", SqliteType.Text);
            var pCategory6 = insertProductCommand.Parameters.Add("@Category6", SqliteType.Text);
            var pCategory7 = insertProductCommand.Parameters.Add("@Category7", SqliteType.Text);
            var pCategory8 = insertProductCommand.Parameters.Add("@Category8", SqliteType.Text);
            var pIsFavoured = insertProductCommand.Parameters.Add("@isFavoured", SqliteType.Integer);
            var pInfoApproved = insertProductCommand.Parameters.Add("@InfoApproved", SqliteType.Integer);
            var pCategoryId = insertProductCommand.Parameters.Add("@CategoryId", SqliteType.Integer);
            var pCategory = insertProductCommand.Parameters.Add("@Category", SqliteType.Text);
            var pQuantity = insertProductCommand.Parameters.Add("@Quantity", SqliteType.Integer);
            var pIsPromoted = insertProductCommand.Parameters.Add("@IsPromoted", SqliteType.Integer);
            var pProductServerId = insertProductCommand.Parameters.Add("@ProductServerId", SqliteType.Integer);
            var pFileUrl = insertProductCommand.Parameters.Add("@FileUrl", SqliteType.Text);
            var pImageName = insertProductCommand.Parameters.Add("@ImageName", SqliteType.Text);
            //var pImageName  = await DownloadImageAsync(category.FileUrl ?? "", category.CategoryId + ".png");
            // Loop like InsertCategory
            foreach (var product in products)
            {
                pCode.Value = product.Code;
                pDescription.Value = product.Description;
                pQuantityOnHand.Value = product.QuantityOnHand;
                pHasImage.Value = product.HasImage ? 1 : 0;
                pImage.Value = (object?)product.Image ?? DBNull.Value;
                pPrice.Value = (object?)product.Price ?? DBNull.Value;
                pPriceIncl.Value = product.PriceIncl;
                pOnSpecial.Value = product.OnSpecial ? 1 : 0;
                pSpecialPrice.Value = (object?)product.SpecialPrice ?? DBNull.Value;
                pTypicalOrderQuantity.Value = (object?)product.TypicalOrderQuantity ?? DBNull.Value;
                pTaxPercentage.Value = product.TaxPercentage;
                pUOM.Value = (object?)product.UOM ?? DBNull.Value;
                pCategory1.Value = (object?)product.Category1 ?? DBNull.Value;
                pCategory2.Value = (object?)product.Category2 ?? DBNull.Value;
                pCategory3.Value = (object?)product.Category3 ?? DBNull.Value;
                pCategory4.Value = (object?)product.Category4 ?? DBNull.Value;
                pCategory5.Value = (object?)product.Category5 ?? DBNull.Value;
                pCategory6.Value = (object?)product.Category6 ?? DBNull.Value;
                pCategory7.Value = (object?)product.Category7 ?? DBNull.Value;
                pCategory8.Value = (object?)product.Category8 ?? DBNull.Value;
                pIsFavoured.Value = product.isFavoured ? 1 : 0;
                pInfoApproved.Value = product.InfoApproved ? 1 : 0;
                pCategoryId.Value = product.CategoryId;
                pCategory.Value = product.Category;
                pQuantity.Value = product.Quantity;
                pIsPromoted.Value = product.IsPromoted ? 1 : 0;
                pProductServerId.Value = product.Id;
                pFileUrl.Value = (object?)product.FileUrl ?? DBNull.Value;
                //pImageName.Value = (object?)product.ImageName ?? DBNull.Value;
                product.ProductServerId = product.Id;
                string pname = "product.png";
                try
                {
                    //pImageName.Value = await DownloadImageAsync(product.FileUrl!, product.ProductServerId + ".png"!);
                    if (imageMap.TryGetValue(product.Id, out var imagePath))
                        pImageName.Value = imagePath;
                    else
                        pImageName.Value = "product.png";
                }
                catch (Exception ex)
                {
                    pImageName.Value = pname;
                    Console.WriteLine(ex.Message);
                }
                //await DownloadImageAsync( pProductServerId + ".png"); //(object?)product.ImageName ?? "";

                await insertProductCommand.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
            await connection.CloseAsync();
        }
        public async Task UpdateProducts(List<ProductsWithQuantity> products)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            var insertProductCommand = connection.CreateCommand();
           // insertProductCommand.Transaction = transaction;

            insertProductCommand.CommandText = @"
       
                    Update Products SET
                        Price = @Price,
                        PriceInc = @PriceInc
                        Where ProductServerId = @ProductServerId ";
   

            // Create parameters ONCE
            var pPrice = insertProductCommand.Parameters.Add("@Price", SqliteType.Real);
            var pPriceIncl = insertProductCommand.Parameters.Add("@PriceInc", SqliteType.Real);
            var pProductServerId = insertProductCommand.Parameters.Add("@ProductServerId", SqliteType.Integer);
            foreach (var product in products)
            {
                pPrice.Value = (object?)product.Price ?? DBNull.Value;
                pPriceIncl.Value = product.PriceIncl;
                pProductServerId.Value = product.Id;

                await insertProductCommand.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
            await connection.CloseAsync();
        }
        public async Task CreateTableCategoriesLocally()
        {

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
                        DROP TABLE IF EXISTS Categories;
                        CREATE TABLE Categories (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            CategoryId INTEGER NULL,
                            CategoryName TEXT NULL,
                            FileUrl TEXT NULL
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

        public async Task CreateTablePromosLocally()
        {

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @"
                        DROP TABLE IF EXISTS Promos;
                        CREATE TABLE Promos (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Url TEXT NULL
                        );
                    ";

                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating Promos table");
                throw;
            }

        }
        public async Task InsertCategory(List<Category> categories)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();
            var imageMap = await DownloadImagesForCategoriesAsync(categories);
            await using var transaction = await connection.BeginTransactionAsync();
            var insertCategoryCommand = connection.CreateCommand();
            insertCategoryCommand.CommandText = @"
                    
                    INSERT INTO Categories (
                        CategoryId,
                        CategoryName,
                        FileUrl
                    )
                    VALUES (
                        @CategoryId,
                        @CategoryName,
                        @FileUrl
                    );
                ";


            // await insertCategoryCommand.ExecuteNonQueryAsync();
            var paramId = insertCategoryCommand.Parameters.Add("@CategoryId", SqliteType.Integer);
            var paramName = insertCategoryCommand.Parameters.Add("@CategoryName", SqliteType.Text);
            var paramFileUrl = insertCategoryCommand.Parameters.Add("@FileUrl", SqliteType.Text);

            foreach (var category in categories)
            {
                paramId.Value = category.CategoryId;
                paramName.Value = category.CategoryName;
                //paramFileUrl.Value = category.FileUrl;// await DownloadImageAsync(category.FileUrl ?? "", category.CategoryId + ".png");
                //paramFileUrl.Value = await DownloadImageAsync(category.FileUrl ?? "", category.CategoryId + ".png"); ;
                try
                {
                    var fileurl = category.FileUrl;
                    if (imageMap.TryGetValue(category.CategoryId, out var imagePath))
                        paramFileUrl.Value = imagePath;
                    else
                        paramFileUrl.Value = "category.png";
                    // paramFileUrl.Value = await DownloadImageAsync(fileurl!, "cat_id_" + category.CategoryId.ToString() + ".png");
                    // pname = await DownloadImageAsync(product.FileUrl!, product.ImageName!);
                    //   x.FileUrl = pname;
                }
                catch (Exception ex)
                {
                    paramFileUrl.Value = "category.png";
                    category.FileUrl = "category.png";
                    Console.WriteLine(ex.Message);
                }
                await insertCategoryCommand.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }

        public async Task InsertPromos(List<TblPromoPicturesSet> promos)
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();
            var imageMap = await DownloadVideosAsync(promos);
            await using var transaction = await connection.BeginTransactionAsync();
            var insertCategoryCommand = connection.CreateCommand();
            insertCategoryCommand.CommandText = @"
                    
                    INSERT INTO Promos(
                        Url
                    )
                    VALUES (
                        @Url
                    );
                ";

            var Url = insertCategoryCommand.Parameters.Add("@Url", SqliteType.Text);
            foreach (var promo in promos)
            {
                try
                {
                    if (imageMap.TryGetValue(promo.Id, out var imagePath))
                        Url.Value = imagePath;// promo.Url;
                    else
                        Url.Value = "category.mp4";
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine(ex.Message);
                }
                await insertCategoryCommand.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }

        public async Task<List<ProductsWithQuantity>> GetProductsByFavorites()
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Products where isFavoured = 1 ORDER BY Description ASC";
            var products = new List<ProductsWithQuantity>();

            await using (var reader = await selectCmd.ExecuteReaderAsync())
            {
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
                        Price = reader.GetDecimal(6),
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
                        IsPromoted = reader.GetBoolean(26),
                        ProductServerId = reader.GetInt32(27),
                        FileUrl = reader.GetString(28),
                        ImageName = reader.GetString(29)
                    });
                }

            }
            if (products.Count == 0)
            {
                selectCmd.CommandText = "SELECT * FROM Products LIMIT 2";
                 products = new List<ProductsWithQuantity>();

                await using var reader1 = await selectCmd.ExecuteReaderAsync();
                while (await reader1.ReadAsync())
                {
                    products.Add(new ProductsWithQuantity
                    {
                        Id = reader1.GetInt32(0),
                        Code = reader1.GetString(1),
                        Description = reader1.GetString(2),
                        QuantityOnHand = reader1.GetDecimal(3),
                        HasImage = reader1.GetBoolean(4),
                        Image = reader1.IsDBNull(5) ? null : (byte[])reader1[5],
                        Price = reader1.GetDecimal(6),
                        PriceIncl = reader1.GetDecimal(7),
                        OnSpecial = reader1.GetBoolean(8),
                        SpecialPrice = reader1.IsDBNull(9) ? null : reader1.GetDecimal(9),
                        TypicalOrderQuantity = reader1.IsDBNull(10) ? null : reader1.GetDecimal(10),
                        TaxPercentage = reader1.GetDecimal(11),
                        UOM = reader1.IsDBNull(12) ? null : reader1.GetString(12),

                        Category1 = reader1.IsDBNull(13) ? null : reader1.GetString(13),
                        Category2 = reader1.IsDBNull(14) ? null : reader1.GetString(14),
                        Category3 = reader1.IsDBNull(15) ? null : reader1.GetString(15),
                        Category4 = reader1.IsDBNull(16) ? null : reader1.GetString(16),
                        Category5 = reader1.IsDBNull(17) ? null : reader1.GetString(17),
                        Category6 = reader1.IsDBNull(18) ? null : reader1.GetString(18),
                        Category7 = reader1.IsDBNull(19) ? null : reader1.GetString(19),
                        Category8 = reader1.IsDBNull(20) ? null : reader1.GetString(20),

                        isFavoured = reader1.GetBoolean(21),
                        InfoApproved = reader1.GetBoolean(22),
                        CategoryId = reader1.GetInt32(23),
                        Category = reader1.GetString(24),
                        Quantity = reader1.GetInt32(25),
                        IsPromoted = reader1.GetBoolean(26),
                        ProductServerId = reader1.GetInt32(27),
                        FileUrl = reader1.GetString(28),
                        ImageName = reader1.GetString(29)
                    });
                }

            }

            return products;
        }

        public async Task<List<ProductsWithQuantity>> GetProductsAside()
        {
            string token = Preferences.Default.Get("token", "null");
            int customerId = Preferences.Default.Get("customerId", 0);
            var typicalproducts = new List<TblTypicalOrdersSet>();    
            var options = new RestClientOptions(API_URL)
            {
                // MaxTimeout = -1,
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true
            };
            var client = new RestClient(options);
            if (customerId == 0)
            {
                return new List<ProductsWithQuantity>();
            }
            var deliveryDate = DateTime.Now.ToString("yyyy-MM-dd");
            var request = new RestRequest($"/api/Products/GetAllTypicalProducts?CustomerId={customerId}", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            if (response.Content != null)
            {
                 typicalproducts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TblTypicalOrdersSet>>(response.Content);
            }

            if (typicalproducts == null || typicalproducts.Count == 0)
            {
                return new List<ProductsWithQuantity>();
            }
            else
            {
                // Sync to IndexedDB
                var Suggestion = new List<ProductsWithQuantity>();    
                var cartItems = await cartRepository.GetCartData();
                var allProducts = Suggestion;// await GetProductsLocally();
                
                    var typicalProductIds = typicalproducts.Select(t => t.TblTypicalOrdersTblProducts).ToHashSet();
                    var typicalProductModels = allProducts.Where(p => typicalProductIds.Contains(p.Id)).ToList();

                    foreach (var product in typicalProductModels)
                    {
                        Suggestion.Add(product);
                    }

                    Console.WriteLine("SuggCount1" + Suggestion.Count());

                    var suggestionsNotInCart = typicalProductModels
                        .Where(p => !cartItems.Any(c => c.ProductServerId == p.Id))
                        .ToList();
                    Console.WriteLine("Suggestions" + suggestionsNotInCart.Count);
                    Console.WriteLine("SuggCount2" + Suggestion.Count());
                    return suggestionsNotInCart;
            }
        }


        private async Task<Dictionary<int, string>> DownloadImagesForProductsAsync(List<ProductsWithQuantity> products)
        {
            var semaphore = new SemaphoreSlim(5);

            var tasks = products
                .Where(p => !string.IsNullOrEmpty(p.FileUrl))
                .Select(async product =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var fileName = product.Id + ".png";
                        var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                        var bytes = await _httpClient.GetByteArrayAsync(product.FileUrl);
                        await File.WriteAllBytesAsync(localPath, bytes);

                        return new KeyValuePair<int, string>(product.Id, localPath);
                    }
                    catch
                    {
                        return new KeyValuePair<int, string>(product.Id, "product.png");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

            var results = await Task.WhenAll(tasks);

            return results.ToDictionary(x => x.Key, x => x.Value);
        }

        private async Task<Dictionary<int, string>> DownloadImagesForCategoriesAsync(List<Category> categories)
        {
            var semaphore = new SemaphoreSlim(5);

            var tasks = categories
                .Where(c => !string.IsNullOrEmpty(c.FileUrl))
                .Select(async category =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var fileName = $"category_{category.CategoryId}.png";
                        var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                        var bytes = await _httpClient.GetByteArrayAsync(category.FileUrl);
                        await File.WriteAllBytesAsync(localPath, bytes);

                        return new KeyValuePair<int, string>(category.CategoryId, localPath);
                    }
                    catch
                    {
                        return new KeyValuePair<int, string>(category.CategoryId, "category.png");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

            var results = await Task.WhenAll(tasks);

            return results.ToDictionary(x => x.Key, x => x.Value);
        }


        public async Task<bool> FetchNewlyAddedProductsAndPriceUpdates()
        {
            var productids = new List<ProductsWithQuantity>();// await ProductServerIds();
            try
            {
                 productids = await GetProductsLocally(2000, 1);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if(productids.Count > 0)
            {
                string token = Preferences.Default.Get("token", "null");
                int customerId = Preferences.Default.Get("customerId", 0);

                var options = new RestClientOptions(API_URL)
                {
                    // MaxTimeout = -1,
                    RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true
                };
                var client = new RestClient(options);
                if (customerId == 0)
                {
                    return false;
                }
                var deliveryDate = DateTime.Now.ToString("yyyy-MM-dd");
                var request = new RestRequest($"/api/ProductsMobile/GetPriceChangedProductsAndNew?CustomerId={customerId}&DeliveryDate={deliveryDate}", Method.Post);
                var body = Newtonsoft.Json.JsonConvert.SerializeObject(productids);
                request.AddStringBody(body, DataFormat.Json);
                RestResponse response = await client.ExecuteAsync(request);
                Console.WriteLine(response.Content);
                if (response.Content != null)
                {
                    var products = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductsWithQuantity>>(response.Content);

                    if (products != null)
                    {
                        if (products.Count > 0)
                        {
                            var newproducts = products.Where(x => x.Status == "New" && x.ProductServerId == 0).ToList();
                            if(newproducts.Count > 0)
                            {
                                await InsertProducts(newproducts);
                            }
                            var deletedproducts = products.Where(x => x.Status == "Deleted" && x.ProductServerId > 0).ToList();
                            if (deletedproducts.Count > 0)
                            {
                                var Idslist = "";
                                foreach (var id in deletedproducts)
                                {
                                    Idslist = Idslist + id.ProductServerId + " or ";
                                }
                                Idslist = Idslist.Trim();
                                Idslist = Idslist.Substring(0, Idslist.Length - 2);
                                await DeleteProducts(Idslist);
                            }
                            var updatedproducts = products.Where(x => x.Status == "Price Updated" && x.ProductServerId > 0).ToList();
                            if (updatedproducts.Count > 0)
                            {
                                await UpdateProducts(updatedproducts);
                            }
                        }
                        return true;
                    }
                }
            }
          
            return false;
        }


        public async Task<List<ProductsWithQuantity>> ProductServerIds()
        {
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = $"SELECT * FROM Products;";
            var products = new List<ProductsWithQuantity>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            products.Add(new ProductsWithQuantity
            {
                Id = reader.GetInt32(0),
                Code = reader.GetString(1),
                Description = reader.GetString(2),
                QuantityOnHand = reader.GetDecimal(3),
                HasImage = reader.GetBoolean(4),
                Image = reader.IsDBNull(5) ? null : (byte[])reader[5],
                Price = reader.GetDecimal(6),
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
                IsPromoted = reader.GetBoolean(26),
                ProductServerId = reader.GetInt32(27),
                FileUrl = reader.GetString(28),
                ImageName = reader.GetString(29)
            });

            return products;
        }

        private async Task<Dictionary<int, string>> DownloadVideosAsync(List<TblPromoPicturesSet> promos)
        {
            var semaphore = new SemaphoreSlim(3); // limit concurrency for large files

            var tasks = promos
                .Where(c => c.IsVideo == true)
                .Select(async promo =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        promo.Url = API_URL + "/Video/Proban" + promo.SlotNo + ".mp4";
                        var fileName = $"promo_{promo.SlotNo}.mp4";
                        var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                        using var response = await _httpClient.GetAsync(promo.Url, HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();

                        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                        var canReportProgress = totalBytes != -1;

                        await using var contentStream = await response.Content.ReadAsStreamAsync();
                        await using var fileStream = File.Create(localPath);

                        var buffer = new byte[81920];
                        long totalRead = 0;
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                            totalRead += bytesRead;

                            if (canReportProgress)
                            {
                                double progress = (double)totalRead / totalBytes * 100;
                                Console.WriteLine($"promo {promo.Id}: {progress:F1}% downloaded");
                                // Optionally, update a UI progress bar
                            }
                        }

                        return new KeyValuePair<int, string>(promo.Id, localPath);
                    }
                    catch
                    {
                        // fallback file in case download fails
                        return new KeyValuePair<int, string>(promo.Id, "default_video.mp4");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

            var results = await Task.WhenAll(tasks);
            return results.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
