using NexaFlow.Domain.Common;
using NexaFlow.Domain.Enums;

namespace NexaFlow.Domain.Entities;

/// <summary>Named WorkflowTask, not Task, to avoid clashing with System.Threading.Tasks.Task.</summary>
public class WorkflowTask : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public Guid WorkflowId { get; set; }
    public Workflow? Workflow { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkflowTaskStatus Status { get; set; } = WorkflowTaskStatus.Pending;

    public Guid? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public DateTime? DueAtUtc { get; set; }
}
