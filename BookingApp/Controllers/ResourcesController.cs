using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookingApp.Data.Models;
using BookingApp.Services.Interfaces;
using AutoMapper;
using BookingApp.DTOs;
using BookingApp.Exceptions;
using BookingApp.Helpers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using BookingApp.Services;
using BookingApp.Controllers.Bases;
using BookingApp.DTOs.Resource;

namespace BookingApp.Controllers
{
    [Route("api/resources")]
    [ApiController]
    public partial class ResourcesController : EntityControllerBase
    {
        readonly IResourcesService resService;
        readonly IBookingsService bookService;
        readonly IMapperService mappService;

        public ResourcesController(IResourcesService resService, IBookingsService bookService, IMapperService mappService)
        {
            this.resService = resService;
            this.bookService = bookService;
            this.mappService = mappService;
        }

        #region GETs
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        // Filtered access: Guest/Admin. 
        public async Task<IActionResult> List()
        {
            var models = IsAdmin ? await resService.ListResourcesWithImage(false) : await resService.ListResourcesWithImage(true);
            var dtos = mappService.Map<IEnumerable<ResourceBriefDto>>(models);
            foreach (var item in dtos)
            {
                var images = models.Where(x => x.Id == item.Id).FirstOrDefault().Image
                    .Select(x => x.ImagePath).AsEnumerable();

                item.Image = images;
            }
            return Ok(dtos);
        }

        [HttpGet("occupancy")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        // Filtered access: Guest/Admin.
        public async Task<IActionResult> ListOccupancy()
        {
            var result = await (IsAdmin ? resService.GetOccupancies() : resService.GetActiveOccupancies());
            return Ok(result);
        }

        [HttpGet("{resourceId}/bookings")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        // Filtered access: Guest/Owner/Admin.
        public async Task<IActionResult> ListRelatedBookings([FromRoute] int resourceId, [FromQuery]DateTime? startTime, [FromQuery]DateTime? endTime)
        {
            await AuthorizeForSingleResource(resourceId);
            DateTime realStartTime;
            if (IsAdmin)
                realStartTime = startTime ?? DateTime.Now;
            else
            {
                realStartTime = startTime ?? DateTime.Now;
                realStartTime = (realStartTime < DateTime.Now) ? DateTime.Now : realStartTime;
            }
            var models = await bookService.ListBookingOfResource(resourceId, realStartTime, endTime ?? DateTime.MaxValue);

            IEnumerable<BookingMinimalDTO> dtos;

            if (IsAdmin)
            {
                dtos = mappService.Map<IEnumerable<BookingAdminDTO>>(models);
            }
            else
            {
                if (IsUser && models.Any(b => b.CreatedUserId == UserId))
                {
                    var diffList = new List<BookingMinimalDTO>();
                    var currentUserId = UserId;
                    foreach (var model in models)
                    {
                        BookingMinimalDTO suitableDto;

                        if (model.CreatedUserId == currentUserId)
                            suitableDto = mappService.Map<BookingOwnerDTO>(model);
                        else
                            suitableDto = mappService.Map<BookingMinimalDTO>(model);

                        diffList.Add(suitableDto);
                    }
                    dtos = diffList;
                }
                else
                {
                    dtos = mappService.Map<IEnumerable<BookingMinimalDTO>>(models);
                }
            }
            return Ok(dtos);
        }

        [HttpGet("{resourceId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        // Filtered access: Guest/Admin.
        public async Task<IActionResult> Single([FromRoute] int resourceId)
        {
            await AuthorizeForSingleResource(resourceId);
            var resourceModel = await resService.Get(resourceId);

            var images = resourceModel.Image.Select(x => x.ImagePath).AsEnumerable();

            var resourceDTO = mappService.Map<ResourceMaxDto>(resourceModel);
            resourceDTO.Image = images;
           
            return Ok(resourceDTO);
        }

        [HttpGet("{resourceId}/occupancy")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        // Filtered access: Guest/Admin.
        public async Task<IActionResult> SingleOccupancy([FromRoute] int resourceId)
        {
            await AuthorizeForSingleResource(resourceId);
            return Ok(await resService.OccupancyByResource(resourceId));
        }
        #endregion

        #region POST / PUT
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = RoleTypes.Admin)]
        public async Task<IActionResult> Create([FromBody] ResourceDetailedDto item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            #region Mapping
            var itemModel = mappService.Map<Resource>(item);
            itemModel.UpdatedUserId = itemModel.CreatedUserId = UserId;
            itemModel.UpdatedTime = itemModel.CreatedTime = DateTime.Now;
            #endregion

            await resService.Create(itemModel);

            return Created(
                this.BaseApiUrl + "/" + itemModel.Id,
                new { ResourceId = itemModel.Id, itemModel.CreatedTime }
            );
        }

        [HttpPost("marker")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = RoleTypes.Admin)]
        public async Task<IActionResult> AddMarkers([FromBody] ResourceMarkerDto resourceMarkerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resource = await resService.Get(resourceMarkerDto.Id);

            if(resource.Id != 0)
            {
                resource.Coordinates = resourceMarkerDto.Coordinates;
            }

            await resService.Update(resource);

            return Ok();   
        }

        /// <summary>
        /// Update Image
        /// <summary>
        /// <response code="200">Success</response>
        /// <response code="404">Not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost, DisableRequestSizeLimit]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("update-image/{id}")]
        [Authorize(Roles = RoleTypes.Admin)]
        public async Task<IActionResult> UpdateImage(int id)
        {
            try
            {
                var resource = await resService.Get(id);
                var imagePath = await resService.SaveResourceImage(Request.Form.Files[0]);
                resource.Image.Add(new Image { ImagePath = imagePath });
                await resService.Update(resource);
            }
            catch (InvalidOperationException)
            {
                return Ok("Smae image");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex} Message {ex.Message}");
            }
            return Ok();
        }

        /// <summary>
        /// Update Already Uploaded Image 
        /// <summary>
        /// <response code="200">Success</response>
        /// <response code="404">Not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost, DisableRequestSizeLimit]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Route("update-image/already-upload/{id}")]
        [Authorize(Roles = RoleTypes.Admin)]
        public async Task<IActionResult> UpdateAlreadyUpdatedImage(int id, [FromBody] IEnumerable<string> images)
        {
            try
            {
                var resource = await resService.Get(id);
                var imagesForDelete = resource.Image.Select(x => x.ImagePath).Except(images).ToList();
                var newImages = resource.Image.Where(x => !imagesForDelete.Contains(x.ImagePath)).ToList();

                resource.Image = newImages;
                await resService.Update(resource);
            }
            catch (InvalidOperationException)
            {
                return Ok("Some image");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex} Message {ex.Message}");
            }
            return Ok();
        }

        [HttpPut("{resourceId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = RoleTypes.Admin)]
        public async Task<IActionResult> Update([FromRoute] int resourceId, [FromBody] ResourceDetailedDto item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            #region Mapping
            var itemModel = mappService.Map<Resource>(item);
            itemModel.UpdatedUserId = UserId;
            itemModel.UpdatedTime = DateTime.Now;
            itemModel.Id = resourceId;
            #endregion
            await resService.Update(itemModel);
            return Ok(new { itemModel.UpdatedTime });
        }
        #endregion

        #region DELETE
        [HttpDelete("{resourceId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = RoleTypes.Admin)]
        public async Task<IActionResult> Delete([FromRoute] int resourceId)
        {
            await resService.Delete(resourceId);
            return Ok(new { DeletedTime = DateTime.Now });
        }

        [HttpDelete("coordinates/{resourceId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = RoleTypes.Admin)]
        public async Task<IActionResult> DeleteAllRelatedCoordinates([FromRoute] int resourceId)
        {
            await resService.DeleteAllRelatedCoordinates(resourceId);
            return Ok();
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Not found exception factory
        /// </summary>
        CurrentEntryNotFoundException NewNotFoundException => new CurrentEntryNotFoundException("Specified resource not found");

        /// <summary>
        /// Gateway for the single resource. 
        /// Throws Not Found if current user hasn't enough rights for viewing the specified resource.
        /// Throws Not Found if current resource not found.
        /// </summary>
        [NonAction]
        public async Task AuthorizeForSingleResource(int resourceId)
        {
            bool isAuthorized = IsAdmin || await resService.IsActive(resourceId);

            if (!isAuthorized)
                throw NewNotFoundException;// Excuse
        }
        #endregion
    }
}