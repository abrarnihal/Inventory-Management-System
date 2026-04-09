# Sprint 3 – Team Resources & Allocation

## Team Composition

| Role | Count | Sprint 3 Focus |
|---|---|---|
| Project Manager | 1 | Final sprint coordination, release planning, documentation |
| QA Lead | 1 | Test strategy execution, E2E test design, defect triage |
| QA Engineers | 2 | Unit tests, integration tests, automated UI tests |
| Full-Stack Developer | 1 | AI ChatBot feature, notification system, CAPTCHA |
| DevOps / Infrastructure | 1 | Azure deployment, CI/CD pipeline, health checks |

**Total team size:** 6 members

---

## Processes & Methods

### Agile / Scrum (Final Sprint)

- **Daily standups** focused on test coverage metrics and blocker resolution.
- **Bug triage meetings** (3× per week) to prioritize defects found during testing.
- **Sprint review** – Full end-to-end demo of the application to stakeholders, including the ChatBot feature.
- **Final retrospective** – Lessons learned across all three sprints.

### Testing Process

1. **Test plan creation** – QA Lead defined test cases for each module based on Sprint 1 & 2 requirements.
2. **Unit test development** – All controllers, services, and utilities tested in isolation using mocked dependencies.
3. **Integration test development** – API endpoints tested through the full HTTP pipeline with `WebApplicationFactory`.
4. **E2E test development** – Critical business workflows tested via Selenium browser automation.
5. **Automated UI tests** – CRUD operations and navigation validated through automated browser interactions.
6. **Regression testing** – Full test suite executed before every merge to `master`.

### Quality Gates

| Gate | Criteria | Met? |
|---|---|---|
| Unit test coverage | All controllers and services have test classes | ✅ |
| Integration test coverage | All API endpoints tested | ✅ |
| E2E workflow coverage | Purchase cycle, sales cycle, auth flows tested | ✅ |
| Zero critical defects | No P0/P1 bugs at sprint close | ✅ |
| Build passes | All projects compile without errors | ✅ |

### Development Practices

- **Test-driven development (TDD)** for the ChatBot feature – tests written before implementation.
- **Pair programming** for complex E2E test scenarios.
- **Continuous Integration** – All four test projects run on every pull request.
- **Feature flags** – ChatBot feature can be disabled via configuration if OpenAI API key is not provided.

---

## Sprint 3 Velocity & Outcomes

- **Stories completed:** 20 user stories
- **Story points delivered:** 48
- **Test classes created:** 80+ unit, 10 integration, 6 E2E, 11 automated
- **New features:** ChatBot, notifications, health check, CAPTCHA, file import
- **Migrations added:** 4 (ChatLog, ConversationTitle, ConversationIsPinned, AzureSqlMigration)
- **Key achievement:** Production-ready application with comprehensive test coverage and cloud deployment readiness.

---

## Overall Project Summary

| Metric | Sprint 1 | Sprint 2 | Sprint 3 | Total |
|---|---|---|---|---|
| Stories Completed | 18 | 24 | 20 | **62** |
| Story Points | 42 | 56 | 48 | **146** |
| Team Members | 6 | 6 | 6 | **6** |
| Sprint Duration | 2 weeks | 2 weeks | 2 weeks | **6 weeks** |
