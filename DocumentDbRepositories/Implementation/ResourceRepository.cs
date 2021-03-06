﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace DocumentDbRepositories.Implementation
{
	public class ResourceRepository
	{
		private readonly DocumentClient _client;
		private readonly DocumentCollection _collection;

		public ResourceRepository(DocumentClient client, DocumentCollection collection)
		{
			_client = client;
			_collection = collection;
		}
		public Task<ScampResource> GetResource(string resourceId)
		{
			//TODO Check
			var resources = from r in _client.CreateDocumentQuery<ScampResource>(_collection.SelfLink)
							where r.Id == resourceId && r.Type == "resource"
							select r;
			var resourceList = resources.ToList();
			if (resourceList.Count == 0)
				return Task.FromResult((ScampResource)null);
			return Task.FromResult(resourceList[0]);
		}

		public Task<IEnumerable<ScampResourceGroup>> GetResources()
		{
			var resources = from r in _client.CreateDocumentQuery<ScampResource>(_collection.SelfLink)
							where r.Type == "resource"
							select r;
			var resourceList = resources.ToList();
			return Task.FromResult((IEnumerable<ScampResourceGroup>)resourceList);
		}

        // given a list of resourceId's, return the resource references
        public async Task<List<ScampResourceReference>> GetResources(List<string> resourceIds)
        {
            List<ScampResourceReference> rtnList = new List<ScampResourceReference>();

            //TODO: build and execute the query

            return rtnList;
        }

		public Task<IEnumerable<ScampResourceGroup>> GetResourcesByOwner(string userId)
		{
			//TODO: need to add "join" to get by owner relationship
			var resources = from r in _client.CreateDocumentQuery<ScampResourceGroup>(_collection.SelfLink)
							select r;
			var resourceList = resources.ToList();
			return Task.FromResult((IEnumerable<ScampResourceGroup>)resourceList);
		}

		public async Task CreateResource(ScampResource resource)
		{

			var created = await _client.CreateDocumentAsync(_collection.SelfLink, resource);
		}

		public Task<IEnumerable<ScampResource>> GetResourcesByGroup(ScampUserReference user, string groupId)
		{
			bool isGroupAdmin = IsGroupAdmin(user, groupId);

			var resources = _client.CreateDocumentQuery<ScampResource>(_collection.SelfLink)
				.Where(u => u.Type == "resource" && u.ResourceGroup.Id == groupId);
			if (!isGroupAdmin)
			{
				resources = resources.Where(u => u.Owners[0].Id == user.Id);
			}
			var resourceList = resources.ToList();
			return Task.FromResult((IEnumerable<ScampResource>)resourceList);
		}

		private bool IsGroupAdmin(ScampUserReference user, string groupId)
		{	// check all group's admin
			return _client.CreateDocumentQuery<dynamic>(_collection.SelfLink, new SqlQuerySpec
			{
				QueryText = "SELECT g FROM Groups g JOIN u IN g.admins WHERE g.id = @groupId AND g.type='group' AND u.id = @userId",
				Parameters = new SqlParameterCollection()
					{
						  new SqlParameter("@groupId", groupId),
						  new SqlParameter("@userId", user.Id)
					}
			}).ToList().Any();
		}

		public Task AddResource(string groupID)
		{
			//TODO: stuff
			throw new NotImplementedException();
		}

		public void AddOwner(string groupID)
		{
			//TODO: stuff
			throw new NotImplementedException();
		}

		public async Task UpdateResource(ScampResource resource)
		{
			//TODO Check
			var dbRes = (from u in _client.CreateDocumentQuery(_collection.SelfLink)
						 where u.Id == resource.Id
						 select u).ToList().FirstOrDefault();

			await _client.ReplaceDocumentAsync(dbRes.SelfLink, resource);
		}

		public async Task DeleteResource(string resourceId)
		{
			var dbRes = (from u in _client.CreateDocumentQuery(_collection.SelfLink)
						 where u.Id == resourceId
						 select u).ToList().FirstOrDefault();
			await _client.DeleteDocumentAsync(dbRes.SelfLink);
		}
	}
}
