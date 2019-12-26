
## JobFac.Services

JobFac service interface definitions as Microsoft Orleans grains. .NET client applications only need to reference this library in order to use JobFac services (Orleans is interface-driven as far as the client is concerned, since clients operate through proxies instead of directly against the grain implementations).

JobFac clients need only inject `IJobFacServiceProvider` -- they don't need to know anything about Orleans or grains. That interface provides three simple methods:

* `GetJobFactory()`
* `GetJob(jobInstanceId)`
* `GetSequence(sequenceInstanceId)`


