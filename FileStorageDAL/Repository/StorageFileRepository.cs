﻿using FileStorageDAL.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileStorageDAL.Repository
{
    public class StorageFileRepository : RepositoryBase<StorageFile>, IStorageFileRepository
    {
        private readonly new FileStorageDbContext _context;
        public StorageFileRepository(FileStorageDbContext context) : base(context)
        {
            _context = context;
        }

        public void DeleteFile(int id)
        {
            var file = _context.StorageFiles.FirstOrDefault(x => x.Id == id);
            if (file != null)
            {
                _context.StorageFiles.Remove(file);
                File.Delete(file.RelativePath);
            }
            _context.SaveChanges();
        }

        public IEnumerable<StorageFile> GetAllFiles(string query)
        {
            var files = _context.StorageFiles;
            if (query == null)
            {
                return files;
            }
            return files.Where(x => x.Name.ToLower().Contains(query.ToLower()));
        }

        public IEnumerable<StorageFile> GetFilesByUser(string userId)
        {
            var files = _context.StorageFiles.Where(x => x.ApplicationUser.Id == userId);
            return files;
        }

        public void AddFile(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                string path = "../FileStorageDAL/Files/" + uploadedFile.FileName;
                StorageFile file = new()
                {
                    Name = uploadedFile.FileName,
                    RelativePath = path,
                    Created = DateTime.Now,
                    Size = uploadedFile.Length,
                    Extension = uploadedFile.ContentType,
                    //ApplicationUser = _userManager.Users.FirstOrDefault(),
                    IsPublic = true,
                    Id = _context.StorageFiles.Count() + 1
                };
                _context.StorageFiles.AddAsync(file);
                _context.SaveChangesAsync().Wait();
            }
        }
    }
}
