
## JobFac.database

A repository-style library for interacting with the JobFacStorage database. Uses Dapper.

A DI registration extension is available, allowing setup as `services.AddDatabaseServices()` while the service provider is being built. This results in two injectable repository services, `DefinitionsRepository` and `HistoryRepository`.

