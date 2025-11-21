# Feature Specification: Basic Todo Dashboard

**Feature Branch**: `001-todo-dashboard`  
**Created**: 2025-11-21  
**Status**: Draft  
**Input**: User description: "I would like to create and basic todo app with(add, edit, delete, filter on todo done or todo not done), simpel deashboard showing al the todos. do not impelment user auth as it is and simpel app for my self"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Manage Personal Todos (Priority: P1)
As the solo user, I capture tasks, update their text, mark them done when complete, or delete them when they are no longer needed.

**Why this priority**: Without reliable creation and maintenance of items the app has no value, so this flow is the foundation of every other scenario.

**Independent Test**: Start with an empty list, add a task, edit it, toggle done/undone, and delete it again to confirm CRUD works without any other features.

**Acceptance Scenarios**:

1. **Given** no todos exist, **When** I add a task with a name and optional note, **Then** it appears at the top of the list with the entered details.
2. **Given** a todo already exists, **When** I edit its text and save, **Then** the list reflects the updated value without creating duplicates.
3. **Given** a todo is visible, **When** I delete it, **Then** it is removed from the list and no longer appears after reloading the page.

---

### User Story 2 - Filter by Completion State (Priority: P2)
I switch between viewing all tasks, only active tasks, or only completed tasks so I can focus on what still needs attention.

**Why this priority**: Filtering keeps the to-do list usable as it grows, enabling focus without adding new data sources.

**Independent Test**: Populate sample data, toggle between filter options, and ensure the view updates immediately without changing stored items.

**Acceptance Scenarios**:

1. **Given** some todos are done and others are not, **When** I choose the "Active" filter, **Then** only undone tasks remain visible while the full dataset stays intact.
2. **Given** the "Completed" filter is enabled, **When** I mark a shown item back to undone, **Then** it disappears from the filtered list and reappears under "Active".

---

### User Story 3 - Review Todo Dashboard (Priority: P3)
I open a single dashboard screen that lists all tasks and shows counts of total, active, and completed items so I can understand workload at a glance.

**Why this priority**: A summary view provides motivation and quick insight without diving into each item, increasing usefulness of a solo productivity tool.

**Independent Test**: With a mix of tasks, load the dashboard screen and verify it renders all items plus aggregate counts without needing any other feature.

**Acceptance Scenarios**:

1. **Given** there are multiple todos, **When** I visit the dashboard, **Then** the list displays every item along with badges showing total, active, and completed counts.
2. **Given** I mark a task as done from the dashboard, **When** the page recalculates, **Then** summary counts update instantly to match the new state.

---

### Edge Cases

- Display a friendly empty state message and quick-add button when no todos exist yet.
- Prevent saving blank titles or duplicate titles that only differ by whitespace to avoid clutter.
- Maintain filters and selected view after page refresh so the user re-enters the same context.
- Handle more than 100 todos without lag by paginating or virtualizing the list as needed (assume a soft limit of 200 per device session).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow the user to create todos with a required title and optional note directly from the dashboard.
- **FR-002**: The system MUST allow the user to edit any existing todo via a modal dialog without duplicating the entry.
- **FR-003**: The user MUST be able to toggle an individual todo between done and not done states, with visual feedback for each state.
- **FR-004**: The system MUST support deleting a todo with a confirmation step and remove it from the persistent store immediately.
- **FR-005**: The dashboard MUST provide filter controls for "All", "Active", and "Completed" and update the visible list instantly when a filter changes.
- **FR-006**: The dashboard MUST show aggregate counts for total, active, and completed todos and keep them synchronized with the underlying data.
- **FR-007**: Todos MUST persist between browser sessions so that reloading or closing the app does not lose the list.
- **FR-008**: The UI MUST surface an empty-state message that invites the user to start by adding a task whenever no todos meet the applied filter.

### Key Entities *(include if feature involves data)*

- **TodoItem**: Represents a single task with fields for identifier, title, optional note, completion state, created timestamp, modified timestamp, and optional due day.
- **DashboardSummary**: Derived counts (total, active, completed) plus current filter selection used to render the dashboard header and filter controls.

## Assumptions

- Single-device personal usage means all data can be stored locally without multi-user conflict handling.
- Browser local storage (or equivalent) is sufficient persistence; no server sync or authentication is required.
- Todos do not require time-of-day scheduling, recurring rules, or reminders in this iteration.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can add, edit, mark done, and delete a todo within 10 seconds each, with changes surviving a page reload.
- **SC-002**: Switching filters updates the visible list in under 1 second for up to 200 items and never alters stored data.
- **SC-003**: The dashboard summary always reflects accurate counts, with 100% consistency verified by automated or manual tests across add/edit/delete actions.
- **SC-004**: At least 90% of usability test participants report they can understand their workload from the dashboard without needing external notes.

## Clarifications

### Session 2025-02-14

- Q: How should todo editing be presented on the dashboard? A: Use a modal dialog for editing.
