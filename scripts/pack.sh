#!/usr/bin/env bash

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
ARTIFACTS_DIR=$SCRIPT_DIR/../artifacts
SRC_DIR=$SCRIPT_DIR/../src
TOOLS_DIR=$SCRIPT_DIR/../tools
NUGET_EXE=$TOOLS_DIR/nuget/nuget.exe

# Create NuGet packages for binding libraries
mono "$NUGET_EXE" pack "$SRC_DIR/Flutnet.Android/Flutnet.Android.nuspec" -OutputDirectory "$ARTIFACTS_DIR/nuget-packages"
mono "$NUGET_EXE" pack "$SRC_DIR/Flutnet.iOS/Flutnet.iOS.nuspec" -OutputDirectory "$ARTIFACTS_DIR/nuget-packages"
mono "$NUGET_EXE" pack "$SRC_DIR/Flutnet.ServiceModel/Flutnet.ServiceModel.nuspec" -OutputDirectory "$ARTIFACTS_DIR/nuget-packages"
