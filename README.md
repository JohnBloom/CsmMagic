## Synopsis

An ORM for Cherwell Service Management.

## Get the Package

You can get CsmMagic by [grabbing the latest NuGet package](https://www.nuget.org/packages/CsmMagic/).

## Motivation

The Trebuchet API exposes the complete functionality of the Cherwell system, which can be overwhelming and confusing. Furthermore, the optimal methods for achieving desired functionality are not obvious. This library is intended to expose a digestible, easy-to-use API that facilitates a specific set of operations - reading and writing data - for client-defined types, for which the underlying Trebuchet sequences have been refined and optimized to maximize performance and correctness.

## Feature Summary

- Internally optimized to use most efficient data read methods.
- Acts as an ORM, exposing CRUD operations that take client-defined types.
- Fluent querying API for readable and easy-to-write query logic, including expression parsing, on client-defined types.
- Allows rich, client-defined types to be mapped to a Cherwell schema through attributing.
- Graph support through include, handler and linking APIs.
- Provides hooks for injecting client-defined data writing behavior.
- Provides a dynamic data querying API.
- Provides interfaces at each seam to allow for easier unit-testing of client application code.

## Installation

This library is available through nuGet. However, an *important* caveat: it requires access to the Trebuchet suite of .dlls provided by Cherwell and those dlls are not provided.The executing application does not need to reference the Trebuchet dlls but they must be placed in the bin folder. If you have Cherwell installed on your development machine the following can be added to your executing project as a post-build step. If you happen to have the dlls in a different directory this script can be modified to point anywhere the Cherwell dlls are located and it will only bring in the dlls needed by CsmMagic:

```
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\htn.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\tern.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\GenuineChannels.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\ICSharpCode.SharpZipLib.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\Trebuchet.API.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\Trebuchet.BusinessLogic.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\Trebuchet.Communication.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\Trebuchet.Core.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\Trebuchet.Database.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\Trebuchet.Presentation.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\Trebuchet.Remoting.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\Trebuchet.Security.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\Trebuchet.SharedDefs.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\Trebuchet.Utility.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\DevExpress.RichEdit.v14.1.Core.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\HtmlAgilityPack.dll" "$(TargetDir)"
xcopy /Q /Y  "C:\Program Files (x86)\Cherwell Browser Applications\CherwellService\bin\Microsoft.Practices.Unity.dll" "$(TargetDir)"


```

Currently CsmMagic only works against the Cherwell 5.11 version.

## API Reference

The entry point of the API is the ICsmClientFactory. This exposes a single method, GetCsmClient, that returns the prime mover of the library - the ICsmClient.

The ICsmClient implementation included in the library performs actions such as authenticating with your Cherwell instance and ensuring that thread principals are correctly set. The interface exposes the following operations:

- Create
- Delete
- Update
- GetQuery
- ExecuteQuery
- Link

Operations are performed on two types of data - subclasses of the `BusinessObjectModel` abstract class; or the `ArbitraryCsmBusinessObject` class. Subclasses of `BusinessObjectModel` use decorated properties to map to the Cherwell blueprint of the business object they represent - these can be tailored specifically to your application needs, and will only query for data that has been explicitly described via the aforementioned decorated properties. The `ArbitraryCsmBusinessObject` class represents entire Cherwell business objects as key/value pairs. They are significantly simpler to work with, but do not form a solid foundation for an application that consumes Cherwell.

There are two attributes that are used in defining a sublcass of `BusinessObjectModel` are the `Field` and `Relationship` attributes. 

### Field

`Field`s are used to describe fields within a Cherwell schema. They are intended to decorate primitive-type properties, such as `string`s and `bool`s. A `Field` attribute has two constructor parameters - `Name`, and `isWriteable`. 

#### Name
`Name` must be the internal name or display name of the corresponding field in Cherwell - the field from which data will be taken to fill the decorated property during a query, or into which data will be written. If the `Name` parameter is not provided, the code name of the decorated property will be used.

#### isWriteable
`isWriteable` is an optional bool that defaults to `true`. `isWriteable` can be used to restrict a field to being read-only - if it is set to false, the property will be read from Cherwell during a query, but its value will not be written back to Cherwell during a write.

### Relationship

`Relationship`s are used to describe relationships between business objects in Cherwell. The relationship attribute has a single constructor parameter, `Name`, that is identical to the `Name` constructor parameter of `Field` - however, the relationship name must always be explicitly specified.

### Querying

Querying for data from Cherwell is performed through the fluent `ICsmQuery<T>` interface. Calling `GetQuery<T>` on an implementation of `ICsmClient` will return the corresponding typed implementation of `ICsmQuery`. The generic type arguments are constrained to subclasses of `BusinessObjectModel`. A separate, superficially similar API is provided for `ArbitraryCsmBusinessObject`s. `ExecuteQuery` takes an `ICsmQuery<T>` and returns the results set. `ICsmQuery` exposes the following querying methods, which can be used in combination. See the Code Example section for more details on how to query.

- ForSingleRecord
- ForChildren<TChild>
- ForRecId
- Where
- Include<TRelated>
- WhereRelated<TRelated>

#### ForSingleRecord

Cherwell queries default to returning sets of data. Calling this method will perform optimizations related to querying for a single item.

#### ForChildren<TChild>

This converts the query from type <T> to type <TChild>. This method takes an `Expression` that points to a collection of <TChild> - this must be used with properties that have been decorated with the `Relationship` attribute.

#### ForRecId

A superset of ForSingleRecord, this specifies that the query is for the business object record identified by the provided Cherwell RecId.

#### Where

This method returns another fluent interface, the `ICsmQueryClause`, that allows multiple query predicates to be added to the query. These predicates can be inclusive or exclusive, and support semantics such as "Equal", "Greater Than", "Contains", etc. These predicates must be based on properties that have been decorated with `Field`.

#### Include<TRelated>

This method, much like `ForChildren<TChild>`, takes an expression that points to a <TRelated> type property. `Include` is used to return navigational data properties in queries.

#### WhereRelated<TRelated>

This complex querying method takes an expression and a `Where` predicate - the first expression points to a related property (decorated with `Relationship`), and the `Where` is called against that navigational item.

### Linking

The `LinkChildToParent<TChild, TParent>` and `LinkOneToOne<TOne, TOther>` methods link business objects together in Cherwell. Both methods take an expression that points to a property, decorated by `Relationship`, and create the entry in that specific relationship. The primary distinction between the two methods is that `LinkOneToOne` will *overwrite* any existing data in that relationship, whereas `LinkChildToParent` will simply add the new object to the relationship. This method is most commonly used after `Create`ing a new object in Cherwell - although some times calling it is unnecessary. Clients will have to judge if making this call in their specific business case is required.

### Injecting custom behavior

CsmMagic makes use of classes called `BusinessObjectHandlers` to determine what it should do when given instructions about a specific kind of business object. When CsmMagic is told to write data to Cherwell, it first searches for a custom BusinessObjectHandler; failing to find one, it will defer to the default handler. The default handler is sufficient for simple cases, but if (for example) your application's models describe an object graph with relationships to objects which require specific logic on instantiation or deletion, then custom handlers should be written and associated with those types to ensure that logic is executed on each write.

#### Writing a custom handler

Writing a custom handler involves subclassing `BaseBusinessObjectHandler<T>` in the CsmMagic.Handlers namespace. This class provides overrides for the Create, Update, and Delete methods exposed by the `ICsmClient` interface. These overrides provide an `IHandlerClient` - which is identical to `ICsmClient` -  as a method parameter which is automatically injected by CsmMagic during execution. Thus, the full expressiveness and power of the `ICsmClient` can be used in these overrides - including querying, linking, and so on.

#### Registering a custom handler with CsmMagic

If you have a type `Incident`, and you write an `IncidentHandler : BaseBusinessObjectHandler<Incident>`, registering the handler with CsmMagic involves using the `HandledBusinessObjectAttribute` to decorate your `Incident` class. The usage is as follows:

```
[HandledBusinessObject(typeof(IncidentHandler))]
public class Incident 
{
    [...]
}
```

CsmMagic will then delegate to the overridden functionality whenever an `Incident` is created, updated, or deleted. If a type that is *not* correct is provided to the attribute - for example, if `IncidentHandler` did not subclass correctly, or if an entirely unrelated type is provided - an exception will be thrown at runtime.

## Code Examples

### Connecting to the API with the CsmClient

The CsmClient requires a connection to Cherwell, a username, and a password. The cherwell connection must exist in the client machine's `Connections.xml` file, and is identified in the application by the name of the connection element. The CsmClientFactory is configured with the following client application config elements, which are appended to your application config by nuGet:

```
. . .
<configSections>
	<section name="CsmMagic" type="CsmMagic.Config.CsmMagicConfiguration, CsmMagic" />
</configSections>
 . . .
<CsmMagic> 
	<CherwellConnection userName="username" password="password" connectionName="connectionName" />
</CsmMagic>
. . .
```

Several classes are provided with the library to assist in this configuration. The `CsmMagicConfiguration` class will pick up on the `<CsmMagic>` element and create a `CherwellConnectionConfigElement` instance, which in turn will read the username, password, and connectionName attributes. Finally, the `CsmClientConfiguration` class will either find the CsmMagicConfiguration by itself (paramaterless constructor) or take the three required strings as parameters. The `CsmClientFactory` takes in a `CsmClientConfiguration` in *its* constructor, and uses it to build `CsmClient`s.

```
namespace Example
{
	public class CsmMagicClient
	{
		protected readonly ICsmClientFactory ClientFactory;

		/// <summary>
		/// This constructor relies on the CsmClientConfiguration to locate its required configuration elements
		/// </summary>
		public CsmMagicClient() 
		{
			var configuration = new CsmClientConfiguration();
			ClientFactory = new CsmClientFactory(configuration);
		}

		/// <summary>
		/// Whereas this constructor makes use of the three-string overload to pass in the configuration details from a higher level
		/// </summary>
		public CsmMagicClient(string username, string password, string connectionName)
		{
			var configuration = new CsmClientConfiguration(username, password, connectionName);
			ClientFactory = new CsmClientFactory(configuration);
		}
	}
}
```


### Subclassing `BusinessObjectModel`

The following is a simplified, annotated example of representing the `Incident` business object as a subclass of `BusinessObjectModel`, so that it can be used in application code.

```
using System.Collections.Generic;
using CsmMagic.Attributes;
using CsmMagic.Models;

namespace Example
{
    public class CsmIncident : BusinessObjectModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CsmIncident"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor of BusinessObjectModel takes in the business object type name as defined in Cherwell - which in this case is "Incident"
    	/// </remarks>
        public CsmIncident() : base("Incident")
        {
        }

        /// <summary>
        /// The user-name of the Cherwell user who closed the incident, if the incident has been closed
        /// </summary>
        /// <remarks>
        /// Notice that the 'name' parameter has been explicitly set, as it differs from the property name
        /// </remarks>
        [Field(name: "ClosedBy")]
        public string ClosedByUserName { get; set; }

        /// <summary>
        /// The collection of journal entries associated with this incident
        /// </summary>
        /// <remarks>
        /// This property can be used in querying with Include or ForChildren
        /// CsmJournal is another subclass of BusinessObjectModel
        /// </remarks>
        [Relationship(name: "Incident Owns Journals")]
        public IEnumerable<CsmJournal> Journals { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <remarks>
        /// The 'name' parameter of the Field attribute is left as to the default, since 'Description' is also the internal name of the field in Cherwell
        /// </remarks>
        [Field]
        public string Description { get; set; }

		[Field]        
        public string IncidentID { get; set; }
    }
}
```

### Querying

An example of querying for an incident by a field on the object:

```        
public CsmIncident GetIncident(string incidentId)
{
    using (var csmClient = ClientFactory.GetCsmClient())
    {
        var query = csmClient.GetQuery<CsmIncident>() // This creates a query for our data type
            .ForSingleRecord() // This specifies that we only want one record
            .Include(i => i.Journals) // This will bring back all the CsmJournals that are associated with incidents returned by this query as data within the Journals property
            .Where(i => i.IncidentID == incidentId); // This predicate queries on the IncidentID column, finding the record where the column value matches our data
        // ExecuteQuery always returns an IEnumerable<T>, even though we called ForSingleRecord, so we get the single item
        var csmIncident = csmClient.ExecuteQuery(query).FirstOrDefault();
        return csmIncident;
    }    
}
```

### Creating and linking objects

An example of creating a new journal entry, and associating it with an incident:

```        
public void CreateJournal(CsmJournal journal) // CsmJournal subclasses BusinessObjectModel, as with CsmIncident
{
    using (var csmClient = ClientFactory.GetCsmClient())
    {
    	// Get the specific incident we want to associate this journal with    	
        var parentIncident = csmClient.ExecuteQuery(csmClient.GetQuery<CsmIncident>().ForRecId(journal.OwningRecId)).FirstOrDefault();
        if (parentIncident == null)
        {
            throw new IncidentNotFoundException(journal.OwningRecId);
        }

        csmClient.Create(csmJournal);
        // LinkChildToParent takes an expression that points to the [Relationship]-decorated property, the new child object, and the RecID of the parent
        csmClient.LinkChildToParent<CsmJournal, CsmIncident>(incident => incident.Journals, csmJournal, journal.OwningRecId);
    }
}
```

### Creating and using custom handlers

An example of putting the same functionality inside a custom handler:

```
public class JournalHandler : BaseBusinessObjectHandler<CsmJournal>
{    
    public override void Create(CsmJournal journal, IHandlerClient<CsmJournal> client)
    { 
        var parentIncident = client.ExecuteQuery(client.GetQuery<CsmIncident>().ForRecId(journal.OwningRecId)).FirstOrDefault();
        if (parentIncident == null)
        {
            throw new IncidentNotFoundException(journal.OwningRecId);
        }

        client.Create(csmJournal);
        client.LinkChildToParent<CsmJournal, CsmIncident>(incident => incident.Journals, csmJournal, journal.OwningRecId);
    }
}

```

Then, decorate your model class with the appropriate attribute:

```
[HandledBusinessObject(typeof(JournalHandler))]
public class CsmJournal 
{
    [...]
}
```

Then, your code's `Create` method can simply call the client, and your custom behavior will be invoked by the CsmClient:

```        
public void CreateJournal(CsmJournal journal)
{
    using (var csmClient = ClientFactory.GetCsmClient())
    {
        csmClient.Create(journal);
    }
}
```

## Authors and Contributors

Authors:
@jbloom
@nvanderende

Contributors:
@jmartin
@devans
@myiannakakis

## License

The MIT License (MIT)

Copyright (c) 2015 John Bloom and Nicholas Vander Ende

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.