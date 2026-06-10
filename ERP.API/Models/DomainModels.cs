using System;
using System.Collections.Generic;

namespace ERP.API.Models
{
    public enum InvoiceStatus { Pending = 0, Approved = 1, Rejected = 2 }
    public enum ApprovalStatus { Pending = 0, Approved = 1, Rejected = 2 }

    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public string VendorName { get; set; }
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastModifiedOn { get; set; }

        public ICollection<ApprovalRequest> ApprovalRequests { get; set; }
    }

    public class ApprovalRequest
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
        public string RequestedBy { get; set; }
        public string ApproverId { get; set; }
        public ApprovalStatus Status { get; set; }
        public string ApprovalToken { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class Conversation
    {
        public int Id { get; set; }
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime LastActivityOn { get; set; }

        public ICollection<ChatMessage> Messages { get; set; }
        public ICollection<ToolExecution> ToolExecutions { get; set; }
        public ICollection<AgentExecution> AgentExecutions { get; set; }
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public string Role { get; set; }
        public string Message { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class ToolExecution
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public string ToolName { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public DateTime ExecutedOn { get; set; }
    }

    public class AgentExecution
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public string AgentName { get; set; }
        public string Intent { get; set; }
        public double Confidence { get; set; }
        public DateTime ExecutedOn { get; set; }
    }

    public class MemorySummary
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string QdrantPointId { get; set; }
        public string Summary { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string PerformedBy { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
