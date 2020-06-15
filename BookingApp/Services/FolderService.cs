using BookingApp.Data.Models;
using BookingApp.Repositories.Interfaces;
using BookingApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace BookingApp.Services
{
    public class FolderService : IFolderService
    {
        IFolderRepository repository;
        private readonly string fulldPathToFolderImage = "../BookingApp/ClientApp/src/assets/resources/Folders/";
        private readonly string shortPathToFilderImage = "/assets/resources/Folders/";

        public FolderService(IFolderRepository r)
        { 
            repository = r;
        }

        public async Task<IEnumerable<Folder>> GetFoldersActive()
        {
            return await repository.ListActiveAsync();
        }

        public async Task<IEnumerable<Folder>> GetFolders()
        {
            return await repository.ListActiveAsync();
        }

        public async Task<Folder> GetDetail(int id)
        {
            return await repository.GetAsync(id);
        }

        public async Task Create(string userId, Folder Folder)
        {
            Folder.UpdatedUserId = Folder.CreatedUserId = userId;
            await repository.CreateAsync(Folder);
        }

        public async Task Update(int currentFolderId, string userId, Folder Folder)
        {
            if (Folder.ParentFolderId != null)
                await repository.IsParentValidAsync(Folder.ParentFolderId, currentFolderId);    

            Folder.Id = currentFolderId;
            Folder.UpdatedUserId = userId;
            Folder.UpdatedTime = DateTime.Now;
            await repository.UpdateAsync(Folder);
        }

        public async Task Delete(int id) 
        {
            await repository.DeleteAsync(id);
        } 

        public async Task<string> SaveFolderImage(IFormFile file)
        {
            if (file.Length > 0)
            {
                var fullPath = Path.Combine(fulldPathToFolderImage, file.FileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return await Task.FromResult(Path.Combine(shortPathToFilderImage, file.FileName));
            }
            else
            {
                throw new Exception("File can't be created");
            }
        }
    }
}
