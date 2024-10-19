using Concessionaire.WebAPI.Entities;
using Concessionaire.WebAPI.Enums;
using Concessionaire.WebAPI.Requests;
using Concessionaire.WebAPI.Services;
using System.Data;
using Dapper;

namespace Concessionaire.WebAPI.Repositories
{
    public class CarsRepository : ICarsRepository
    {
        private readonly IAzureStorageService azureStorageService;
        private readonly IDbConnection _dbConnection;

        public CarsRepository(IAzureStorageService azureStorageService, IDbConnection dbConnection)
        {
            this.azureStorageService = azureStorageService;
            _dbConnection = dbConnection;
        }

        public async Task<Cars> AddAsync(CarRequest request)
        {
            string baseUrl = "https://demoiesisaalmacenamiento.blob.core.windows.net/imagenesiesisa/";
            var car = new Cars()
            {
                Brand = request.Brand,
                Model = request.Model,
                Year = request.Year
            };

            if (request.Image != null)
            {
                car.ImagePath = await this.azureStorageService.UploadAsync(request.Image, ContainerEnum.IMAGENESIESISA);
            }

            //if (request.TechnicalDataSheet != null)
            //{
            //    car.TechnicalDataSheetPath = await this.azureStorageService.UploadAsync(request.TechnicalDataSheet, ContainerEnum.DOCUMENTOSIESISA);
            //}

            // Usando el SP para agregar un carro
            var parameters = new DynamicParameters();
            parameters.Add("Brand", car.Brand);
            parameters.Add("Model", car.Model);
            parameters.Add("Year", car.Year);
            parameters.Add("ImagePath", baseUrl + car.ImagePath);
            parameters.Add("TechnicalDataSheetPath", car.TechnicalDataSheetPath);

            var insertedCar = await _dbConnection.QuerySingleAsync<Cars>("sp_AddCar", parameters, commandType: CommandType.StoredProcedure);

            return insertedCar;
        }

        public async Task<IEnumerable<Cars>> GetAllAsync()
        {
            string baseUrl = "https://demoiesisaalmacenamiento.blob.core.windows.net/imagenesiesisa/";

            // Usando el SP para obtener todos los carros
            var cars = await _dbConnection.QueryAsync<Cars>("sp_GetAllCars", commandType: CommandType.StoredProcedure);

            foreach (var car in cars)
            {
                car.ImagePath = $"{baseUrl}{car.ImagePath}";
            }

            return cars;
        }

        public async Task<Cars> GetByIdAsync(int id)
        {
            string baseUrl = "https://demoiesisaalmacenamiento.blob.core.windows.net/imagenesiesisa/";

            // Usando el SP para obtener un carro por ID
            var car = await _dbConnection.QuerySingleOrDefaultAsync<Cars>("sp_GetCarById", new { Id = id }, commandType: CommandType.StoredProcedure);

            if (car != null)
            {
                car.ImagePath = $"{baseUrl}{car.ImagePath}";
            }

            return car;
        }

        public async Task RemoveByIdAsync(int id)
        {
            var car = await GetByIdAsync(id);

            if (car != null)
            {
                if (!string.IsNullOrEmpty(car.ImagePath))
                {
                    await this.azureStorageService.DeleteAsync(ContainerEnum.IMAGENESIESISA, car.ImagePath);
                }

                if (!string.IsNullOrEmpty(car.TechnicalDataSheetPath))
                {
                    await this.azureStorageService.DeleteAsync(ContainerEnum.DOCUMENTOSIESISA, car.TechnicalDataSheetPath);
                }

                // Usando el SP para eliminar un carro por ID
                await _dbConnection.ExecuteAsync("sp_RemoveCarById", new { Id = id }, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<Cars> UpdateAsync(int id, CarRequest request)
        {
            var car = await GetByIdAsync(id);

            if (car != null)
            {
                car.Brand = request.Brand;
                car.Model = request.Model;
                car.Year = request.Year;

                if (request.Image != null)
                {
                    car.ImagePath = await this.azureStorageService.UploadAsync(request.Image, ContainerEnum.IMAGENESIESISA, car.ImagePath);
                }

                //if (request.TechnicalDataSheet != null)
                //{
                //    car.TechnicalDataSheetPath = await this.azureStorageService.UploadAsync(request.TechnicalDataSheet, ContainerEnum.DOCUMENTOSIESISA, car.TechnicalDataSheetPath);
                //}

                // Usando el SP para actualizar un carro
                var parameters = new DynamicParameters();
                parameters.Add("Id", id);
                parameters.Add("Brand", car.Brand);
                parameters.Add("Model", car.Model);
                parameters.Add("Year", car.Year);
                parameters.Add("ImagePath", car.ImagePath);
                parameters.Add("TechnicalDataSheetPath", car.TechnicalDataSheetPath);

                await _dbConnection.ExecuteAsync("sp_UpdateCar", parameters, commandType: CommandType.StoredProcedure);
            }

            return car;
        }
    }

    public interface ICarsRepository
    {
        Task<IEnumerable<Cars>> GetAllAsync();
        Task<Cars> GetByIdAsync(int id);
        Task<Cars> AddAsync(CarRequest request);
        Task<Cars> UpdateAsync(int id, CarRequest request);
        Task RemoveByIdAsync(int id);
    }
}
