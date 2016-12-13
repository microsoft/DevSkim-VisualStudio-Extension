DevSkim Plugin for Visual Studio
================================

The plugin implements a security linter within the Visual Studio 2015, leveraging the rules from the [DevSkim-Rules](https://github.com/Microsoft/DevSkim-Rules) repo. It helps software engineers to write secure code by flagging potentially dangerous calls, and gives in-context advice for remediation.

Requirements
--------------

The plugin requires Visual Studio 2015

Installation
------------
If using [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/), simply install the `DevSkim` package.

Alternatively, you can clone the plugin and rules repos directly, build it and install the `DevSkim.vsix` file

Platform support
----------------
#### Operating System:

Microsoft Windows 7 and later

#### Sublime Text Version:

The plugin requires [Visual Studio 2015 Community](https://www.visualstudio.com/vs/community/) or commercial version

Rules System
------------

The plugin supports both built-in and custom rules:

#### Built-In Rules

Built-in rules come from the [DevSkim](https://github.com/Microsoft/DevSkim) repo, and should be stored
in the `rules` directory within the DevSkim directory.

Rules are organized by subdirectory and file, but are flattened internally when loaded.

Each rule contains a set of patterns (strings and regular expressions) to match, a list of file types to
apply the rule to, and, optionally, a list of possible code fixes. An example rule is shown below:

```
[
    {
        "id": "DS126858",
        "name": "Weak/Broken Hash Algorithm",
        "active": true,
        "tags": [
            "Cryptography.BannedHashAlgorithm"
        ],
        "severity": "critical",
        "description": "A weak or broken hash algorithm was detected.",
        "replacement": "Consider switching to use SHA-256 or SHA-512 instead.",
        "rule_info": "https://github.com/microsoft/devskim/guidance/DS126858.md",
        "applies_to": [
            "$PHP"
        ],
        "patterns": [
            {
                "pattern": "md5(",
                "type": "string"
            },
        ],
        "fix_it": [
            {
                "type": "regex_substitute",
                "name": "Change to SHA-256",
                "search": "\\bmd5\\(([^\\)]+\\)",
                "replace": "hash('sha256', \\1)"
            }
        ]
    }
]
```

Screenshots
------

TODO

Reporting Issues
-------
Please see [CONTRIBUTING](https://github.com/Microsoft/DevSkim-VisualStudio-Plugin/blob/master/CONTRIBUTING.md) for information on reporting issues and contributing code.

Tips and Known Issues
----
See tips and known issues in the [wiki page](https://github.com/Microsoft/DevSkim-VisualStudio-Plugin/wiki/Tips-and-Known-Issues).
