# TranslateKit

TranslateKit a .NET library and set of tools for building language translations
of C# projects.

Features include:

* A simple mechanism to annotate strings in code that need translation
* Command line tool to extract translatable strings from code to JSON file.
* Ability to update extracted translations without overwriting existing translation work.
* Machine translation provides good starting point for translation work
* Various means to provide context information and comments to assist human translators
* Small helper library to do the actual runtime string replacements from the JSON file. (optional, custom translator functions can be used if preferred)



## Step 1 - Add a Reference to the Library

The first step is to add a reference to `Topten.TranslateKit` library from nuget.


## Step 2 - Decorate Translatable Strings

Not every string in your project needs to be translated.  Decorate those that
are language sensitive and need translation by suffixing them with `.T()`.

```csharp
// required to get the T() extension method (or see below)
using Topten.TranslateKit;

Console.WriteLine("Hello World".T())
```


## Step 3 - Install the TranslateTool

The TranslateTool provides various commands for working with translation files

```
> dotnet tool install -g Topten.TranslateTool
```

Check it's installed by running:

```
> translatetool --version
```

## Step 4 - Extract the Translatable Strings to a JSON file

Next you run the translate tool to extract the translatable strings:

```
translatetool extract --t --json --out:extracted.json *.cs
```

This will produce a JSON file like so:

```json
[
	{
		"phrase": "Hello World!",
		"context": null,
		"comment": null,
		"locations": [],
		"translation": null,
		"machine": false
	},
]
```

## Step 5 - Create the Per-Language Translation Files

Once the strings have been extracted that file can be used to create or update a JSON file for each target 
language using the `update` command.

eg: suppose you're maintaining an Italian and French translation:

```
translatetool update extracted.json strings-it.json
translatetool update extracted.json strings-fr.json
```

## Step 6 - Start with a Machine Translation

Rather than having to manually translate every string, you can start with a 
Google Translate machine translation:

```
translatetool translate strings-it.json --apikey:XXXXXXXXXXXXXXXXXXXXXXXXXX
translatetool translate strings-fr.json --apikey:XXXXXXXXXXXXXXXXXXXXXXXXXX
```

This will translate and update any strings that currently don't have a 
translation.  Further it will mark any newly translated strings by setting
the `machine` setting to `true` indicating the string still needs to be reviewed.

You should end up with a JSON file like this:

```json
[
	{
		"phrase": "Hello World!",
		"context": null,
		"comment": null,
		"locations": [],
		"translation": "Ciao mondo!",
		"machine": true
	},
]
```

## Step 7 - Review the Translation

Typically the next step would be for a human translator to review the translation
file, checking and updating each translated string.  

After reviewing each string the `machine` flag should be set to `false` (or the 
entry simply removed) to mark the string as having been reviewed and doesn't need 
further attention.

## Step 8 - Loading the Translation

Once you have your translation files, you need to configure your program 
to load the JSON files and map the original strings in code to the translated string from the JSON file.  

TranslateKit includes some helper functions for this:

```csharp
// Load strings JSON file from same directory as executable
var baseDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
var stringsFile = System.IO.Path.Combine(baseDir, "strings-it.json");

// Create and install a translator function.  This function will be called
// by the T() extension method.
StringTranslationExtensions.Translate = Translator.CreateTranslator(stringsFile);
```

You can now run your program and you'll get the translated string:

```
Ciao mondo!
```


## Step 9 - Maintaining the Translation Files

As you continue development of your project you'll undoubtedly add new strings
that need to be translated.  Simply re-run steps 4 to 7:

* the existing strings in the translation file will be kept.
* new strings will be machine translated
* removed strings in the app, will be removed from the translation files.

You should then manually review the translation again, just checking the 
strings that have the `machine` flag set to true. Use the `translatetool list --todo` command to list out the strings that still need attention.



## Providing Your Own T() implementation

If you don't want to reference the `Topten.TranslateKit` library you can manually
provide your own `T()` extension method:

```csharp
static class TranslateExtensions
{
    public static string T(this string This, string context)
    {
        // Do you own work here to load the JSON file and 
        // provide the translated version
    }
}
```


## Context Sensitive Strings

Sometimes the same string will be used in different places with different
meanings.  In these cases you can qualify the string with a context:

```csharp
// Here the term "block" is used in the sense of obstructing something
Console.WriteLine("Block".T("obstruct"));

// Here it's used to mean a geometric block
Console.WriteLine("Block".T("cube"));
```

In this case there will be two entries in the JSON file each with a different context allowing each to have a 
different translation.


## Comments

You can assist your translators by providing additional notes
about how the string is being used by adding  
comments inside the `T()` function call.

To extend the above example further

```csharp
// Here the term "block" is used in the sense of obstructing something
Console.WriteLine("Block".T("obstruct" /* as in "obstruct" */));

// Here it's used to mean a geometric block
Console.WriteLine("Block".T("cube" /* as in "a brick" */));
```

These comments will be included in the generated JSON file:

```json
[
	{
		"phrase": "Block",
		"context": "obstruct",
		"comment": "as in \"obstruct\"",
		"locations": [],
		"translation": null,
		"machine": false
	},
	{
		"phrase": "Block",
		"context": "cube",
		"comment": "as in \"a brick\"",
		"locations": [],
		"translation": null,
		"machine": false
	}
]
```

## Locations

The generated JSON can also include a list of all the filenames where the string
is used and can be used to help clarify the context by showing where the string
was extracted from.  This is just a hint to provide extra context to the 
person doing the translation work.

```json
[
	{
		"phrase": "Hello World!",
		"context": null,
		"comment": null,
		"locations": 
		[
			".\\Program.cs"
		],
		"translation": null,
		"machine": false
	},
]
```

## License

Copyright © 2014-2021 Topten Software.  
All Rights Reserved.

Licensed under the Apache License, Version 2.0 (the "License") you may not use this
product except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under
the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.</p>
