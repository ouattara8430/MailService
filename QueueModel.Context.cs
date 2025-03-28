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
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class QueueDBEntities : DbContext
    {
        public QueueDBEntities()
            : base("name=QueueDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<TblService> TblServices { get; set; }
        public virtual DbSet<TblServiceType> TblServiceTypes { get; set; }
        public virtual DbSet<TicketHistory> TicketHistories { get; set; }
        public virtual DbSet<TblRole> TblRoles { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<TicketAssignment> TicketAssignments { get; set; }
        public virtual DbSet<TblDecision> TblDecisions { get; set; }
        public virtual DbSet<TblStaff> TblStaffs { get; set; }
    
        public virtual ObjectResult<FetchListService_Result> FetchListService(Nullable<int> service_id)
        {
            var service_idParameter = service_id.HasValue ?
                new ObjectParameter("service_id", service_id) :
                new ObjectParameter("service_id", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<FetchListService_Result>("FetchListService", service_idParameter);
        }
    
        public virtual ObjectResult<Nullable<int>> FetchAvailableStaff()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("FetchAvailableStaff");
        }
    
        public virtual ObjectResult<FetchListTicket_Result> FetchListTicket()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<FetchListTicket_Result>("FetchListTicket");
        }
    
        public virtual int GenerateTicketNo(ObjectParameter ticketNumber)
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("GenerateTicketNo", ticketNumber);
        }
    
        public virtual ObjectResult<FetchServices_Result> FetchServices()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<FetchServices_Result>("FetchServices");
        }
    }
}
