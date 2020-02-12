<p align="center">
    <img alt="Bijou.js" src="logo.svg" width="300"/>
</p>

<p align="center">
    <a href="https://lbesson.mit-license.org/">
        <img alt="MIT" src="https://img.shields.io/badge/License-MIT-blue.svg" height="20"/>
    </a>
</p>

# Bijou.js

**Bijou.js** is an open-source JavaScript runtime environment, built with [Chakra](https://github.com/microsoft/ChakraCore) in .Net/C#, and aimed at being embedded into UWP applications.

The name is inspired by our favourite [pizzeria in Paris](https://bijou-paris.fr/), that literally nourished this project. 

## How to use it 
### Add it to your project
* Clone ths repository and open [Bijou.js's solution file](src/Bijou.sln) in Visual Studio 2019
* Compile and copy the generated binaries in your project
* In your project, create a new JS executor
```cs 
var engine = new UWPChakraHostExecutor();
```

### Load and run a script
Bijou.js can load and execute JS scripts using `RunScriptAsync` method

```cs 
var engine = new UWPChakraHostExecutor();
var result = await engine.RunScriptAsync(@"
                function square() { return 10 * 10; }
                square();
            ");
result = await engine.RunScriptAsync(new Uri("path/to/a/script.js"));
```

### Call a JS function from C#
Using `CallFunctionAsync` method it is possible to call functions in the global js object

```cs 
var engine = new UWPChakraHostExecutor();
// First, load a script declaring a global function
var result = await engine.RunScriptAsync(@"
                function square() { 
                    return 10 * 10; 
                }
            ");
// Then, call it
await engine.CallFunctionAsync("square"));
```
Let's have more fun, let's pass some parameters to a JS function
```cs 
var engine = new UWPChakraHostExecutor();
// First, load a script declaring a global function
var result = await engine.RunScriptAsync(@"
                function multiply(a, b) {
                    return a * b; 
                }
            ");
// Then, call it passing parameters
await engine.CallFunctionAsync("multiply", 3, 2));
```
This let you send data from C# to JS
```cs 
var engine = new UWPChakraHostExecutor();
// First, load a script declaring a global function
var result = await engine.RunScriptAsync(@"
                function sendMessage(message) {
                    // do something with the message
                    console.log(message); 
                }
            ");
// Then, call it passing parameters
await engine.CallFunctionAsync("sendMessage", "Hello from C#"));
```
### Sending messages from JS to C#
Bijou.js implements `sendToHost` API in JS, that lets sending messages from JS to C#. Calls to `sendToHost` trigger a `OnMessageReceived` event in C#. By listening to `OnMessageReceived` on the `UWPChakraHostExecutor` instance, C# code can receives calls from JS.
Let's see this in an example  
```cs 
var engine = new UWPChakraHostExecutor();
var messageReceived = new AutoResetEvent(false);
var timeout = TimeSpan.FromSeconds(1);
var reply = String.Empty;
// First, register to the OnMessageReceived event
engine.OnMessageReceived += (sender, msg) => {
                    reply = msg;
                    messageReceived.Set();
                };
// Then load a cool JS function 
var result = await engine.RunScriptAsync(@"
                function coolJSFunction(message) {
                    // do something with the message
                    const reply = message + ' from JS'
                    //
                    sendToHost(reply) 
                }
            ");
// Now call it passing parameters
await engine.CallFunctionAsync("coolJSFunction", "Hello"));
// Finally, wait for the reply
messageReceived.WaitOne(timeout);
Debug.WriteLine(reply);
```
### Bijou.JS JavaScript API
Bijou.JS is based on [Chakra](https://en.wikipedia.org/wiki/Chakra_(JavaScript_engine)), and therefor is ES6 compatible. 
On top of that it implements common non-ES6 JavaScript APIs, here's a complete list
*  Timers (`setTimeout`, `setInterval`, `clearTimeout`, `clearInterval`)
* XmlHttpRequest
* console

It also provide a custom FileSystem API and encryption APIs
