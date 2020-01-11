
# JobFac: The Background Job Factory

The contents of this repository are a work-in-progress.

Watch this space for real documentation links.

## Summary

JobFac runs and manages console programs acting as background or automated processing jobs. 

Jobs launch on a scheduled basis or on-demand via service calls. Single-server or even local implementation is possible, but the primary use-case is job-execution load balanced across a self-managing server cluster. The system includes the usual monitoring, notification, and reporting features found in systems like this.

Job interaction is available to client applications through an HTTP-based API. While a .NET client application could take advantage of binary-level connectivity to the job services, a system of this nature does not typically require such convenience/performance tradeoffs. (The legacy .NET Framework is not supported except as a job client issuing HTTP-based API calls.)

There is also a sequencing system which can launch and control a series of jobs with conditional start rules at each step and branching features based on job outcome (success, failure, mixed). Sequences can also be scheduled or started on-demand and can provide altered arguments or JobFac payloads to any of the jobs in the sequence.

Currently this is implemented as one giant solution. Later it will be split into several solutions and a few NuGet packages. Each project in this repository has a separate README with a bit more detail about the project (specifically any project with a JobFac prefix, the rest are early-stage, temporary test projects such as DevConsoleHost).

The goal is to keep this fully open sourced and free to use, along the same lines of other popular .NET projects like Serilog or IdentityServer4.

## Dependencies

* .NET Core 3.1
* Microsoft Orleans 3.0
* Dapper 2.0
* SQL Server 2016+
* Noda Time 2.4

## Progress Notes

2020-jan-11
* simplified implementation of JobFac-aware jobs
* support for dev/test of JobFac-aware jobs without a JobFac server

2020-jan-09
* formalized JobFac-aware jobs with base class and registration support
* switched DateTimeAnalysis to factory construction (better semantics)

2020-jan-03
* job sequence support completed
* custom exceptions added

2020-jan-02
* all schedule date/time combinations tested
* initial work to support job sequence execution

2020-jan-01
* :beer: :fireworks: :date: :sparkler: :cocktail:
* refactored schedule planner to allow intenstive testing

2019-dec-31
* scheduling system complete
* refactored all date/time handling to Noda Time (avoid DST/timezone issues etc.)

2019-dec-30
* scheduled execution features partly completed
* added Config table and internal config-cache service

2019-dec-29
* heavy refactoring to define a sequence as kind of job, simplify database

2019-dec-28
* implemented JobFac-aware sample and test code with startup payload retrieval
* added CoordinatedBackgroundService (cleaner startup than .NET's BackgroundService)
* migrated all hosted services to CoordinatedBackgroundService

2019-dec-27
* job "already running" options implemented
* remote-access to JobFac server's ILogger (Runner is a separate process)

2019-dec-26
* added service provider to hide Orleans dependencies from client apps
* added Generic Host support including clean Orleans client shutdown
* migrating test apps and Runner to Generic Host for cleaner architecture

2019-dec-25
* overhauled namespaces and projects (and lost git history...)

2019-dec-24
* capture stdout and/or stderr to database
* capture stdout and/or stderr to files with optional timestamp
* support for JobFac-aware jobs

2019-dec-23
* dev-quality host and client test apps
* can execute jobs and track status to completion
