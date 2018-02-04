# Data Object Notation (DON)
## What is DON?
DON is a lightweight object model that is human readable, extremely fast to parse, and uses a very minimal rule set. It's as if you took a bit of JSON and combined it with XML.

## Why use DON?
There are plenty of other fully featured options such as XML, JSON, and YAML. Why use DON?

Why have more flavors than chocolate or vanilla? Well... sometimes you're in the mood for chocolate and vanilla swirled together. Sometimes a new flavor sensation is born with the right combination of old and new.

## Why was DON created?
JSON has a good notion of objects (key value pairs) and arrays (lists of objects). However, it's easy to make a syntax error mistake. We've all forgotten a quotation, colon, or comma when moving data around.

XML has attributes that tie very nicely with a named parent tag. However opening and closing tags is verbose and lists of things is repetitive.

YAML is the nicest markup language to read but it also is error prone to forgetting a dash, indent, or colon.

DON was created to minimize special characters while keeping object array syntax from JSON, attributes from XML, and human readability from YAML. Parsing should also be extremely fast (single pass) and dead simple.

## What's the catch?
* DON is very new and isn't fully featured.
* It's only parser implementation is in C#.
* Data types in DON are all strings and require runtime reflection to convert (or break) into something more usable.
* Feeding bad input into the parser doesn't give you a nice error message you telling you exactly where the problem is.

Now with that out of the way...

## What does DON look like?
### List of names:

    Alpha, Beta, Charlie, Delta
or with no commas on newlines

    Alpha
    Beta
    Charlie
    Delta
Yep you've written DON compliant data before!

### Book example:
An object such as a book can be described completely with DON Properties (Key/Value pairs) enclosed between a `(` and a `)`.

    (
        ISBN= 978-3-16-148410-0
        Name= A book title without quotes
        Author= John Doe
        Price= 12.50
        Description= Intelligently determining characters such as ()"= don't need to be escaped.
    )
Note: Spaces and tabs after the equals but before the first character are ignored. Keys can contain spaces (not recommended) but not equals.

### Compact objects:
Objects can be described compactly in a single line but need to be escaped if containing a `,` or `)`. Escaping any compact text can be done with double bars `||`.

    (Width=4,Color=Blue,Function=||Foo()||,Text=||Sir, I bring news!||)
What about a list of compact objects?

    Square(Width=2,Height=2,Color=Red)
    Circle(Radius=9,Color=Green,Weight=9.5)
    Triangle(Base=3,Height=7,Color=Yellow)

Objects in a list are prefixed with a name. For this example they aren't useful but the next example will make more sense.

### Employee example:
Properties as you can see are simple key value pairs. Nesting complex objects in them would break the readability. Instead a new object should be created as a child which is anything between `{` and `}`

    (Name=John Doe,Age=36,Height=5.6)
    {
        Skills
        {
            Reading
            Writing
            Arithmetic
        }
        Spouse(Name=Jane Doe,Age=37,Height=5.4)
        Children
        {
            Tim(Name=Tim,Age=3)
            Alice(Name=Alice,Age=6)
        }
    }

This structure can be mapped directly to the following C# classes

    public class Person
    {
        public string Name;
        public int Age;
        public float Height;
    }

    public class Employee : Person
    {
        public List<string> Skills;
        public Person Spouse;
        public List<Person> Children;
    }

As you can see children can represent a single object or an array of objects and you can nest them freely. Nice!

## How do I use the C# library?
Grab the 2 source files or wait for a Nuget package to be available.

    using DataObjectNotation;
    
    var dataObject = DON.Parse(donString);
    var myClass = dataObject.Deserialize<MyType>();

    // Use myClass as strongly typed instance

## What next?
Take a look at the code for DON.Parse() it's only slightly longer than 100 lines! Check out the unit tests for more examples. Extend it, add some functionality and send a pull request!