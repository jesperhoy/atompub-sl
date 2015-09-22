# atompub-sl
## Atom Publishing Protocol (AtomPub) server library for ASP.NET

This .NET library is intended for developers who want to implement the Atom Publishing Protocol (AtomPub) on an ASP.NET web-site - so that web-site users can publish news articles (like blog posts) and/or other content using an AtomPub client such as Windows Live Writer.

The library handles all communication with the client - HTTP requests/responses, authentication, parsing and formatting AtomPub XML messages, etc. However, web-site developers must add their own code to retrieve, store, update, and delete feed entries and associated media files from/to some type of permanent storage. This of course also means that developers are free to use any storage format and medium as they see fit.

###Concepts

News articles / blog posts ("entries") and media files (images, video, etc.) are stored to / retrieved from different "collections".
There are two types of collections - "EntryCollections" and "MediaCollections".
An EntryCollection is basically a news feed / blog containing articles.
A MediaCollection fascilitates storing media files and contains "media links" referencing the stored media files (which may be stored elsewhere - like at a CDN).
You must provide (through your code) at least one collection of each type for end-users to be able to post articles containing images or other media.
If you provide multiple collections, Windows Live Writer will ask the end-user which collections to use for what.
You may provide different collections to different users or based on other criteria.
The word "collection" comes from the RFC describing the AtomPub protocol - where a "service document" lists "work spaces" containing "collections". This library supports any number of "collections", but only a single "work space" (we found no need for having more).

###How to use it

Note: All class names from this library mentioned below are from the `JHSoftware.AtomPub` name space.

First copy the "JHAtomPub.dll" file to the "bin" folder of your ASP.NET web-site.
Then create a new .NET class inheriting from `PublicationRequestHandler` and override the required methods and properties.
The overridden `GetCollections` method must return a set of objects each inheriting from `EntryCollection` or `MediaCollection` (see Concepts above). Override methods of these collection objects to create, update, fetch, and delete feed entries and media.

Then add a handler for your class to your web.config file (below is for IIS7 / ASP.NET 4.0 - differs for other versions):

```XML
<configuration>
   <system.webServer> 
      <handlers> 
         <add name="AtomPub" path="atompub" verb="*" type="YourClassName" 
              resourceType="Unspecified" preCondition="integratedMode" />
```

Finally enable AtomPub auto-discovery by linking to the handler from your site's default web-page:

```HTML
<html>
   <head>
      <link rel="service" type="application/atomsvc+xml" href="http://yoursite.com/atompub" />
```

Now you can point Windows Live Writer (or similar) to http://yoursite.com and you are ready to publish!

###User authentication

User authentication is required for all publishing requests with this library.
If your user accounts are stored in a database or similar, you can use the authentication feature built into this library (based on HTTP Basic Authentication).
If your users have Windows user accounts on the web-server or in your activate directory, you can use Windows authentication through one of the IIS supplied authentication methods.
You control which method is used through the `PublicationRequestHandler.UseWindowsAuthentication` property.

If `UseWindowsAuthentication` is `false`, you must disable all the authentication methods for the website in IIS - except "Anonymous authentication".
The `PublicationRequestHandler.Authenticate` method will be called for each publishing request - allowing you to verify the user name/password in your own code.
A `system.security.Principal.GenericIdentity` for the authenticated user will be passed to all methods that have a `user` parameter.

If `UseWindowsAuthentication` is `true`, you must enable one of the authentication methods for the website in IIS (Note that Forms authentication does not work with Windows Live Writer).
The `PublicationRequestHandler.Authenticate` method will not be called.
A `system.security.Principal.WindowsIdentity` object for the authenticated Windows user will be passed to all methods that have a `user` parameter.

In overridden methods which have a `user` parameter, you should throw a `ForbiddenException` whenever that user is not permitted to perform the requested action. This ensures that an appropriate HTTP response status code (403) is returned to the client application.

###About IDs

The library uses a simple string to `ID` field to identify collection, entry, and medialink items (required field for all 3 types). These IDs are used as parameters in various URLs passed to the client, and therefore should be relatively short.
Collection IDs must be unique for the service. Entry and medialink IDs must be unique within the collection that they belong to.
Other than this, you can pretty much use any format you want - for example integer values or GUIDs matching record keys in a database, or simply file names.

Externally (in published feeds) each collection (feed) and each entry (article) must have a permanent globally unique ID.
By default, the library generates a GUID for this by calculating a hash value based the collection and entry IDs and the the web-site's domain name (excluding www prefix).
You can override this by setting the `OverrideGLobalID` field value ([IRI format](http://www.ietf.org/rfc/rfc3987.txt)) on each item. You should override if the feed may be accessed with different domain names or if you are using this to replace an existing feed (to preserve the global IDs).

###About "Slugs"

Two methods that you override, `EntryCollection.CreateEntry` and `MediaCollection.CreateMedia`, each take a "Slug" parameter. A "slug" is a suggested name for the resource being created. You can use this to as part of a generated URL or file name if you wish, but this is not required.
Note that the slug parameter may be null/nothing if the client application did not provide a value.

###Syndication

This library is primarily intended for publishing - that is creating / updating feed entries.
However it does also provide objects/methods for Atom syndication - serving (read-only) feeds.
The simplest way to do this is using an .ashx file (Generic Handler). In the `ProcessRequest` method, new up a `SyndicationCollection`, set its field values, and call its `Render` method.

###NOT compatible with WebDAV

AtomPub uses HTTP "PUT" and "DELETE" methods to update/delete feed entries.
The WebDAV protocol uses these same methods to update / delete web-site files directly.
Therefore, unfortunately, it is not possible to use AtomPub and WebDAV on the same web-site.
If your IIS server has the WebDAV module installed you will need to remove it (disabling is not enough) from web-sites using AtomPub, either through the IIS manager or in their web.config files (below is for IIS7 - may differ for other versions):

```XML
<configuration>
   <system.webServer>
      <modules>
         <remove name="WebDAVModule" />
```

###Download

Download JH Software's AtomPub server library for ASP.NET v. 1.0 (August 30th 2010):

Compiled for .NET 4.0:  [JHSoftware.AtomPub-DN4.zip](https://github.com/jhsoftware/atompub-sl/releases/download/1.0/JHSoftware.AtomPub-DN4.zip) (12 KB)

Compiled for .NET 2.0 - 3.5:  [JHSoftware.AtomPub-DN2.zip](https://github.com/jhsoftware/atompub-sl/releases/download/1.0/JHSoftware.AtomPub-DN2.zip) (12 KB)

###Sample implementation

We have written a sample web-site that uses local web-server memory to store entries and media (meaning that a web-site restart wipes the data).
Production code should obviously store data to a more durable medium such as a database, file system, CDN, etc.
The sample is written in VB.NET (ASP.NET 4.0) and runs on IIS 7 and later. It is of course also possible to use the librabry with other .NET languages (C#, F#, etc.) and the configuration can be modified to work with earlier versions of IIS.
Note we have not been able to run this under the Visual Studio ASP.NET Development Server - only under the full IIS (some type of authentication issue).
Download sample web-site:

 [JHSoftware.AtomPub.sample.zip](https://github.com/jhsoftware/atompub-sl/releases/download/1.0/JHSoftware.AtomPub.Sample.zip) (17 KB)

###References

[RFC4287 - The Atom Syndication Format](http://www.ietf.org/rfc/rfc4287.txt)

[RFC5023 - The Atom Publishing Protocol](http://www.ietf.org/rfc/rfc5023.txt)

[Windows Live Writer (Part of Windows Essentials)](http://windows.microsoft.com/en-us/windows-live/essentials-other)

[MSDN - Windows Live Writer Provider Customization API](http://msdn.microsoft.com/en-us/library/bb463266.aspx)

[W3C Feed Validation Service](http://validator.w3.org/feed/)

[Wikipedia - Atom (standard)](https://en.wikipedia.org/wiki/Atom_(standard))
