![OpenSky](https://raw.githubusercontent.com/opensky-to/branding/master/png/OpenSkyLogo_Banner64.png)

[![Discord](https://img.shields.io/discord/837475420923756544.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.com/invite/eR3yePrj79)
[![Facebook](https://img.shields.io/badge/-OpenSky-e84393?label=&logo=facebook&logoColor=ffffff&color=6399AE&labelColor=00C2CB)](https://www.facebook.com/Opensky.to/)
![Maintained][maintained-badge]
[![Make a pull request][prs-badge]][prs]
[![License][license-badge]](LICENSE.md)

OpenSky is an open-source airline management simulation currently in development. We are actively seeking aviation enthusiast whom would love to be part of this upcoming project and shape it with us! If you have experience in coding, graphical or game design and feel like you could be an asset to the project, please head over to the [contribute page](https://www.opensky.to/contribute) and do not hesitate to jump into our [Discord](https://discord.com/invite/eR3yePrj79) and say hello! We would love to hear your ideas and feedback and are actively collecting them in our [forums](https://forum.opensky.to/)!

## OpenSky S2Geometry

This repository contains the C# code for handling S2 Geometry calculations used in OpenSky, published as a [nuget package](https://www.nuget.org/packages/OpenSky.S2Geometry/).

This is a fork from novotnyllc's C# port of Google's S2 Geometry Library from both Java and C++, the aim of this fork is to provide a shared version for both .net 4.8 and 5/6 with some added convinience methods
https://github.com/novotnyllc/s2-geometry-library-csharp
https://code.google.com/p/s2-geometry-library-java/
https://code.google.com/p/s2-geometry-library/

This library is can be used to create GeoHashes for fast querying. The Java version is used by AWS for 
GeoSpatial queries in DynamoDB.

S2 uses Hilbert Curves extensivly. 
For more info, see the original google presentation https://docs.google.com/presentation/d/1Hl4KapfAENAOf4gv-pSngKwvS_jwNVHRPZTTDzXXn6Q/view

## License

Original source code and assets and present in this repository are licensed under the MIT license.

[maintained-badge]: https://img.shields.io/badge/maintained-yes-brightgreen
[license-badge]: https://img.shields.io/badge/license-MIT-blue.svg
[license]: https://github.com/maximegris/angular-electron/blob/master/LICENSE.md
[prs-badge]: https://img.shields.io/badge/PRs-welcome-red.svg
[prs]: http://makeapullrequest.com
