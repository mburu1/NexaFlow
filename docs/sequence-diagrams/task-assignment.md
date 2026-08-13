# Task assignment → notification (planned, Phase 2)

**Not implemented.** `WorkflowTaskService.CreateAsync`/`UpdateAsync` persist the task today
and stop there — no event is published. This diagram documents the intended Phase 2 flow
once `NexaFlow.Messaging`'s `IEventPublisher` gets a real RabbitMQ implementation (it's
currently `NoOpEventPublisher`) and `NexaFlow.Notifications`'s `MailKitEmailSender` gets
wired up to consume it. See [ADR-002](../adr/002-messaging-choice.md).

```mermaid
sequenceDiagram
    actor Manager
    participant Api as WorkflowTasksController
    participant Svc as WorkflowTaskService
    participant Pub as IEventPublisher (planned: RabbitMQ)
    participant Consumer as Notification consumer (planned)
    participant Mail as MailKitEmailSender
    participant User as Assigned user

    Manager->>Api: POST /workflows/{id}/tasks {title, assignedToUserId}
    Api->>Svc: CreateAsync(workflowId, request)
    Svc->>Svc: persist WorkflowTask (implemented today)
    Note over Svc,Pub: Phase 2: publish TaskAssignedEvent here
    Svc-->>Pub: TaskAssignedEvent(taskId, assignedToUserId)
    Pub->>Consumer: RabbitMQ message (task.assigned queue)
    Consumer->>Mail: SendAsync(user.Email, "New task assigned", ...)
    Mail-->>User: email delivered (MailHog in dev, SendGrid in prod)
```
