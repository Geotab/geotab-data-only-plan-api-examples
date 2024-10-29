# Geotab Data-Only Plan API Examples

The *Geotab Data-Only Plan API Examples* solution is a Visual Studio solution that demonstrates usage of the [Geotab Data-Only Plan API](https://www.nuget.org/packages/Geotab.DataOnlyPlan.API/). For detailed information, refer to the [Geotab Data-Only Plan API (.NET) - Developer Guide [PUBLIC]](https://docs.google.com/document/d/1BEYfMMujh1oud5Xpkw-rQO5eqX4dNXnt_9TO_jdTrA0/edit#).

## Prerequisites

The solution requires:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or higher
- Geotab.Checkmate.ObjectModel (included with the NuGet package)
- Ideally, MyAdmin credentials with the *MyAdminApiUser* and *Device_Admin* roles should be used when authenticating in order to be able to take advantage of all available methods; MyGeotab credentials may be used, but then the *CreateDatabaseAsync()* method cannot be used.

## Getting started

**IMPORTANT:**  See the [Geotab Data-Only Plan API (.NET) - Developer Guide [PUBLIC]](https://docs.google.com/document/d/1BEYfMMujh1oud5Xpkw-rQO5eqX4dNXnt_9TO_jdTrA0/edit#) before attempting to run these examples; the developer guide contains important information about necessary configuration.

```shell
> git clone https://github.com/Geotab/geotab-data-only-plan-api-examples.git geotab-data-only-plan-api-examples
```