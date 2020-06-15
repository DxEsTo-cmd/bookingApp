using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookingApp.Services.Interfaces;
using BookingApp.Entities.Statistics;
using BookingApp.DTOs.Statistics;
using BookingApp.Helpers;
using Microsoft.AspNetCore.Authorization;
using BookingApp.Controllers.Bases;
using BookingApp.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.AspNetCore.StaticFiles;

namespace BookingApp.Controllers
{
    [Route("api/stats")]
    [ApiController]
    [Authorize(Roles = RoleTypes.Admin)]
    public class StatisticsController : EntityControllerBase
    {
        readonly IStatisticsService statisticsService;
        readonly IMapperService dtoMapper;
        readonly ExcelService excelService;
        
        public StatisticsController(IStatisticsService statisticsService, IMapperService mapperService)
        {
            this.statisticsService = statisticsService;
            dtoMapper = mapperService;
            excelService = new ExcelService();
        }

        [HttpGet("bookings-creations")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BookingsCreations([FromQuery] DateTime? startTime,[FromQuery] DateTime? endTime,[FromQuery] string interval,[FromQuery] int[] rID)
        {
            DateTime start = startTime ?? DateTime.Now.AddDays(-7);
            DateTime end = endTime ?? DateTime.Now;
            BookingsStats stats = await statisticsService.GetBookingsCreations(start, end, interval, rID);
            BookingStatsDTO dto = dtoMapper.Map<BookingStatsDTO>(stats);
            return Ok(dto);
        }

        [HttpGet("bookings-creations/download")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BookingsCreationsDownload([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime, [FromQuery] string interval, [FromQuery] int[] rID)
        {
            DateTime start = startTime ?? DateTime.Now.AddDays(-7);
            DateTime end = endTime ?? DateTime.Now;
            BookingsStats stats = await statisticsService.GetBookingsCompletions(start, end, interval, rID);

            var memory = await excelService.CreateAndSaveExcelFile(stats);
            return File(memory, GetContentType("../../file.xls"), "../../file.xls");
        }

        [HttpGet("bookings-cancellations")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BookingsCancellations([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime, [FromQuery] string interval, [FromQuery] int[] rID)
        {
            DateTime start = startTime ?? DateTime.Now.AddDays(-7);
            DateTime end = endTime ?? DateTime.Now;
            BookingsStats stats = await statisticsService.GetBookingsCancellations(start, end, interval, rID);
            BookingStatsDTO dto = dtoMapper.Map<BookingStatsDTO>(stats);
            return Ok(dto);
        }

        [HttpGet("bookings-cancellations/download")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BookingsCancellationsDownload([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime, [FromQuery] string interval, [FromQuery] int[] rID)
        {
            DateTime start = startTime ?? DateTime.Now.AddDays(-7);
            DateTime end = endTime ?? DateTime.Now;
            BookingsStats stats = await statisticsService.GetBookingsCompletions(start, end, interval, rID);

            var memory = await excelService.CreateAndSaveExcelFile(stats);
            return File(memory, GetContentType("../../file.xls"), "../../file.xls");
        }

        [HttpGet("bookings-terminations")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BookingsTerminations([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime, [FromQuery] string interval, [FromQuery] int[] rID)
        {
            DateTime start = startTime ?? DateTime.Now.AddDays(-7);
            DateTime end = endTime ?? DateTime.Now;
            BookingsStats stats = await statisticsService.GetBookingsTerminations(start, end, interval, rID);
            BookingStatsDTO dto = dtoMapper.Map<BookingStatsDTO>(stats);
            return Ok(dto);
        }

        [HttpGet("bookings-terminations/download")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BookingsTerminationsDownload([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime, [FromQuery] string interval, [FromQuery] int[] rID)
        {
            DateTime start = startTime ?? DateTime.Now.AddDays(-7);
            DateTime end = endTime ?? DateTime.Now;
            BookingsStats stats = await statisticsService.GetBookingsCompletions(start, end, interval, rID);

            var memory = await excelService.CreateAndSaveExcelFile(stats);
            return File(memory, GetContentType("../../file.xls"), "../../file.xls");
        }

        [HttpGet("bookings-completions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BookingsCompletions([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime, [FromQuery] string interval, [FromQuery] int[] rID)
        {
            DateTime start = startTime ?? DateTime.Now.AddDays(-7);
            DateTime end = endTime ?? DateTime.Now;
            BookingsStats stats = await statisticsService.GetBookingsCompletions(start, end, interval, rID);
            BookingStatsDTO dto = dtoMapper.Map<BookingStatsDTO>(stats);
            return Ok(dto);
        }

        [HttpGet("bookings-completions/download")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> BookingsCompletionsDownload([FromQuery] DateTime? startTime, [FromQuery] DateTime? endTime, [FromQuery] string interval, [FromQuery] int[] rID)
        {
            DateTime start = startTime ?? DateTime.Now.AddDays(-7);
            DateTime end = endTime ?? DateTime.Now;
            BookingsStats stats = await statisticsService.GetBookingsCompletions(start, end, interval, rID);
            
            var memory = await excelService.CreateAndSaveExcelFile(stats);
            return File(memory, GetContentType("../../file.xls"), "../../file.xls");
        }
        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
        //[HttpGet]
        //[Route("booking/download")]
        //public async Task<IActionResult> Download([FromQuery] string file)
        //{
        //    var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
        //    var filePath = Path.Combine(uploads, file);
        //    if (!System.IO.File.Exists(filePath))
        //        return NotFound();

        //    var memory = new MemoryStream();
        //    using (var stream = new FileStream(filePath, FileMode.Open))
        //    {
        //        await stream.CopyToAsync(memory);
        //    }
        //    memory.Position = 0;
        //    excelService.CreateAndSaveExcelFile()

        //    return File(memory, GetContentType(filePath), file);
        //}


        [HttpGet("resources")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]        
        public async Task<IActionResult> ResourcesUsage()
        {
            IEnumerable<ResourceStatsDTO> dTOs;

            IEnumerable<ResourceStats> resourceStats = await statisticsService.GetResourceStats();

            dTOs = dtoMapper.Map<IEnumerable<ResourceStatsDTO>>(resourceStats);

            return Ok(dTOs);
        }


        [HttpGet("resources-rating")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ResourcesRating()
        {
            IEnumerable<ResourceStatsBriefDTO> dTOs;

            IEnumerable<ResourceStats> resourceStats = await statisticsService.GetResourcesRating();

            dTOs = dtoMapper.Map<IEnumerable<ResourceStatsBriefDTO>>(resourceStats);

            return Ok(dTOs);
        }

        [HttpGet("resources/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ResourceUsage([FromRoute] int id)
        {   
            ResourceStats resourceStats = await statisticsService.GetResourceStats(id);

            ResourceStatsDTO dTO = dtoMapper.Map<ResourceStatsDTO>(resourceStats);

            return Ok(dTO);
        }

        [HttpGet("users")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UsersStats()
        {
            UsersStatsDTO dTO;

            UsersStats userStats = await statisticsService.GetUsersStats();

            dTO = dtoMapper.Map<UsersStatsDTO>(userStats);

            return Ok(dTO);
        }
    }
}