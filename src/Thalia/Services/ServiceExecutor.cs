﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thalia.Services.Cache;
using System.Linq;

namespace Thalia.Services
{
    public class ServiceExecutor<T> : IServiceExecutor<T> where T : class
    {
        protected List<IServiceOperation<T>> _operations = new List<IServiceOperation<T>>();
        protected ICacheRepository<T> _cacheRepository;
        protected ILogger _logger;

        public ServiceExecutor(ILogger logger, ICacheRepository<T> cacheRepository)
        {
            _logger = logger;
            _cacheRepository = cacheRepository;
        }

        private T GetFromCache(string parameters)
        {
            var item = _cacheRepository.Find(GetType().Name, parameters);
            if (item == null) return null;

            foreach (var operation in _operations)
            {
                if (operation.GetType().Name == item.Operation)
                {
                    return operation.GetResult(item.Result);
                }
            }
            return default(T);                
        }

        public async Task<T> Execute(string parameters)
        {
            var result = GetFromCache(parameters);
            if (result != null)
            {
                return result;
            }

            // iterate and execute until we get a result from any operation
            foreach (var operation in _operations)
            {
                if (!CheckQuota(operation))
                {
                    _logger.LogCritical($"Over quota in {GetType().Name}.{operation.GetType()}");

                    // fall back to next operation
                    continue;
                }

                result = await operation.Execute(parameters);
                if (result != null)
                {
                    _cacheRepository.Add(GetType().Name, operation);
                    return result;
                }
            }
            
            // todo WRITE something for the alert manager to send the email immediately
            // all operations failed, alert the admin
            
            return default(T);
        }

        public bool CheckQuota(IServiceOperation<T> operation)
        {
            if ((operation.RequestsPerMinute == null) || (operation.RequestsPerMinute == 0))
                return true;

            var count = _cacheRepository.CountItems(operation.GetType().Name, DateTime.Now.AddMinutes(-1));
            return count <= operation.RequestsPerMinute;
        }
    }
}
