"# BCIT-Social"

## Development sample data

When the app runs in the Development environment, startup applies pending EF Core
migrations and seeds sample users, profiles, communities, posts, comments, events,
messages, tags, and their related records. Seeding is idempotent; restarting the app
does not duplicate the sample data. It seeds at least 20 records in each application
data table, including the join tables. Identity framework system tables such as
claims and external logins are not populated because the sample data does not need them.

The seeded demo users can sign in with any of these email addresses:

- `maya.chen@example.com`
- `noah.patel@example.com`
- `ava.williams@example.com`
- `liam.garcia@example.com`

The shared local-development password is `Demo123!`. These accounts are only
created in Development and should not be used in a deployed environment."
