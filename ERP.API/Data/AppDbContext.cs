using ERP.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ToolExecution> ToolExecutions { get; set; }
        public DbSet<AgentExecution> AgentExecutions { get; set; }
        public DbSet<MemorySummary> MemorySummaries { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApprovalRequest>()
                .HasOne(a => a.Invoice)
                .WithMany(i => i.ApprovalRequests)
                .HasForeignKey(a => a.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatMessage>()
                .HasOne(cm => cm.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(cm => cm.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ToolExecution>()
                .HasOne(te => te.Conversation)
                .WithMany(c => c.ToolExecutions)
                .HasForeignKey(te => te.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AgentExecution>()
                .HasOne(ae => ae.Conversation)
                .WithMany(c => c.AgentExecutions)
                .HasForeignKey(ae => ae.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
