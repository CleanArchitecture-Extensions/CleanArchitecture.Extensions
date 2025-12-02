# Quickstart

Get an extension running fast. Adjust package names/configs as real packages land.

## 1) Create or open a Clean Architecture solution

Use Jason Taylor's template as the base (see repository root for reference copy).

## 2) Add an extension package

# example placeholder package name

# replace with the actual extension when available

dotnet add src/YourProject/YourProject.csproj package CleanArchitecture.Extensions.Sample
`

## 3) Configure the extension

Add minimal configuration (placeholder example):
`json
{
  "Extensions": {
    "Sample": {
      "Enabled": true
    }
  }
}
`

## 4) Run and verify

`dotnet run --project src/YourProject/YourProject.csproj`
Check logs/output for the extension initializing successfully.

!!! tip
When real packages land, swap the package name/config keys and link to the relevant recipe or extension page.
