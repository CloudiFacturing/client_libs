**DEPRECATED!** This library collection is currently not maintained and therefore does not reflect or support the current state of the SemWES platform. It is therefore **not** recommended to any of the libraries in this repository. Use [clfpy](https://github.com/CloudiFacturing/clfpy) instead.

# CloudFlow client libraries
This repository provides client libraries for various infrastructure
components of the CloudFlow platform.

Everyone is welcome to contribute to this repository by implementing client 
libraries in languages which are not available yet. If you do so, please develop
in a separate branch and create pull requests when ready.

## Available libraries
### Generic libraries
* [clfpy](https://github.com/CloudiFacturing/clfpy): Python package with 
  leightweight SOAP clients to several infrastructure components (GSS, Authentication 
  manager, HPC service, ...). Recommended library for infrastructure access and
  actively maintained.

  Note: Has its own github repository: https://github.com/CloudiFacturing/clfpy

### GSS libraries

* [gssClients/gssCppLibrary](gssClients/gssCppLibrary): C++ library for
  accessing GSS. 

  _Warning:_ Has not yet been tested within the CloudiFacturing project!

* [gssClients/gssCSharpClients](gssClients/gssCSharpClients): C# library for
  accessing GSS.

  _Warning:_ Has not yet been tested within the CloudiFacturing project!

* [gssClients/gssJavaLibrary](gssClients/gssJavaLibrary): Java library for
  accessing GSS.

  _Warning:_ Has not yet been tested within the CloudiFacturing project, and
  might be outdated / not fully functional!

* [gssClients/gssPythonClients](gssClients/gssPythonClients) (OUTDATED): Set of
  Python scripts to access GSS. 

  _Warning:_ This set of scripts is outdated and not well maintained. It is here
  mainly for reference, use the `cfpy` package if you use Python.

### Authentication-manager libraries

* [authenticationClients/authCppLibrary](authenticationClients/authCppLibrary): 
  C++ library for accessing the authentication service. 

  _Warning:_ Has not yet been tested within the CloudiFacturing project!

* [authenticationClients/authCSharpLibrary](authenticationClients/authCSharpLibrary): 
  C# library for accessing the authentication service.

  _Warning:_ Has not yet been tested within the CloudiFacturing project!

