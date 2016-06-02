SMV - Static Module Verifier
============================

This is the core project that enables users to build and verify their
modules using SMV. Currently SMV supports multiple build environments,
and produces IR that is based on the SLAM toolchain. Currently, SMV is
capable of producing IR in the LI format for C/C++ projects that are
using the following build environments: Razzle, MSBuild, CoreXT, and
Make. SMV can be easily configured to use any other toolchain in mind
for producing different IR.

# Building
- Prerequisites:
  + Visual Studio 2015
  + Microsoft Azure SDK for VS 2015: version 2.9 .NET
  + NuGet with default configurations
- Please clone the SMV sources from https://staticmoduleverifier.visualstudio.com/SMV/_git/SMV
- After cloning, you should be able to open the smv.sln file in your repository in VS2015
- The solution should build as is for all configurations and platforms

# Installing
Installing SMV requires that the relevant binaries are gathered in a
single folder. For this purpose, the deployment folder contains
scripts to create such deployments. After gathering the relevant
binaries, SMV expects that analysis plugins are created in an
analysisPlugins folder which resides at the same level as the bin
folder (where you placed all the binaries).

Interception can be performed based on an intercept.xml file that
needs to be present in the same folder as the interceptor
binaries. For example, if you are intercepting cl.exe, you need to
copy interceptor.exe to cl.exe and have an intercept.xml file in the
same folder.

The final directory structure should look as follows:

- %SMV%
  + bin: contains all binaries, and today, the intercept.xml as well
  + analysisPlugins: contains sub folders that have analysis plugins
    * SDV: Static Driver Verifier analysis plugin
    - bin: binaries that are SDV specific. Usually also a cmd script that
      wraps calls to smv.exe
    - configurations: SMV configurations for build and analysis
      - ...: any other folders you need for your plugin

# SMV and the Azure Cloud
- Deploying to Azure
  + You will need a valid subscription in Azure
  + You will need to create the following:
    * A storage account
    * A service bus namespace
  + Once those are created, you can deploy the SMV Worker Role project directly from VS2015
  + Edit the following files for the connection strings for your newly created services:
    * SmvCloud\ServiceConfiguration.Cloud.cscfg
    * SmvLibrary\CloudConfig.xml
    * SmvCloudWorkerContent\CloudConfig.xml
- To use the cloud, you can add *executeOn="cloud"* to any action tag,
  and it will execute using the SMV cloud