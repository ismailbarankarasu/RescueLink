<!--
Thank you for contributing to RescueLink.

Before submitting:
- Read CONTRIBUTING.md.
- Keep the pull request focused on one confirmed issue.
- Do not include secrets, tokens, connection strings, real .env values, or private user data.
- Replace placeholder text and mark only the checks that are true.
-->

## Related issue

<!-- Use "Fixes #123" when this pull request fully resolves an issue. -->

Fixes #<issue-number>

## Type of change

<!-- Mark all that apply with an x. -->

- [ ] Bug fix
- [ ] New feature
- [ ] Refactor
- [ ] Tests
- [ ] Documentation
- [ ] Build, CI, or tooling
- [ ] Database migration

## Summary

<!-- What problem does this pull request solve? Keep the explanation concise and user-focused. -->

Describe the change and why it is needed.

## Implementation details

<!-- Explain important technical decisions, affected layers, and any alternatives considered. -->

- 
- 

## Architecture and scope

<!-- Confirm the relevant RescueLink conventions. -->

- [ ] Controllers remain thin; business logic is in Domain/Application.
- [ ] CQRS, MediatR, FluentValidation, and existing feature-folder conventions are followed where applicable.
- [ ] Dapper SQL remains parameterized where applicable.
- [ ] The pull request contains no unrelated refactoring or formatting changes.

## API and database impact

<!-- Describe contract or schema changes. Write "None" when not applicable. -->

### API changes

None.

### Database changes

None.

### Migration

- [ ] A migration is included and reviewed.
- [ ] No migration is required.

## Verification

<!-- List the tests and manual checks performed. Include useful screenshots or example responses when applicable. -->

### Commands run

```bash
dotnet build RescueLink.slnx --configuration Release
dotnet test RescueLink.slnx --configuration Release
dotnet format RescueLink.slnx --verify-no-changes
```

### Test coverage

- [ ] New or updated tests cover the changed behavior.
- [ ] Existing tests pass.
- [ ] Manual API verification was performed where applicable.
- [ ] The change is documentation-only and does not require new automated tests.

## Security and privacy impact

<!-- Consider authorization, ownership, locations, contact data, uploads, tokens, logs, and error responses. Write "None" only after reviewing these areas. -->

None.

- [ ] Authorization and ownership behavior is preserved or tested.
- [ ] No secrets, credentials, tokens, connection strings, real `.env` values, or private user data are included.
- [ ] Logs, screenshots, request examples, and test data contain no sensitive information.

## Breaking changes

<!-- Describe required client, configuration, or deployment updates. Write "None" when there are no breaking changes. -->

None.

## Final checklist

- [ ] I read and followed [CONTRIBUTING.md](../../CONTRIBUTING.md).
- [ ] The confirmed issue scope and acceptance criteria are satisfied.
- [ ] The branch is based on the latest `master`.
- [ ] I reviewed the final diff.
- [ ] Build and relevant tests pass locally.
- [ ] Documentation and API examples are updated when behavior changed.
- [ ] CI checks are expected to pass.
