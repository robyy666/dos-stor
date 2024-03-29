﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbContexts;
using Microsoft.EntityFrameworkCore;
using Models;
using ViewModels;
using ViewModels.Enums;

namespace Services
{
    public class DocumentSearchService
    {
        private readonly DocumentStorageContext _documentStorage;

        public DocumentSearchService(DocumentStorageContext documentStorage)
        {
            _documentStorage = documentStorage;
        }

        public async Task<List<Document>> SearchDocumentByName(string query)
        {
            query = query.ToLower();
            return await _documentStorage.Documents.Include(document => document.Tags).Where(document => document.Name.ToLower().Contains(query)).ToListAsync();
        }

        public async Task<List<Document>> SearchDocumentByDate(DateSearchViewModel model)
        {
            return await _documentStorage.Documents.Where(document => Helpers.CheckDate(document.CreationTime, model)).ToListAsync();
        }

        public async Task<List<Document>> SearchDocumentsByTags(TagSearchViewModel model)
        {
            return model.Mode == TagSearchMode.Any ? await SearchDocumentsByAnyTags(model.Tags) : await SearchDocumentsByExactTags(model.Tags);
        }

        public async Task<List<Document>> SearchDocumentsByAnyTags(List<Tag> tags)
        {
            return _documentStorage.Tags.Include(tag => tag.Documents)
                .Where(tag => tags.Contains(tag))
                .Select(tag => tag.Documents)
                .Aggregate((accumulated, current) => accumulated.Concat(current).ToList()); //функция сокращения Reduce
        }

        public async Task<List<Document>> SearchDocumentsByExactTags(List<Tag> tags)
        {
            return await _documentStorage.Documents
                .Where(doc => tags.Count == doc.Tags.Intersect(tags).Count())
                .ToListAsync();
        }
    }
}
