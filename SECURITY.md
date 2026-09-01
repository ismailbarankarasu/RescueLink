# Security Policy

RescueLink handles authentication credentials, precise geographic locations, uploaded images, and user contact information. Please report security issues responsibly so they can be investigated before details become public.

## Supported Versions

RescueLink is currently developed from the `master` branch and does not yet publish stable versioned releases.

| Version | Supported |
| --- | --- |
| Latest `master` | Yes |
| Older commits, forks, and unofficial deployments | No |

Security fixes are applied to the latest development version. After versioned releases begin, this table will be updated.

## Reporting a Vulnerability

Do not open a public GitHub issue, discussion, or pull request containing vulnerability details, proof-of-concept code, credentials, tokens, personal data, or exploitation steps.

Preferred reporting method:

1. Open the repository's **Security** tab.
2. Select **Advisories**.
3. Choose **Report a vulnerability**.
4. Provide the information listed below.

If private vulnerability reporting is not available, contact the repository owner using the private contact information listed on the [owner's GitHub profile](https://github.com/ismailbarankarasu). Do not disclose technical details publicly. If no private channel can be reached, open a minimal issue asking for a private contact method without describing the vulnerability.

Include:

- a clear description of the vulnerability;
- the affected endpoint, component, branch, or commit;
- prerequisites and the smallest safe reproduction;
- potential impact;
- suggested mitigation, if known;
- whether the issue is already public or actively exploited;
- a safe way to contact you for follow-up.

Remove real secrets and personal data. Use test accounts, placeholder tokens, and non-production environments.

## Response Process

The project aims to:

- acknowledge a report within 5 business days;
- provide an initial assessment or request more information within 10 business days;
- keep the reporter informed when meaningful progress is made;
- coordinate disclosure after a fix or mitigation is available.

These are targets rather than guaranteed service-level agreements. Response time may depend on severity, reproducibility, and maintainer availability.

## Scope Priorities

Security-sensitive areas include:

- JWT access and refresh-token creation, rotation, replay protection, and revocation;
- authentication, authorization, ownership, and contact disclosure;
- exact locations and privacy-aware public responses;
- file upload signature validation, size limits, storage keys, and path traversal;
- SQL injection risks in EF Core and Dapper queries;
- rate limiting, CORS, logging, and safe exception responses;
- Docker, environment variables, User Secrets, and deployment configuration;
- notification ownership and cross-user data isolation;
- dependencies and GitHub Actions workflows.

## Responsible Disclosure

Please:

- avoid accessing, modifying, or deleting data that does not belong to you;
- avoid privacy violations, service disruption, denial-of-service testing, and social engineering;
- test only with accounts and data you control;
- give the project reasonable time to investigate and fix the issue;
- coordinate public disclosure with the maintainer.

Reports made in good faith and within these guidelines are appreciated.

## Not Security Vulnerabilities

The following normally belong in the regular [bug report form](https://github.com/ismailbarankarasu/RescueLink/issues/new/choose):

- ordinary functional defects without a security impact;
- feature requests;
- unsupported local environment problems;
- dependency update suggestions without a demonstrated vulnerability;
- hypothetical concerns without an affected component or plausible impact.

When uncertain, prefer private reporting.
