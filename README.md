# [Flutnet](https://www.flutnet.com) runtime libraries

<img src="github_assets/flutnet_prj_structure_cross_platform_code_200px.jpg" width="35%" style="background-color:white; padding:0px;" />

This repository provides all the code to build your own nuget packages [Flutnet.iOS](https://www.nuget.org/packages/Flutnet.iOS/) and [Flutnet.Android](https://www.nuget.org/packages/Flutnet.Android/). 

This packages contains both the [FlutnetRuntime](https://www.flutnet.com/Documentation/Reference/Flutnet-Runtime) and the [FlutnetBrigde](https://www.flutnet.com/Documentation/Reference/Flutnet-Bridge) used to provide the communication between the Flutter and Xamarin for your Flutnet Project.

## What is Flutnet?

Flutnet is a set of tools and libraries that will make super easy embed Flutter (graphical framework) inside a Xamarin application (as a backend layer) to obtain a powerful and native cross platform application. For more detailts see https://www.flutnet.com/.

## Build requirements

In order to build these packages you need to have already builded the [Flutnet Xamarin Bindings Libraries](https://github.com/flutnet/flutnet_xamarin-bindings).

## Build procedure

>The following procedure refer to a **macOS system**, because we need a macOS machine in order to build both Android and iOS.

The build process is composed by:
 - [Build the source projects](#build-the-source-projects)
 - [Create Nuget packages](#create-nuget-packages)
 - [Build the Sample](#build-the-sample)

### Build the source projects

Before building the source code ensure to have already builded the [Flutnet Xamarin Bindings Libraries](https://github.com/flutnet/flutnet_xamarin-bindings). Ensure to clone `flutnet_xamarin-bindings` and `flutnet_xamarin-libs` projects in the same parent directory.

<img src="github_assets/folders.png" width="40%" />

Run the following script in order to build the projects **Flutnet.Android** and **Flutnet.iOS** inside the `Flutnet.sln` solution.

```sh
scripts/build-src.sh
```

>NOTE: Visual studio build tool know where to find the nuget packages because the information is indicated inside the `NuGet.Config` file.

### Create Nuget packages

From the projects already builded we now can create the nuget packages.

Run
```sh
scripts/pack.sh
```

All the packages will be placed inside `artifacts/nuget-packages/` folder:
- Flutnet.Android.2.0.0.nupkg
- Flutnet.iOS.2.0.0.nupkg
- Flutnet.ServiceModel.2.0.0.nupkg

<img src="github_assets/packages.png" width="50%" />

### Build the Sample

The sample project is placed in `samples/` folder is a fully Flutnet sample app for both Android and iOS. The build process will build the `samples/Flutnet.Samples.sln` file.

Run
```sh
scripts/build-samples.sh
```

At the first time, the build process will require more time because needs to build the Flutter Module used inside the Xamarin app. Check the [Flutnet Architecture](https://www.flutnet.com/Documentation/Introduction/Flutnet-Architecture) for more details.

>NOTE: Visual studio know where to find the nuget packages because the information is indicated inside the `samples/NuGet.Config` file.

### Run the project on a device

You can alwais open the `samples/Flutnet.Samples.sln` in Visual Studio running the project on a real device. If you want to run the project for iOS ensure to setup your apple-ID in Visual Studio.