﻿using System.Collections.ObjectModel;

namespace core.services;

public interface IDnsService
{
    Task<List<string>> GetCnameAsync(string hostName);

    Task<List<string>> GetTxtAsync(string hostName);

    Task<bool> WaitForCnameAsync(string hostname, string expectedValue,
        TimeSpan waitBetweenRetries, int maxRetries,
        Collection<string>? statusCollection = null);

    Task<bool> WaitForTxtAsync(string hostname, string expectedValue,
        TimeSpan waitBetweenRetries, int maxRetries,
        Collection<string>? statusCollection = null);
}