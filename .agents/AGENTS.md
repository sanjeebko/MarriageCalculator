# Project Rules & Guidelines for Marriage Calculator

## Ticket & Branching Workflow (STRICT REQUIREMENT)

1. **Ticket-Driven Development**:
   - ALL work (features, bug fixes, refactoring, documentation, infrastructure) MUST be backed by a corresponding GitHub issue/ticket.
   - If a task does not have an open issue, create one first via `gh issue create`.

2. **Feature Branch Isolation**:
   - ALL changes MUST be made in a dedicated feature/fix branch created for the specific ticket (e.g., `feature/issue-14-dotnet10-upgrade` or `fix/issue-<number>-<description>`).
   - **The `main` branch MUST NOT be edited or committed to directly under any condition.**

3. **Pull Request Merging**:
   - Feature branches MUST be merged into `main` via Pull Request only (`gh pr create` / GitHub UI).
   - Direct pushes or direct commits to `main` are strictly forbidden.
