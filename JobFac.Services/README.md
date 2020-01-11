
## JobFac.Services

JobFac service interface definitions as Microsoft Orleans grains. .NET client applications only need to reference this library in order to use JobFac services (Orleans is interface-driven as far as the client is concerned, since clients operate through proxies instead of directly against the grain implementations).

In general, JobFac clients (such as those wishing to start or check on jobs) need only inject `IJobFacServiceProvider` -- they don't need to know anything about Orleans or grains. That interface provides three simple methods:

* `GetJobFactory()`
* `GetExternalProcessJob(instanceId)`
* `GetSequenceJob(instanceId)`

JobFac-aware jobs are Generic Host console programs which need only derive from one simple base class in order to recieve startup payloads and report results back to JobFac, and call one simple method during the host build process:

* `AddJobFacAwareServicesAsync(options => { ... })`
* `public class MyJob : JobFacAwareProcessBase { ... }`

JobFac provides a simple one-line builder command to make it easier to develop and test the JobFac-aware job without needing a real JobFac infrastructure. With this command in place, simply start your job like any other console program. The JobFac infrastructure is mocked, and progress information is output to the console as it runs. You might, for example, wrap this in a `#if` block:

* `UseJobFacAwareDevelopmentMode(startupPayload:"foo,bar")`
