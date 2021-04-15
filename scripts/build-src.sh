#!/usr/bin/env bash

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
SRC_DIR=$SCRIPT_DIR/../src
VSTOOL_PATH="/Applications/Visual Studio.app/Contents/MacOS/vstool"
MSBUILD_PATH=/Library/Frameworks/Mono.framework/Versions/Current/bin/msbuild
SOLUTION_PATH=$SCRIPT_DIR/../Flutnet.sln

# Restore Nuget Packages
# The following commands do NOT work for Xamarin projects
# https://xamarin.github.io/bugzilla-archives/58/58254/bug.html
# dotnet restore "$SOLUTION_PATH"
"$MSBUILD_PATH" /t:restore "$SOLUTION_PATH"

# Clean and build
"$VSTOOL_PATH" build --configuration:Debug --target:Clean "$SOLUTION_PATH"
"$VSTOOL_PATH" build --configuration:Release --target:Clean "$SOLUTION_PATH"

"$VSTOOL_PATH" build --configuration:Debug "$SOLUTION_PATH"
"$VSTOOL_PATH" build --configuration:Release "$SOLUTION_PATH"