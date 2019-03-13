## Versioning strategy

Rdd intends to respect Semantic Versioning 2.0.0 (https://semver.org/), meaning that 

> Given a version number MAJOR.MINOR.PATCH, increment the:
>  
>  1. MAJOR version when you make incompatible API changes,
>  2. MINOR version when you add functionality in a backwards-compatible manner, and
>  3. PATCH version when you make backwards-compatible bug fixes.
>
>  Additional labels for pre-release and build metadata are available as extensions to the MAJOR.MINOR.PATCH format.


Nevertheless, **MINOR version could also be incremented when making minor backwards-incompatible bug fixes**. If a bug cannot be properly fixed without slightly modifying a public API, the bug could be fixed in a MINOR version. A correction leaving the public API intact should always be preferred. Introducing changes on the public API on a minor versions cannot be the result of a new feature.

Here is the agreed upon list of bug fixes that may lead to breaking changes in a MINOR version:
- Typos. Typos on public API's name and arguments' name may be corrected in minor versions. Otherwise, names should remain unaffected, even for API unification and homogeneity, or to promote new API.
- Sync to Async. Some methods might change from a synchronous version to an asynchronous version in a MINOR version, if the current implementation clearly calls for it and is a current source of problems.
- Dangerous or extremely poorly performing API with safe equivalent alternatives. If an existing API standard use-cases are dangerous AND that a safer and equivalent alternative exists, removing the API could be done in a MINOR version. If the API is considered dangerous in edge-cases, it should not be removed in a MINOR version. If the API is dangerous but without alternative, it should not be removed in a MINOR version.
- API behaviour modification in extremely unordinary usages. If an API's behaviour is altered for extremely unordinary usages as a result of new functionality or a bug fix, this could be done in a MINOR version.


Otherwise, the following modifications should **NEVER** happend on a MINOR version:
 - Modifying the arguments of an API
 - Modifying the name of an API -> Except typos
 - Modifying the returned type of an API -> Except Sync->Async
 - Modifying the behaviour of an API -> Except bug fixes and extremely unordinary usages
 - Removing an API -> Except dangerous with safe equivalent alternatives