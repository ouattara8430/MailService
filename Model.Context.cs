﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré à partir d'un modèle.
//
//     Des modifications manuelles apportées à ce fichier peuvent conduire à un comportement inattendu de votre application.
//     Les modifications manuelles apportées à ce fichier sont remplacées si le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MailService
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CommitmentDBTestEntities : DbContext
    {
        public CommitmentDBTestEntities()
            : base("name=CommitmentDBTestEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Branch> Branches { get; set; }
        public virtual DbSet<Decision> Decisions { get; set; }
        public virtual DbSet<Financing> Financings { get; set; }
        public virtual DbSet<GestionnaireReport> GestionnaireReports { get; set; }
        public virtual DbSet<LoanApplication> LoanApplications { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<UserProfile> UserProfiles { get; set; }
        public virtual DbSet<AlertRequest> AlertRequests { get; set; }
    }
}
