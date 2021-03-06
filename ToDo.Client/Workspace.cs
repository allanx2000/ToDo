﻿using Microsoft.Data.Entity;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Client.Core;
using ToDo.Client.Core.Lists;
using ToDo.Client.Core.Tasks;

namespace ToDo.Client
{
    public partial class Workspace : DbContext
    {   

        private static Workspace instance;

        /*
        public static async Task LoadWorkspaceAsync(string workspacePath, string dbFile)
        {
            LoadWorkspace(workspacePath, dbFile);
        }
        */

        public static void LoadWorkspace(string workspacePath, string dbFile)
        {
            try
            {
                if (instance == null)
                {
                    instance = new Workspace(workspacePath, dbFile);
                    instance.Database.EnsureCreated();
                }
            }
            catch (Exception e)
            {
                instance = null;
                throw;
            }
        } 
        public static Workspace Instance
        {
            get
            {
                return instance;
            }
        }

        public DbSet<Comment> Comments {get; set; }
        public DbSet<TaskList> Lists { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskLog> TasksLog { get; set; }
        
        private string workspacePath, dbPath;
        private Workspace(string workspacePath, string dbPath)
        {
            //TODO: Change to just one
            this.workspacePath = workspacePath;
            this.dbPath = dbPath;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskLog>()
                .HasKey(x => new { x.TaskID, x.Date });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = dbPath };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            optionsBuilder.UseSqlite(connection);

        }

        

        //TODO: Add Seed(...) and Unique indexes
        //Not supported in EF7...
        public void RejectChanges()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        {
                            
                            //entry.State.CurrentValues.SetValues(entry.OriginalValues);
                            entry.State = EntityState.Unchanged;
                            break;
                        }
                    case EntityState.Deleted:
                        {
                            entry.State = EntityState.Unchanged;
                            break;
                        }
                    case EntityState.Added:
                        {
                            entry.State = EntityState.Detached;
                            break;
                        }
                }
            }
        }

        public static void Unload()
        {
            TasksUpdateTimer.StopTimer();

            instance.Dispose();
            instance = null;
        }
    }
}
