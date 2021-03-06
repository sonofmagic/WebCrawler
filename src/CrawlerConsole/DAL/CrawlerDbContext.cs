﻿using CrawlerConsole.DAL.Entity;

namespace CrawlerConsole.DAL
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class CrawlerDbContext : DbContext
    {
        //您的上下文已配置为从您的应用程序的配置文件(App.config 或 Web.config)
        //使用“CrawlerDbContext”连接字符串。默认情况下，此连接字符串针对您的 LocalDb 实例上的
        //“CrawlerConsole.DAL.CrawlerDbContext”数据库。
        // 
        //如果您想要针对其他数据库和/或数据库提供程序，请在应用程序配置文件中修改“CrawlerDbContext”
        //连接字符串。
        public CrawlerDbContext()
            : base("name=CrawlerDbContext")
        {
        }
        public virtual DbSet<GroupPrice> T_GroupPrice { get; set; }
        public virtual DbSet<Line> T_Line { get; set; }

        public virtual DbSet<ArriveCity> T_ArriveCity { get; set; }

        public virtual DbSet<DepartureCity> T_DepartureCity { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ArriveCityConfig());
            //modelBuilder.Entity<GroupPrice>().HasRequired(g => g.Line).WithMany(l => l.GroupPrices).WillCascadeOnDelete(true);
            //base.
        }

        //为您要在模型中包含的每种实体类型都添加 DbSet。有关配置和使用 Code First  模型
        //的详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=390109。

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}