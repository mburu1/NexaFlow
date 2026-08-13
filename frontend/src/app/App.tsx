import './App.css';

/**
 * Placeholder app shell. The dashboard (auth, workflows, tasks features) is
 * scaffolded under src/features but not implemented yet — see README roadmap.
 */
function App() {
  return (
    <div className="app-shell">
      <h1>NexaFlow</h1>
      <p>Enterprise task &amp; workflow orchestration platform.</p>
      <p>
        The backend API (auth, tenants, workflows, tasks) is live — see{' '}
        <code>backend/README.md</code>. This dashboard is scaffolded for Phase 2.
      </p>
      <div className="phase-list">
        <strong>Roadmap</strong>
        <ul>
          <li>✅ Phase 1 — Multi-tenant auth, RBAC, workflow/task CRUD</li>
          <li>⏳ Phase 2 — Messaging, SignalR live updates, this dashboard</li>
          <li>⏳ Phase 3 — Redis rate limiting, observability</li>
          <li>⏳ Phase 4 — Kubernetes, Helm</li>
        </ul>
      </div>
    </div>
  );
}

export default App;
