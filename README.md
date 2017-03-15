DevSkim Extension for Visual Studio
===================================

The extension implements a security linter within the Visual Studio 2015, leveraging the rules from the [DevSkim](https://github.com/Microsoft/DevSkim) repo. It helps software engineers to write secure code by flagging potentially dangerous calls, and gives in-context advice for remediation.

### PUBLIC PREVIEW

DevSkim is currently in *public preview*. We're looking forward to working with the community
to improve both the scanning engines and rules over the next few months, and welcome your feedback
and contributions!

![DevSkim Demo](https://github.com/Microsoft/DevSkim-VisualStudio-Extension/raw/master/doc/DevSkim-VisualStudio-Demo-1.gif)

Installation
------------
Clone the extensions and rules repos directly, build it and install the `DevSkim.vsix` file. This extension will be available in the Visual Studio marketplace shortly.

Platform support
----------------
#### Operating System:

Microsoft Windows 7 and later

#### Visual Studio:

The extension requires [Visual Studio 2015](https://www.visualstudio.com/vs/community/) and higher

Rules System
------------

The extension supports both built-in and custom rules:

#### Built-In Rules

Built-in rules come from the [DevSkim](https://github.com/Microsoft/DevSkim) repo, and must be stored
in the `rules` directory within the `Microsoft.DevSkim.VSExtension` directory.

Rules are organized by subdirectory and file, but are flattened internally when loaded.

Each rule contains a set of patterns (strings and regular expressions) to match, a list of file types to
apply the rule to, and, optionally, a list of possible code fixes. For more information on rules format, see [WiKi](https://github.com/Microsoft/DevSkim/wiki/Writing-Rules).


Reporting Issues
-------
Please see [CONTRIBUTING](https://github.com/Microsoft/DevSkim-VisualStudio-Extension/blob/master/CONTRIBUTING.md) for information on reporting issues and contributing code.

Tips and Known Issues
----
See tips and known issues in the [wiki page](https://github.com/Microsoft/DevSkim-VisualStudio-Extension/wiki/Tips-and-Known-Issues).
