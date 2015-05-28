Let the football be the metaphor for a new IRC message.

  * [Quirk](http://theminds.googlecode.com/svn/trunk/Quirk/Quirk.cs) gets the football and passes it off to an assigned [Buffer instance](http://theminds.googlecode.com/svn/trunk/Theminds/Buffer.cs) via the `NewLine` event
  * Buffer passes the football to its `NewLine` event, which many classes with the `DesiresAppControls` attribute have filled
  * Each class does what it wants and changes the line on each pass via `ref` keyword magic
  * At least one class also assigns the line to the right LogBox [via BufferData,](http://theminds.googlecode.com/svn/trunk/Theminds/Bufferlings.cs) which maps to `Room` via the Dictionary `proust`, via knowing the right server and channel
  * A class may also assign the text a color or, hopefully in the future, colors
  * [Buffer adds it to the LogBox](http://theminds.googlecode.com/svn/trunk/Theminds/Buffer.cs)
  * Recurse forever, holding hands and lips
