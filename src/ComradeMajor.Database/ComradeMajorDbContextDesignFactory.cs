using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ComradeMajor.Database
{
    public class ComradeMajorDbContextDesignFactory : IDesignTimeDbContextFactory<ComradeMajorDbContext>
    {
        public ComradeMajorDbContext CreateDbContext(string[] args)
        {
            string connectionString;
            if (args.Length < 1)
            {
                Console.WriteLine("Using empty connection string.");
                connectionString = string.Empty;
            }
            else
            {
                connectionString = args[0];
            }
            return new ComradeMajorDbContext(connectionString);
        }
    }
}