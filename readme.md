# ![RealWorld Example App](logo.png)

> ### [Enterprise Web Library](http://enterpriseweblibrary.org/) codebase containing real world examples (CRUD, auth, advanced patterns, etc) that adheres to the [RealWorld](https://github.com/gothinkster/realworld) spec and API.


### [Demo](http://ewl.enterpriseweblibrary.org/RealWorld)&nbsp;&nbsp;&nbsp;&nbsp;[RealWorld](https://github.com/gothinkster/realworld)

This codebase was created to demonstrate a fully fledged fullstack application built with the **[Enterprise Web Library](http://enterpriseweblibrary.org/)** including CRUD operations, authentication, routing, pagination, and more.

We’ve gone to great lengths to adhere to the **[Enterprise Web Library](http://enterpriseweblibrary.org/)** community styleguides & best practices.


# How it works

The `Library` project contains configuration files, provider classes (which allow parts of EWL’s behavior to be customized), and a `GlobalInitializer` class (which gives you a place to initialize static fields when your system starts up). `Library` is also the home of the “business logic” and anything else that you would reuse across multiple applications in the system. This RealWorld example system only contains a single web app, but larger systems could contain additional web apps, or a different type of application, e.g. a Windows service.

The `Website` project, which references `Library`, contains your pages and other resources.

More information is available from our developers; please [ask for help in the forum](https://community.enterpriseweblibrary.org/).


# Getting started


## Requirements

*	Windows 10, .NET Framework 4.6.2, IIS Express 10 (.NET Core support is on the roadmap!)
*	Visual Studio 2017 (recommended)
* SQL Server 2016 or later; if you don’t have this, Express edition is [free and available for download](https://www.microsoft.com/en-us/sql-server/sql-server-editions-express)


## Building and running the system

1.	Clone or download this repository.

2.	Open the solution file in Visual Studio. Ignore the message that “one or more projects in the solution were not loaded correctly”; the next step will resolve this by generating the missing file.

3.	In the Package Manager Console, perform these steps:

	1. Click the `Restore` button.

	2. **If you’re using SQL Server Express edition,** run `Initialize-InstallationConfiguration`. Then open `Library/Configuration/Installation/Installations/Development/Standard.xml` and change `<database xsi:type="SqlServerDatabase" />` to `<database xsi:type="SqlServerDatabase"><server>(local)\SQLExpress</server></database>`.

	3. Run `Update-Data` (to create the database) and `Update-DependentLogic` (to configure IIS Express and generate some code).

4.	Right-click the `Website` project and select `Reload Project`.

5.	Run the `Website` project. You should see the home page.