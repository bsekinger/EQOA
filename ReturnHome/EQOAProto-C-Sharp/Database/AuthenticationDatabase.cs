﻿using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;


using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

using System.Collections.Generic;
using System;
using System.Net;

using ReturnHome.Database.Models.Auth;

namespace ReturnHome.Database
{
    public class AuthenticationDatabase
    {
        //private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Exists(bool retryUntilFound)
        {
            //var config = Common.ConfigManager.Config.MySql.Authentication;

            for (; ; )
            {
                using (var context = new AuthDbContext())
                {
                    //if (((RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>()).Exists())
                    //{
                        //log.Debug($"Successfully connected to {config.Database} database on {config.Host}:{config.Port}.");
                        //return true;
                    //}
                }

                //log.Error($"Attempting to reconnect to {config.Database} database on {config.Host}:{config.Port} in 5 seconds...");

                if (retryUntilFound)
                    Thread.Sleep(5000);
                else
                    return false;
            }
        }


        public int GetAccountCount()
        {
            using (var context = new AuthDbContext())
                return context.Account.Count();
        }

        /// <summary>
        /// Will return null if the accountId was not found.
        /// </summary>
        public Account GetAccountById(uint accountId)
        {
            using (var context = new AuthDbContext())
            {
                return context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountId == accountId);
            }
        }

        /// <summary>
        /// Will return null if the accountName was not found.
        /// </summary>
        public Account GetAccountByName(string accountName)
        {
            using (var context = new AuthDbContext())
            {
                return context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountName == accountName);
            }
        }

        /// <summary>
        /// id will be 0 if the accountName was not found.
        /// </summary>
        public uint GetAccountIdByName(string accountName)
        {
            using (var context = new AuthDbContext())
            {
                var result = context.Account
                    .AsNoTracking()
                    .FirstOrDefault(r => r.AccountName == accountName);

                return (result != null) ? result.AccountId : 0;
            }
        }

        public void UpdateAccount(Account account)
        {
            using (var context = new AuthDbContext())
            {
                context.Entry(account).State = EntityState.Modified;

                context.SaveChanges();
            }
        }

        /*
        public bool UpdateAccountAccessLevel(uint accountId, AccessLevel accessLevel)
        {
            using (var context = new AuthDbContext())
            {
                var account = context.Account
                    .First(r => r.AccountId == accountId);

                if (account == null)
                    return false;

                account.AccessLevel = (uint)accessLevel;

                context.SaveChanges();
            }

            return true;
        }

        public List<string> GetListofAccountsByAccessLevel(AccessLevel accessLevel)
        {
            using (var context = new AuthDbContext())
            {
                var results = context.Account
                    .AsNoTracking()
                    .Where(r => r.AccessLevel == Convert.ToUInt32(accessLevel)).ToList();

                var result = new List<string>();
                foreach (var account in results)
                    result.Add(account.AccountName);

                return result;
            }
        }
        */
        public List<string> GetListofBannedAccounts()
        {
            using (var context = new AuthDbContext())
            {
                var results = context.Account
                    .AsNoTracking()
                    .Where(r => r.BanExpireTime > DateTime.UtcNow).ToList();

                var result = new List<string>();
                foreach (var account in results)
                {
                    var bannedbyAccount = account.BannedByAccountId.Value > 0 ? $"account {GetAccountById(account.BannedByAccountId.Value).AccountName}" : "CONSOLE";
                    result.Add($"{account.AccountName} -- banned by {bannedbyAccount} until server time {account.BanExpireTime.Value.ToLocalTime():MMM dd yyyy  h:mmtt}{(!string.IsNullOrWhiteSpace(account.BanReason) ? $" -- Reason: {account.BanReason}" : "")}");
                }

                return result;
            }
        }
    }
}
