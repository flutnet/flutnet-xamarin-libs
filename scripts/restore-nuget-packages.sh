#!/usr/bin/env bash

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
MSBUILD_PATH=/Library/Frameworks/Mono.framework/Versions/Current/bin/msbuild

# The following commands do NOT work for Xamarin projects
# https://xamarin.github.io/bugzilla-archives/58/58254/bug.html
# dotnet restore "$SCRIPT_DIR/../Flutnet.sln"
# dotnet restore "$SCRIPT_DIR/../samples/Flutnet.Samples.sln"

"$MSBUILD_PATH" /t:restore "$SCRIPT_DIR/../Flutnet.sln"
"$MSBUILD_PATH" /t:restore "$SCRIPT_DIR/../samples/Flutnet.Samples.sln"