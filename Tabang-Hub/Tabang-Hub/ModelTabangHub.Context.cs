﻿

//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace Tabang_Hub
{

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;


public partial class TabangHubEntities : DbContext
{
    public TabangHubEntities()
        : base("name=TabangHubEntities")
    {

    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        throw new UnintentionalCodeFirstException();
    }


    public DbSet<Role> Role { get; set; }

    public DbSet<UserAccount> UserAccount { get; set; }

    public DbSet<UserRoles> UserRoles { get; set; }

    public DbSet<vw_UserRoles> vw_UserRoles { get; set; }

}

}
