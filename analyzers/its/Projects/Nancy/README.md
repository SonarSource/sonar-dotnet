# ** Announcement ** - Nancy is no longer being maintained! 

We would like to thank all the thousands of users of Nancy, all the people who wrote blog posts, conference speakers, video producers and those that spread the word of Nancy.  

We would like to thank the 150+ contributors to Nancy that made it what it became, without you the work would have been much harder and opportunities missed.  

We would like to thank [VQ](http://www.vqcomms.com) for financially sponsoring our open source efforts.  

We would like to thank the core contributors to Nancy [@jchannon](https://github.com/jchannon), [@khellang](https://github.com/khellang), [@damianh](https://github.com/damianh), [@phillip-haydon](https://github.com/phillip-haydon), [@prabirshrestha](https://github.com/prabirshrestha), [@horsdal](https://github.com/horsdal) for working hard into the nights coding, testing and writing docs but most importantly the founders of Nancy itself [@thecodejunkie](https://github.com/thecodejunkie) and [@grumpydev](https://github.com/grumpydev) whose vision made Nancy what it was, a fun, performant and enjoyable web framework.

## Support
We understand that organisations may have services and products that still depend on Nancy in production. A couple of members of the team can offer a support, maintenance, migration services on commercial terms. Please reach out to [nancyfx.help@gmail.com](mailto:nancyfx.help@gmail.com) to discuss options.

## Forking
Nancy's licence is permissible so we encourage forking if you need to perform maintenance. However, the logos and name are copyright to Andreas Håkansson and Steven Robbins and are not for re-use or editing. Please see full licence information [here](https://github.com/NancyFx/Nancy.Portfolio/blob/master/license.txt)


------------------------------------------------------------------------------------------------------------------------------------------
# Meet Nancy [![NuGet Version](http://img.shields.io/nuget/v/Nancy.svg?style=flat)](https://www.nuget.org/packages/Nancy/) [![Slack Status](http://slack.nancyfx.org/badge.svg)](http://slack.nancyfx.org)

Nancy is a lightweight, low-ceremony, framework for building HTTP based services on .NET Framework/Core and [Mono](http://mono-project.com). The goal of the framework is to stay out of the way as much as possible and provide a super-duper-happy-path to all interactions.

Nancy is designed to handle `DELETE`, `GET`, `HEAD`, `OPTIONS`, `POST`, `PUT` and `PATCH` requests and provides a simple, elegant, [Domain Specific Language (DSL)](http://en.wikipedia.org/wiki/Domain-specific_language) for returning a response with just a couple of keystrokes, leaving you with more time to focus on the important bits..
**your** code and **your** application.

Write your application
```csharp
public class Module : NancyModule
{
    public Module()
    {
        Get("/greet/{name}", x => {
            return string.Concat("Hello ", x.name);
        });
    }
}
```

Compile, run and enjoy the simple, elegant design!

## Features

* Built from the bottom up, not simply a DSL on top of an existing framework. Removing limitations and feature hacks of an underlying framework, as well as the need to reference more assemblies than you need. _keep it light_
* Run anywhere. Nancy is not built on any specific hosting technology can be run anywhere. Out of the box, Nancy supports running on ASP.NET/IIS, WCF, Self-hosting and any [OWIN](http://owin.org)
* Ultra lightweight action declarations for GET, HEAD, PUT, POST, DELETE, OPTIONS and PATCH requests
* View engine integration (Razor, Spark, dotLiquid, our own SuperSimpleViewEngine and many more)
* Powerful request path matching that includes advanced parameter capabilities. The path matching strategy can be replaced with custom implementations to fit your exact needs
* Easy response syntax, enabling you to return things like int, string, HttpStatusCode and Action<Stream> elements without having to explicitly cast or wrap your response - you just return it and Nancy _will_ do the work for you
* A powerful, light-weight, testing framework to help you verify the behavior of your application
* Content negotiation
* And much, much more

## The super-duper-happy-path

The "super-duper-happy-path" (or SDHP if you’re ‘down with the kids’ ;-)) is a phrase we coined to describe the ethos of Nancy; and providing the “super-duper-happy-path” experience is something we strive for in all of our APIs.

While it’s hard to pin down exactly what it is, it’s a very emotive term after all, but the basic ideas behind it are:

* “It just works” - you should be able to pick things up and use them without any mucking about. Added a new module? That’s automatically discovered for you. Brought in a new View Engine? All wired up and ready to go without you having to do anything else. Even if you add a new dependency to your module, by default we’ll locate that and inject it for you - no configuration required.
* “Easily customisable” - even though “it just works”, there shouldn’t be any barriers that get in the way of customisation should you want to work the way you want to work with the components that you want to use. Want to use another container? No problem! Want to tweak the way routes are selected? Go ahead! Through our bootstrapper approach all of these things should be a piece of cake.
* “Low ceremony” - the amount of “Nancy code” you should need in your application should be minimal. The important part of any Nancy application is your code - our code should get out of your way and let you get on with building awesome applications. As a testament to this it’s actually possible to fit a functional Nancy application into a single Tweet :-)
* “Low friction” - when building software with Nancy the APIs should help you get where you want to go, rather than getting in your way. Naming should be obvious, required configuration should be minimal, but power and extensibility should still be there when you need it.

Above all, creating an application with Nancy should be a pleasure, and hopefully fun! But without sacrificing the power or extensibility that you may need as your application grows.

## Getting started

As a start several working samples are provided in the `/sample` directory. Simply run the build script `build.ps1` (for Windows PowerShell) or `build.sh` (for \*nix-Bash) first.

## Community

Nancy followers can be found on Slack [NancyFx team](http://nancyfx.slack.com). You can also find Nancy on Twitter using the #NancyFx hashtag.

## Help out

There are many ways you can contribute to Nancy. Like most open-source software projects, contributing code
is just one of many outlets where you can help improve. Some of the things that you could help out with in
Nancy are:

* Documentation (both code and features)
* Bug reports
* Bug fixes
* Feature requests
* Feature implementations
* Test coverage
* Code quality
* Sample applications

## Continuous integration builds

| Platform                    | Status                                                                                                                                  |
|-----------------------------|-----------------------------------------------------------------------------------------------------------------------------------------|
| AppVeyor (.NET & .NET Core) | [![Build Status](https://ci.appveyor.com/api/projects/status/mpd9lbxvithu16vg/branch/master?svg=true)](https://ci.appveyor.com/project/NancyFx/nancy) |
| Travis (Mono)               | [![Build Status](https://travis-ci.org/NancyFx/Nancy.png?branch=master)](https://travis-ci.org/NancyFx/Nancy)                           |

To get build artifacts of latest `master`, please use our [MyGet feed](https://www.myget.org/gallery/nancyfx)

## Contributors

Nancy is not a one man project and many of the features that are available would not have been possible without the awesome contributions from the community!

For a full list of contributors, please see [the website](http://www.nancyfx.org/contribs.html).

## Code of Conduct

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/) to clarify expected behavior in our community. For more information see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).

## Contribution License Agreement

Contributing to Nancy requires you to sign a [contribution license agreement](https://cla2.dotnetfoundation.org/) (CLA) for anything other than a trivial change. By signing the contribution license agreement, the community is free to use your contribution to .NET Foundation projects.

## .NET Foundation

This project is supported by the [.NET Foundation](http://www.dotnetfoundation.org).

## Copyright

Copyright © 2010 Andreas Håkansson, Steven Robbins and contributors

## License

Nancy is licensed under [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Refer to license.txt for more information.
