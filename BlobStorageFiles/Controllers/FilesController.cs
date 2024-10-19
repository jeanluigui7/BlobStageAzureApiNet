using Concessionaire.WebAPI.Repositories;
using Concessionaire.WebAPI.Requests;
using Microsoft.AspNetCore.Mvc;

namespace BlobStorageFiles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ICarsRepository carsRepository;

        public FilesController(ICarsRepository carsRepository)
        {
            this.carsRepository = carsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cars = await this.carsRepository.GetAllAsync();
            return Ok(cars);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var car = await this.carsRepository.GetByIdAsync(id);
            if (car != null)
            {
                return Ok(car);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CarRequest request)
        {
            return Ok(await this.carsRepository.AddAsync(request));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromForm] CarRequest request)
        {
            return Ok(await this.carsRepository.UpdateAsync(id, request));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            await this.carsRepository.RemoveByIdAsync(id);
            return NoContent();
        }
    }
}
