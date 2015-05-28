  * [Install Visual C# Express](http://msdn.microsoft.com/vstudio/express/visualcsharp/) (sorry), which will automatically install the right framework.
  * Open the solution file and build.

This process has a distinct disadvantages: People who just want to build without compiling have to download the bulky Visual C# Express. NAnt seems to support solution files however from a cursory glance at their `examples/Solutions` directory as long as you have the right framework number (3.5 and thereabouts). Excerpt from that directory:

```
<target name="build.winforms">
    <solution configuration="${configuration}" solutionfile="WinForms.sln">
    </solution>
    <property name="expected.output" value="bin/${configuration}/WinForms.exe"/>
    <fail unless="${file::exists(expected.output)}">Output file doesn't exist in ${expected.output}</fail>
</target>
```

Apologies: I'm not a well-versed NAnt user, and I don't have the time to maintain such a file.